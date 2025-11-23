#pragma warning disable ASPIREPIPELINES001
#pragma warning disable ASPIREINTERACTION001
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppHost.PipelineSteps;

public static class DriverStep
{
    public static IDistributedApplicationBuilder AddDriverStep(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ParameterResource> runStep1Param,
        IResourceBuilder<ParameterResource> runStep2Param)
    {
        builder.Pipeline.AddStep(PipelineStepNames.Driver.ToStepName(), async context =>
        {
            var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(nameof(DriverStep));
            
            // Get parameter values asynchronously
            var runStep1Value = await runStep1Param.Resource.GetValueAsync(context.CancellationToken);
            var runStep2Value = await runStep2Param.Resource.GetValueAsync(context.CancellationToken);
            
            var runStep1 = runStep1Value?.ToLower() is "true" or "yes" or "y";
            var runStep2 = runStep2Value?.ToLower() is "true" or "yes" or "y";
            
            // Store decisions in environment variables for the steps to check
            Environment.SetEnvironmentVariable("ASPIRE_RUN_STEP1", runStep1.ToString(), EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPIRE_RUN_STEP2", runStep2.ToString(), EnvironmentVariableTarget.Process);

            logger.LogInformation("Driver complete - Step 1: {Step1Status}, Step 2: {Step2Status}",
                runStep1 ? "enabled": "disabled",
                runStep2 ? "enabled": "disabled");
        });
        
        return builder;
    }
}


