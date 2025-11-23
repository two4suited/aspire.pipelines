#pragma warning disable ASPIREPIPELINES001
#pragma warning disable ASPIREINTERACTION001
using Aspire.Hosting;
using Aspire.Hosting.Pipelines;
using Microsoft.Extensions.DependencyInjection;

namespace AppHost.PipelineSteps;

public static class CreateApiStep
{
    public static IDistributedApplicationBuilder AddCreateApiStep(this IDistributedApplicationBuilder builder)
    {
        builder.Pipeline.AddStep("create-api", async context =>
        {
            // Ask if user wants to add a database using IInteractionService
            var interactionService = context.Services.GetRequiredService<IInteractionService>();
            var dbResult = await interactionService.PromptInputAsync(
                title: "Database Configuration",
                message: "Do you want to add a PostgreSQL database for the API? (yes/no)",
                new InteractionInput
                {
                    Name = "addDatabase",
                    Label = "Add Database",
                    InputType = InputType.Text,
                    Required = true,
                    Placeholder = "yes"
                },
                cancellationToken: context.CancellationToken);
            
            var addDatabase = dbResult.Data?.Value?.ToLower() is "yes" or "y";
            
            var repoRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".."));
            var apiProjectName = "Api";
            var apiProjectPath = Path.Combine(repoRoot, apiProjectName);
            
            // Create the webapi project
            var createProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"new webapi -n {apiProjectName} -o {apiProjectPath}",
                WorkingDirectory = repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            
            if (createProcess != null)
            {
                await createProcess.WaitForExitAsync();
            }
            
            // Add the project to the solution
            var slnPath = Path.Combine(repoRoot, "aspire.pipelines.sln");
            var addProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"sln {slnPath} add {Path.Combine(apiProjectPath, $"{apiProjectName}.csproj")}",
                WorkingDirectory = repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            
            if (addProcess != null)
            {
                await addProcess.WaitForExitAsync();
            }
            
            // Add ServiceDefaults as a reference to the Api project
            var serviceDefaultsProjectPath = Path.Combine(repoRoot, "ServiceDefaults", "ServiceDefaults.csproj");
            var addRefProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"add {Path.Combine(apiProjectPath, $"{apiProjectName}.csproj")} reference {serviceDefaultsProjectPath}",
                WorkingDirectory = repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            
            if (addRefProcess != null)
            {
                await addRefProcess.WaitForExitAsync();
            }
            
            // Create extension method file for adding the Api project
            var templatePath = Path.Combine(AppContext.BaseDirectory, "PipelineSteps", "Templates", "ProjectExtensions.template");
            var extensionMethodContent = await File.ReadAllTextAsync(templatePath);
            
            // Replace placeholders
            extensionMethodContent = extensionMethodContent
                .Replace("{{ProjectName}}", apiProjectName)
                .Replace("{{projectName}}", apiProjectName.ToLower());
            
            var extensionMethodPath = Path.Combine(repoRoot, "AppHost", "Extensions", $"{apiProjectName}Extensions.cs");
            Directory.CreateDirectory(Path.GetDirectoryName(extensionMethodPath)!);
            await File.WriteAllTextAsync(extensionMethodPath, extensionMethodContent);
            
            // Add builder.AddServiceDefaults() to Api/Program.cs before var app = builder.Build()
            var apiProgramPath = Path.Combine(apiProjectPath, "Program.cs");
            var apiProgramContent = await File.ReadAllTextAsync(apiProgramPath);
            
            // Check if AddServiceDefaults() call already exists
            if (!apiProgramContent.Contains("builder.AddServiceDefaults()"))
            {
                // Insert before var app = builder.Build()
                var buildMarker = "var app = builder.Build();";
                if (apiProgramContent.Contains(buildMarker))
                {
                    apiProgramContent = apiProgramContent.Replace(
                        buildMarker,
                        $"builder.AddServiceDefaults();\n\n{buildMarker}"
                    );
                    await File.WriteAllTextAsync(apiProgramPath, apiProgramContent);
                }
            }
            
            // Add the AddApi() call to AppHost.cs before builder.Build().Run()
            var appHostPath = Path.Combine(repoRoot, "AppHost", "AppHost.cs");
            var appHostContent = await File.ReadAllTextAsync(appHostPath);
            
            // Check if the AddApi() call already exists
            if (!appHostContent.Contains("builder.AddApi()"))
            {
                // Add using AppHost; if not already present
                if (!appHostContent.Contains("using AppHost;"))
                {
                    var firstUsingIndex = appHostContent.IndexOf("using ");
                    if (firstUsingIndex >= 0)
                    {
                        appHostContent = appHostContent.Insert(firstUsingIndex, "using AppHost;\n");
                    }
                }
                
                // Insert before builder.Build().Run()
                var insertMarker = "builder.Build().Run();";
                if (appHostContent.Contains(insertMarker))
                {
                    appHostContent = appHostContent.Replace(
                        insertMarker,
                        $"var api = builder.AddApi();\n\n{insertMarker}"
                    );
                    await File.WriteAllTextAsync(appHostPath, appHostContent);
                }
            }
            
            // Conditionally create database if user requested it
            if (addDatabase)
            {
                Console.WriteLine("Creating database resources...");
                
                // Create extension method file for adding the database
                var dbTemplatePath = Path.Combine(AppContext.BaseDirectory, "PipelineSteps", "Templates", "DatabaseExtensions.template");
                var dbExtensionMethodContent = await File.ReadAllTextAsync(dbTemplatePath);
                
                var dbExtensionMethodPath = Path.Combine(repoRoot, "AppHost", "Extensions", "DatabaseExtensions.cs");
                Directory.CreateDirectory(Path.GetDirectoryName(dbExtensionMethodPath)!);
                await File.WriteAllTextAsync(dbExtensionMethodPath, dbExtensionMethodContent);
                
                // Add the AddDatabase() call to AppHost.cs before var api = builder.AddApi()
                appHostContent = await File.ReadAllTextAsync(appHostPath);
                
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
                var apiExtensionPath = Path.Combine(repoRoot, "AppHost", "Extensions", $"{apiProjectName}Extensions.cs");
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
                
                Console.WriteLine("Database resources created successfully.");
            }
        }, requiredBy: "output-pipeline");
        
        return builder;
    }
}

