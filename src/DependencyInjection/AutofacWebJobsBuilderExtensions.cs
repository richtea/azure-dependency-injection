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
        /// <param name="configureAutofacServices"></param>
        /// <param name="configureCoreServices"></param>
        /// <param name="configureScopedAutofacServices"></param>
        public static IWebJobsBuilder AddDependencyInjection(this IWebJobsBuilder builder,
            Action<ContainerBuilder> configureAutofacServices, Action<IServiceCollection> configureCoreServices = null,
            Action<ContainerBuilder> configureScopedAutofacServices = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            //builder.Services.AddSingleton<IServiceProviderBuilder>(_ => new AutofacServiceProviderBuilder(b => { }));
            AddCommonDependencyInjection(builder, configureAutofacServices ?? (_ => { }),
                configureCoreServices ?? (_ => { }), configureScopedAutofacServices ?? (_ => { }));

            return builder;
        }


        private static void AddCommonDependencyInjection(IWebJobsBuilder builder,
            Action<ContainerBuilder> configureServices, Action<IServiceCollection> configureCoreServices, Action<ContainerBuilder> configureScopedAutofacServices)
        {
            configureCoreServices(builder.Services);

            builder.Services.AddSingleton<ServiceProviderHolder>();
            builder.Services.AddSingleton<InjectBindingProvider>();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IFunctionFilter, ScopeCleanupFilter>());

            var cb = new ContainerBuilder();
            cb.Populate(builder.Services);
            configureServices(cb);
            cb.Register(_ => configureScopedAutofacServices);

            var container = cb.Build();
            builder.Services.AddSingleton(container);

            builder.AddExtension<DependencyInjectionExtensionConfigProvider>();
        }
    }
}
