#pragma warning disable ASPIREPIPELINES001
#pragma warning disable ASPIREINTERACTION001
using Aspire.Hosting;
using Aspire.Hosting.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppHost.PipelineSteps;

public static class DriverStep
{
    public static IDistributedApplicationBuilder AddDriverStep(this IDistributedApplicationBuilder builder)
    {
        builder.Pipeline.AddStep(PipelineStepNames.Driver.ToStepName(), async context =>
        {
            var interactionService = context.Services.GetRequiredService<IInteractionService>();

            // Ask if user wants to run step 1
            var step1Result = await interactionService.PromptInputAsync(
                title: "Step 1 Configuration",
                message: "Do you want to run Step 1?",
                new InteractionInput
                {
                    Name = "runStep1",
                    Label = "Run Step 1",
                    InputType = InputType.Choice,
                    Required = true,
                    Options =
                    [
                        new KeyValuePair<string, string>("yes", "Yes"),
                        new KeyValuePair<string, string>("no", "No")
                    ]
                },
                cancellationToken: context.CancellationToken);

            var runStep1 = step1Result.Data?.Value?.ToLower() is "yes" or "y";

            // Ask if user wants to run step 2
            var step2Result = await interactionService.PromptInputAsync(
                title: "Step 2 Configuration",
                message: "Do you want to run Step 2?",
                new InteractionInput
                {
                    Name = "runStep2",
                    Label = "Run Step 2",
                    InputType = InputType.Choice,
                    Required = true,
                    Options =
                    [
                        new KeyValuePair<string, string>("yes", "Yes"),
                        new KeyValuePair<string, string>("no", "No")
                    ]
                },
                cancellationToken: context.CancellationToken);

            var runStep2 = step2Result.Data?.Value?.ToLower() is "yes" or "y";
            var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(nameof(DriverStep));
            
            // Display all resources in the builder
          

            // Store decisions in environment variables for the steps to check
            Environment.SetEnvironmentVariable("ASPIRE_RUN_STEP1", runStep1.ToString(), EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ASPIRE_RUN_STEP2", runStep2.ToString(), EnvironmentVariableTarget.Process);

            // Use logger for output
          
            logger.LogInformation("Driver complete - Step 1: {Step1Status}, Step 2: {Step2Status}",
                runStep1 ? "enabled": "disabled",
                runStep2 ? "enabled": "disabled");
        },requiredBy:PipelineStepNames.Finisher.ToStepName());
        
        return builder;
    }
}


