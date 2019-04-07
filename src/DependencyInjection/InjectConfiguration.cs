using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;

namespace Prabang.Azure.DependencyInjection
{
    internal class InjectConfiguration : IExtensionConfigProvider
    {
        public readonly InjectBindingProvider _injectBindingProvider;
        private readonly IConfiguration _configuration;

        public InjectConfiguration(InjectBindingProvider injectBindingProvider, IConfiguration configuration)
        {
            _injectBindingProvider = injectBindingProvider;
            _configuration = configuration;
        }

        public void Initialize(ExtensionConfigContext context) => context
            .AddBindingRule<InjectAttribute>()
            .Bind(_injectBindingProvider);
    }
}