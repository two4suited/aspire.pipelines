using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspire.Hosting.Pipelines;
using Microsoft.Extensions.Logging;

namespace AppHost
{
    public static partial class SetupExtensions
    {
        /// <summary>
        /// Prepends a prefix to each item in the list. If the list is null, returns an empty list.
        /// </summary>
        public static List<string> PrependResourceNameWithDash(this List<string>? list, string prefix)
        {
            return list?.Select(item => $"{prefix}-{item}").ToList() ?? [];
        }
        public static List<string> PrependResourceNameWithDot(this List<string>? list, string prefix)
        {
            return list?.Select(item => $"{prefix}.{item}").ToList() ?? [];
        }

        #pragma warning disable ASPIREPIPELINES001
        public static IDistributedApplicationBuilder WithSetup(this IDistributedApplicationBuilder builder)
        {
            builder.Pipeline.AddStep("setup", context =>
            {
                context.Logger.LogInformation("Installation step completed successfully.");
                return Task.CompletedTask;
            });

            return builder;
        }
        #pragma warning restore ASPIREPIPELINES001
    }
    
}