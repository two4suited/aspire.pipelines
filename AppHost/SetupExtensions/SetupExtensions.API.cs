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
        #pragma warning disable ASPIREPIPELINES001
        public static IResourceBuilder<SetupResource> WithAPI(
            this IResourceBuilder<SetupResource> builder,
            string name, List<string>? DependsOn = null, List<string>? References = null)
        {
             return builder.WithPipelineStepFactory(factoryContext =>
             {
                var resource = factoryContext.Resource;
                var logger = factoryContext.PipelineContext.Logger;

                 // Prepend resource.Name- to DependsOn values if provided
                 var dependsOnSteps = DependsOn.PrependResourceNameWithDash(resource.Name);
                 
                 // Prepend resource.Name- to References if provided
                 var referencePaths = References?.Select(ref_ => $"{resource.Name.ToLower()}.{ref_.ToLower()}/{resource.Name.ToLower()}.{ref_.ToLower()}.csproj").ToList() ?? [];

                 return new PipelineStep
                 {
                     Name = $"{resource.Name}-{name}",
                     Action = async (context) =>
                     {
                         logger.LogInformation("Creating Web API project for {Name}...", name);
                         
                         var processStartInfo = new System.Diagnostics.ProcessStartInfo
                         {
                             FileName = "dotnet",
                             Arguments = $"new webapi -n {resource.Name.ToLower()}",
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
                                 logger.LogInformation("Web API project created successfully for {Name}.", name);
                                 
                                 // Add references if provided
                                 if (referencePaths.Count > 0)
                                 {
                                     foreach (var refPath in referencePaths)
                                     {
                                         logger.LogInformation("Adding reference {ReferencePath} to {Name}...", refPath, resource.Name);
                                         
                                         var refProcessInfo = new System.Diagnostics.ProcessStartInfo
                                         {
                                             FileName = "dotnet",
                                             Arguments = $"add {resource.Name.ToLower()}/{resource.Name.ToLower()}.csproj reference {refPath}",
                                             WorkingDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..")),
                                             UseShellExecute = false,
                                             RedirectStandardOutput = true,
                                             RedirectStandardError = true
                                         };
                                         
                                         var refProcess = System.Diagnostics.Process.Start(refProcessInfo);
                                         if (refProcess != null)
                                         {
                                             await refProcess.WaitForExitAsync();
                                             if (refProcess.ExitCode == 0)
                                             {
                                                 logger.LogInformation("Reference {ReferencePath} added successfully.", refPath);
                                             }
                                             else
                                             {
                                                 var refError = await refProcess.StandardError.ReadToEndAsync();
                                                 logger.LogWarning("Failed to add reference {ReferencePath}. Error: {Error}", refPath, refError);
                                             }
                                         }
                                     }
                                 }
                             }
                             else
                             {
                                 logger.LogWarning("Failed to create Web API project for {Name}. Error: {Error}", name, error);
                             }
                         }
                         else
                         {
                             logger.LogError("Failed to start dotnet process for creating Web API project.");
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
