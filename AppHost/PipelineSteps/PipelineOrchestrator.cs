#pragma warning disable ASPIREPIPELINES001
using System.IO.Pipelines;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppHost.PipelineSteps;

public static class PipelineOrchestrator
{
    public static IDistributedApplicationBuilder AddPipelineSteps(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ParameterResource> runStep1Param,
        IResourceBuilder<ParameterResource> runStep2Param)
    {
        builder.Pipeline.AddStep(PipelineStepNames.Finisher.ToStepName(), async context =>
        {
        }, dependsOn: PipelineStepNames.Finisher.GetDependencies());

        return builder
            .AddDriverStep(runStep1Param, runStep2Param)
            .AddCreateStep1()
            .AddCreateStep2();
    }
}

