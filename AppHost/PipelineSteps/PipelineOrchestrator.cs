#pragma warning disable ASPIREPIPELINES001
using System.IO.Pipelines;
using Aspire.Hosting;
using Aspire.Hosting.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppHost.PipelineSteps;

public static class PipelineOrchestrator
{
    public static IDistributedApplicationBuilder AddPipelineSteps(this IDistributedApplicationBuilder builder)
    {
        builder.Pipeline.AddStep(PipelineStepNames.Finisher.ToStepName(), async context =>
        {
        }, dependsOn: new[] { PipelineStepNames.Driver.ToStepName(), PipelineStepNames.CreateStep1.ToStepName(), PipelineStepNames.CreateStep2.ToStepName() });
        
        return builder
            .AddDriverStep()
            .AddCreateStep1()
            .AddCreateStep2();
    }
}

