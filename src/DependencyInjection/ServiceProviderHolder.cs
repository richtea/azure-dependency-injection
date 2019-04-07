using System;
using System.Collections.Concurrent;
using Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Prabang.Azure.DependencyInjection
{
    internal class ServiceProviderHolder
    {
        private readonly ConcurrentDictionary<Guid, ILifetimeScope> _scopes = new ConcurrentDictionary<Guid, ILifetimeScope>();
        private readonly IContainer _rootAutofacContainer;
        private readonly IOptions<ExecutionContextOptions> _executionContextOptions;
        private readonly ILogger _logger;

        public ServiceProviderHolder(IContainer rootAutofacContainer, IOptions<ExecutionContextOptions> executionContextOptions, ILoggerFactory loggerFactory)
        {
            _rootAutofacContainer = rootAutofacContainer;
            _executionContextOptions = executionContextOptions;
            _logger = loggerFactory.CreateLogger(LogCategories.CreateFunctionUserCategory("Startup"));
        }

        public void RemoveScope(Guid functionInstanceId)
        {
            if (_scopes.TryRemove(functionInstanceId, out var scope))
            {
                scope.Dispose();
            }
        }

        public object GetRequiredService(BindingContext bindingContext, Type serviceType)
        {
            _logger.LogInformation("Resolving required service with type {Type}", serviceType.AssemblyQualifiedName);

            // From here on we use the Autofac scope rather than the framework one, because Autofac supports registration while creating
            // nested scopes.
            var childScope = _scopes.GetOrAdd(bindingContext.FunctionInstanceId, id =>
            {
                _logger.LogDebug("Creating new scope for instance ID {InstanceID}", id.ToString());
                var context = CreateContext(bindingContext.ValueContext);
                var registerScoped = _rootAutofacContainer.Resolve<Action<ContainerBuilder>>();
                return _rootAutofacContainer.BeginLifetimeScope(bindingContext.FunctionInstanceId,
                    cb =>
                    {
                        registerScoped(cb);
                        cb.RegisterInstance(context);
                    });
            });
            try
            {
                return childScope.Resolve(serviceType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving required service: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        private ExecutionContext CreateContext(ValueBindingContext context)
        {
            var result = new ExecutionContext
            {
                InvocationId = context.FunctionInstanceId,
                FunctionName = context.FunctionContext.MethodName,
                FunctionDirectory = Environment.CurrentDirectory,
                FunctionAppDirectory = _executionContextOptions.Value.AppDirectory
            };

            if (result.FunctionAppDirectory != null)
            {
                result.FunctionDirectory = System.IO.Path.Combine(result.FunctionAppDirectory, result.FunctionName);
            }
            return result;
        }
    }
}