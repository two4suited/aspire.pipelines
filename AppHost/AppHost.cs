using AppHost;
using AppHost.PipelineSteps;

var builder = DistributedApplication.CreateBuilder(args);

#pragma warning disable ASPIREPIPELINES001

builder.AddPipelineSteps();

#pragma warning restore ASPIREPIPELINES001

builder.Build().Run();