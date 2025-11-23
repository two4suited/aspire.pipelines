#pragma warning disable ASPIREPIPELINES001
using Aspire.Hosting;
using Aspire.Hosting.Pipelines;

namespace AppHost.PipelineSteps;

public static class CreateServiceDefaultsStep
{
    public static IDistributedApplicationBuilder AddCreateServiceDefaultsStep(this IDistributedApplicationBuilder builder)
    {
        builder.Pipeline.AddStep("create-servicedefaults", async context =>
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".."));
            var serviceDefaultsProjectName = "ServiceDefaults";
            var serviceDefaultsProjectPath = Path.Combine(repoRoot, serviceDefaultsProjectName);
            
            // Create the aspire-servicedefaults project
            var createProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"new aspire-servicedefaults -n {serviceDefaultsProjectName} -o {serviceDefaultsProjectPath}",
                WorkingDirectory = repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            
            if (createProcess != null)
            {
                await createProcess.WaitForExitAsync();
            }
            
            // Add the project to the solution
            var slnPath = Path.Combine(repoRoot, "aspire.pipelines.sln");
            var addProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"sln {slnPath} add {Path.Combine(serviceDefaultsProjectPath, $"{serviceDefaultsProjectName}.csproj")}",
                WorkingDirectory = repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            
            if (addProcess != null)
            {
                await addProcess.WaitForExitAsync();
            }
        }, requiredBy: "create-api");
        
        return builder;
    }
}

