using AppHost;


var builder = DistributedApplication.CreateBuilder(args)
    .WithSetup();

var setup = builder.AddProjectSetup("project1")
    .WithCreateFile("step1")
    .WithCreateFile("step2", DependsOn: ["step1"])
    .WithServiceDefaults("service-defaults", DependsOn: ["step2"])
    .WithAPI("api", DependsOn: ["service-defaults"], References: ["servicedefaults"]);


builder.Build().Run();

// Custom resource class for the pipeline runner
public class CustomResource(string name) : Resource(name), IResourceWithEnvironment
{
    public IEnumerable<EnvironmentCallbackAnnotation> EnvironmentVariableCallbacks => 
        Annotations.OfType<EnvironmentCallbackAnnotation>();
}