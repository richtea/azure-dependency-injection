# azure-dependency-injection
Autofac dependency injection for Azure V2 functions, loosely based on Jason Roberts' https://github.com/introtocomputerscience/azure-function-autofac-dependency-injection package.

## Usage

### Configuration

Add a startup class to your Function App assembly that implements `IWebJobsStartup`, and call `builder.AddDependencyInjection()` from within the `Configure` method.

```c#
using Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Prabang.Azure.DependencyInjection;

[assembly: WebJobsStartup(typeof(Startup))]

namespace MyApp
{
    internal class Startup : IWebJobsStartup
    {
        public Startup()
        {
            _myLoggerFactory = new MyLoggerFactory();
        }

        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddDependencyInjection(
                ConfigureServices,
                ConfigureAutofacRegistrations,
                ConfigureScopeRegistrations);
        }

        private void ConfigureAutofacRegistrations(ContainerBuilder cb)
        {
            var assembly = Assembly.GetExecutingAssembly();
            cb.RegisterAssemblyModules(assembly);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory>(_ => _myLoggerFactory);
        }

        // This method is called when creating a lifetime scope for the method call. It
        // provides an opportunity to add additional registrations that are unique to
        // the function invocation.
        private void ConfigureScopeRegistrations(ContainerBuilder cb)
        {
            cb.RegisterType<MyClass>().As<IMyClass>().InstancePerLifetimeScope();
        }
    }
}
```

Note that the `ConfigureServices` method is called before the `ConfigureAutofacRegistrations` method.

### Injection

To inject a dependency into your Azure Function method, decorate the parameter with an `InjectAttribute`:

```c#
[FunctionName("Test")]
public static async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req,
    [Inject] IHttpSecurityContext securityContext,
    ILogger log)
{
}
```

In the above example, the `securityContext` parameter will be injected with the configured `IHttpSecurityContext` dependency.