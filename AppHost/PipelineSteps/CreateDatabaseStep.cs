#pragma warning disable ASPIREPIPELINES001
using Aspire.Hosting;
using Aspire.Hosting.Pipelines;

namespace AppHost.PipelineSteps;

public static class CreateDatabaseStep
{
    public static IDistributedApplicationBuilder AddCreateDatabaseStep(this IDistributedApplicationBuilder builder)
    {
        builder.Pipeline.AddStep("create-database", async context =>
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".."));
            
            // Create extension method file for adding the database
            var templatePath = Path.Combine(AppContext.BaseDirectory, "PipelineSteps", "Templates", "DatabaseExtensions.template");
            var extensionMethodContent = await File.ReadAllTextAsync(templatePath);
            
            var extensionMethodPath = Path.Combine(repoRoot, "AppHost", "Extensions", "DatabaseExtensions.cs");
            Directory.CreateDirectory(Path.GetDirectoryName(extensionMethodPath)!);
            await File.WriteAllTextAsync(extensionMethodPath, extensionMethodContent);
            
            // Add the AddDatabase() call to AppHost.cs before var api = builder.AddApi()
            var appHostPath = Path.Combine(repoRoot, "AppHost", "AppHost.cs");
            var appHostContent = await File.ReadAllTextAsync(appHostPath);
            
            // Check if the AddDatabase() call already exists
            if (!appHostContent.Contains("builder.AddDatabase()"))
            {
                // Insert before var api = builder.AddApi()
                var insertMarker = "var api = builder.AddApi();";
                if (appHostContent.Contains(insertMarker))
                {
                    appHostContent = appHostContent.Replace(
                        insertMarker,
                        "var db = builder.AddDatabase();\nvar api = builder.AddApi(db);"
                    );
                    await File.WriteAllTextAsync(appHostPath, appHostContent);
                }
            }
            
            // Update the Api project's extension method to add database reference
            var apiExtensionPath = Path.Combine(repoRoot, "AppHost", "Extensions", "ApiExtensions.cs");
            if (File.Exists(apiExtensionPath))
            {
                var apiExtensionContent = await File.ReadAllTextAsync(apiExtensionPath);
                
                // Check if WithReference already exists
                if (!apiExtensionContent.Contains("WithReference"))
                {
                    // Add a parameter for the database
                    apiExtensionContent = apiExtensionContent.Replace(
                        "public static IResourceBuilder<ProjectResource> AddApi(this IDistributedApplicationBuilder builder)",
                        "public static IResourceBuilder<ProjectResource> AddApi(this IDistributedApplicationBuilder builder, IResourceBuilder<PostgresServerResource>? database = null)"
                    );
                    
                    // Update the return statement to conditionally include database reference
                    apiExtensionContent = apiExtensionContent.Replace(
                        "return builder.AddProject(\"api\", \"../Api/Api.csproj\");",
                        @"var api = builder.AddProject(""api"", ""../Api/Api.csproj"");
        
        if (database != null)
        {
            api = api.WithReference(database);
        }
        
        return api;"
                    );
                    await File.WriteAllTextAsync(apiExtensionPath, apiExtensionContent);
                }
            }
        }, requiredBy: "output-pipeline");
        
        return builder;
    }
}

