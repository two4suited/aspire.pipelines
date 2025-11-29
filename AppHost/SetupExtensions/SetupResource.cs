using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aspire.Hosting.ApplicationModel;

    public sealed class SetupResource(string name): CustomResource(name)   {}

    public static class SetupResourceExtensions
    {
        public static IResourceBuilder<SetupResource> AddProjectSetup(
            this IDistributedApplicationBuilder builder,
            string name)
        {
            var resource = new SetupResource(name);
            return builder.AddResource(resource);
        }
    }
