using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aspire.Hosting.Pipelines;
using Microsoft.Extensions.Logging;

namespace AppHost
{
    public static partial class SetupExtensions
    {
        #pragma warning disable ASPIREPIPELINES001
        public static IResourceBuilder<SetupResource> WithCreateFile(
            this IResourceBuilder<SetupResource> builder,
            string name, List<string>? DependsOn = null)
        {
             DependsOn = DependsOn.PrependResourceNameWithDash(builder.Resource.Name);

             return builder.WithPipelineStepFactory(factoryContext =>
             {
                var resource = factoryContext.Resource;
                var logger = factoryContext.PipelineContext.Logger;

                 return new PipelineStep
                 {
                     Name = $"{resource.Name}-{name}",
                     Action = async (context) =>
                     {
                         var repoRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".."));
                         var filePath = Path.Combine(repoRoot, $"{name}.txt");
            
                         await File.WriteAllTextAsync(filePath, $"{name} executed at {DateTime.Now}");
                     },
                     RequiredBySteps = ["setup"],
                    DependsOnSteps = DependsOn
                     
                 };
             });
        }
        #pragma warning restore ASPIREPIPELINES001
    }
}
