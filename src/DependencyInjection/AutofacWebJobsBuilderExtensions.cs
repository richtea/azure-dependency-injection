using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Prabang.Azure.DependencyInjection
{
    public static class AutofacWebJobsBuilderExtensions
    {
        /// <summary>
        /// Adds Autofac dependency injection extension to the provided <see cref="IWebJobsBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/> to configure.</param>
        /// <param name="configureServices">A delegate to register services by using the standard Microsoft services interface.</param>
        /// <param name="configureAutofacRegistrations">A delegate to register dependencies directly with the Autofac container.</param>
        /// <param name="configureScopedAutofacRegistrations">A delegate to register Autofac dependencies at the lifetime scope level (i.e. per function invocation).</param>
        public static IWebJobsBuilder AddDependencyInjection(this IWebJobsBuilder builder,
            Action<IServiceCollection> configureServices = null,
            Action<ContainerBuilder> configureAutofacRegistrations = null,
            Action<ContainerBuilder> configureScopedAutofacRegistrations = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            //builder.Services.AddSingleton<IServiceProviderBuilder>(_ => new AutofacServiceProviderBuilder(b => { }));
            AddCommonDependencyInjection(builder,
                configureServices ?? (_ => { }),
                configureAutofacRegistrations ?? (_ => { }),
                configureScopedAutofacRegistrations ?? (_ => { }));

            return builder;
        }


        private static void AddCommonDependencyInjection(IWebJobsBuilder builder,
            Action<IServiceCollection> configureCoreServices, 
            Action<ContainerBuilder> configureAutofacRegistrations, 
            Action<ContainerBuilder> configureScopedAutofacRegistrations)
        {
            configureCoreServices(builder.Services);

            builder.Services.AddSingleton<ServiceProviderHolder>();
            builder.Services.AddSingleton<InjectBindingProvider>();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IFunctionFilter, ScopeCleanupFilter>());

            var cb = new ContainerBuilder();
            cb.Populate(builder.Services);
            configureAutofacRegistrations(cb);
            cb.Register(_ => configureScopedAutofacRegistrations);

            var container = cb.Build();
            builder.Services.AddSingleton(container);

            builder.AddExtension<DependencyInjectionExtensionConfigProvider>();
        }
    }
}
