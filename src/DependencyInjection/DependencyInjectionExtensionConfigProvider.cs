using System;
using Microsoft.Azure.WebJobs.Host.Config;

namespace Prabang.Azure.DependencyInjection
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DependencyInjectionExtensionConfigProvider : IExtensionConfigProvider
    {
        private readonly InjectBindingProvider _injectBindingProvider;

        public DependencyInjectionExtensionConfigProvider(InjectBindingProvider injectBindingProvider)
        {
            _injectBindingProvider = injectBindingProvider;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var bindingAttributeBindingRule = context.AddBindingRule<InjectAttribute>();
            bindingAttributeBindingRule.Bind(_injectBindingProvider);

        }
    }
}
