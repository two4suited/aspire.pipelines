using AppHost;
using AppHost.PipelineSteps;

var builder = DistributedApplication.CreateBuilder(args);

#pragma warning disable ASPIREPIPELINES001

builder
    .AddCreateServiceDefaultsStep()
    .AddCreateApiStep()
    .AddOutputPipelineStep();

#pragma warning restore ASPIREPIPELINES001

builder.Build().Run();