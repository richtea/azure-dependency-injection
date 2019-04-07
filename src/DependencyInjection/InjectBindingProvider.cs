using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;

namespace Prabang.Azure.DependencyInjection
{
    internal class InjectBindingProvider : IBindingProvider
    {
        private readonly ServiceProviderHolder _serviceProviderHolder;
        private readonly ILogger _logger;

        public InjectBindingProvider(ServiceProviderHolder serviceProviderHolder, ILoggerFactory loggerFactory)
        {
            _serviceProviderHolder = serviceProviderHolder;
            _logger = loggerFactory.CreateLogger(LogCategories.CreateFunctionUserCategory("Startup"));
        }

        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            _logger.LogInformation("Creating InjectBinding for parameter of type {Type}", context.Parameter.ParameterType.AssemblyQualifiedName);
            try
            {
                IBinding binding = new InjectBinding(_serviceProviderHolder, context.Parameter.ParameterType);
                return Task.FromResult(binding);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating InjectBinding: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}