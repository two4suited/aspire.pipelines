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
        public static IResourceBuilder<SetupResource> WithServiceDefaults(
            this IResourceBuilder<SetupResource> builder,
            string name, List<string>? DependsOn = null)
        {
             return builder.WithPipelineStepFactory(factoryContext =>
             {
                var resource = factoryContext.Resource;
                var projectDirectory = $"{resource.Name.ToLower()}.servicedefaults";
                var logger = factoryContext.PipelineContext.Logger;

                 // Prepend resource.Name- to DependsOn values if provided
                 var dependsOnSteps = DependsOn.PrependResourceNameWithDash(resource.Name);

                 return new PipelineStep
                 {
                     Name = $"{resource.Name}-{name}",
                     Action = async (context) =>
                     {
                         logger.LogInformation("Creating Aspire service defaults for {Name}...", name);
                         
                         var processStartInfo = new System.Diagnostics.ProcessStartInfo
                         {
                             FileName = "dotnet",
                             Arguments = $"new aspire-servicedefaults -n {projectDirectory}",
                             WorkingDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..")),
                             UseShellExecute = false,
                             RedirectStandardOutput = true,
                             RedirectStandardError = true
                         };
                         
                         var process = System.Diagnostics.Process.Start(processStartInfo);
                         if (process != null)
                         {
                             var output = await process.StandardOutput.ReadToEndAsync();
                             var error = await process.StandardError.ReadToEndAsync();
                             await process.WaitForExitAsync();
                             
                             if (process.ExitCode == 0)
                             {
                                 logger.LogInformation("Service defaults created successfully for {Name}.", name);
                             }
                             else
                             {
                                 logger.LogWarning("Failed to create service defaults for {Name}. Error: {Error}", name, error);
                             }
                         }
                         else
                         {
                             logger.LogError("Failed to start dotnet process for creating service defaults.");
                         }
                     },
                     RequiredBySteps = ["setup"],
                    DependsOnSteps = dependsOnSteps
                 };
             });
        }
        #pragma warning restore ASPIREPIPELINES001
    }
}
