using AppHost;
using AppHost.PipelineSteps;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

#pragma warning disable ASPIREPIPELINES001

// Add parameters that will be used by the pipeline


// Add a custom resource for the pipeline runner
var pipelineRunnerResource = new CustomResource("pipeline-runner");
var pipelineRunner = builder.AddResource(pipelineRunnerResource)
    .WithCommand(
        "run-finisher",
        "Run Pipeline",
        async context => 
        {
            var notifier = context.ServiceProvider.GetRequiredService<ResourceNotificationService>();
            
            // Update state to Running
            await notifier.PublishUpdateAsync(pipelineRunnerResource, state => state with 
            { 
                State = new("Running", KnownResourceStateStyles.Info)
            });
            
            // Trigger the pipeline execution
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "aspire",
                Arguments = "do finisher",
                WorkingDirectory = Environment.CurrentDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            
            var process = System.Diagnostics.Process.Start(processStartInfo);
            if (process != null)
            {
                await process.WaitForExitAsync(context.CancellationToken);
                
                // Update state to Finished when done
                await notifier.PublishUpdateAsync(pipelineRunnerResource, state => state with 
                { 
                    State = new("Finished", KnownResourceStateStyles.Success),
                    ExitCode = process.ExitCode
                });
                
                return process.ExitCode == 0 
                    ? CommandResults.Success() 
                    : CommandResults.Failure($"Pipeline failed with exit code {process.ExitCode}");
            }
            
            return CommandResults.Failure("Failed to start pipeline process");
        },
        new CommandOptions
        {
            UpdateState = context => ResourceCommandState.Enabled,
            IconName = "Play",
            IsHighlighted = true
        })
    .WithInitialState(new CustomResourceSnapshot
    {
        ResourceType = "Pipeline Runner",
        State = new("Ready", KnownResourceStateStyles.Success),
        Properties = []
    });

// Make parameters appear as children by adding them as environment variables to the pipeline runner
var runStep1Param = builder.AddParameter("runStep1", secret: false).WithParentRelationship(pipelineRunner.Resource);
var runStep2Param = builder.AddParameter("runStep2", secret: false).WithParentRelationship(pipelineRunner.Resource);

// Pass the parameters to the pipeline orchestrator
builder.AddPipelineSteps(runStep1Param, runStep2Param);

#pragma warning restore ASPIREPIPELINES001

builder.Build().Run();

// Custom resource class for the pipeline runner
public class CustomResource(string name) : Resource(name), IResourceWithEnvironment
{
    public IEnumerable<EnvironmentCallbackAnnotation> EnvironmentVariableCallbacks => 
        Annotations.OfType<EnvironmentCallbackAnnotation>();
}