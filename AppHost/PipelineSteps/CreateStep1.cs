#pragma warning disable ASPIREPIPELINES001
#pragma warning disable ASPIREINTERACTION001
using Aspire.Hosting;
using Aspire.Hosting.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppHost.PipelineSteps;

public static class CreateStep1
{
    public static IDistributedApplicationBuilder AddCreateStep1(this IDistributedApplicationBuilder builder)
    {
        builder.Pipeline.AddStep(PipelineStepNames.CreateStep1.ToStepName(), async context =>
        {
            // Get logger
            var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(nameof(CreateStep1));
            
            // Check if the setup step wants Step 1 to run
            var runStep1Env = Environment.GetEnvironmentVariable("ASPIRE_RUN_STEP1");
            var runStep1 = bool.TryParse(runStep1Env, out var shouldRun) && shouldRun;
            
            if (!runStep1)
            {
                logger.LogInformation("Step 1 skipped (not enabled in setup).");
                return;
            }
            
            var repoRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".."));
            var filePath = Path.Combine(repoRoot, "step1.txt");
            
            await File.WriteAllTextAsync(filePath, $"Step 1 executed at {DateTime.Now}");
            
            logger.LogInformation("Created {FilePath}", filePath);
        },dependsOn: PipelineStepNames.Driver.ToStepName());
        
        return builder;
    }
}

