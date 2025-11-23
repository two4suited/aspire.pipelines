#pragma warning disable ASPIREPIPELINES001
using Aspire.Hosting;
using Aspire.Hosting.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppHost.PipelineSteps;

public static class CreateStep2
{
    public static IDistributedApplicationBuilder AddCreateStep2(this IDistributedApplicationBuilder builder)
    {
        
        builder.Pipeline.AddStep(PipelineStepNames.CreateStep2.ToStepName(), async context =>
        {
            var runStep2Env = Environment.GetEnvironmentVariable("ASPIRE_RUN_STEP2");
            if (!bool.TryParse(runStep2Env, out var shouldRun) || !shouldRun)
            {
                return;
            }
            
            var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(nameof(CreateStep2));
            
            var repoRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".."));
            var filePath = Path.Combine(repoRoot, "step2.txt");
            
            await File.WriteAllTextAsync(filePath, $"Step 2 executed at {DateTime.Now}");
            
            logger.LogInformation("Created {FilePath}", filePath);
        },dependsOn: PipelineStepNames.Driver.ToStepName());
        
        return builder;
    }
}

