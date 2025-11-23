#pragma warning disable ASPIREPIPELINES001
using Aspire.Hosting;
using Aspire.Hosting.Pipelines;

namespace AppHost.PipelineSteps;

public static class OutputPipelineStep
{
    public static IDistributedApplicationBuilder AddOutputPipelineStep(this IDistributedApplicationBuilder builder)
    {
        builder.Pipeline.AddStep("output-pipeline", async context =>
        {
            var animals = new[] { "dog", "cat", "elephant", "lion", "tiger" };
            
            var yamlContent = "animals:\n" + string.Join("\n", animals.Select(a => $"  - {a}"));
            
            await File.WriteAllTextAsync("pipelines.yml", yamlContent);
        }, requiredBy: WellKnownPipelineSteps.Publish);
        
        return builder;
    }
}

