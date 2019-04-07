using System;
using System.Collections.Generic;
using System.Text;
using Autofac;

namespace Prabang.Azure.DependencyInjection
{
    /// <summary>
    /// Defines the interface of builder that creates an instance of an Autofac <see cref="ContainerBuilder"/>.
    /// </summary>
    public interface IAutofacContainerBuilder
    {
        /// <summary>
        /// Creates an instance of a <see cref="ContainerBuilder"/>.
        /// </summary>
        /// <returns></returns>
        ContainerBuilder Build();
    }
}
