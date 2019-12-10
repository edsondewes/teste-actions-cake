#addin nuget:?package=Cake.Docker&version=0.10.1
#addin nuget:?package=Cake.FileHelpers&version=3.2.1
#addin nuget:?package=Cake.Git&version=0.21.0
#addin nuget:?package=Cake.OctoDeploy&version=3.2.0
#addin nuget:?package=Cake.Yarn&version=0.4.6

var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");

// Github
var buildTag = GitLogTip(".").Sha.Substring(0, 10);

Task("default")
    .IsDependentOn("docker-push")
    .Does(() =>
    {
        Information("Publicação realizada com sucesso");
    });

Task("client-build-standalone")
    .Does(() => {
         Yarn
            .FromPath("./web")
            .Install()
            .RunScript("build:standalone");
    });

Task("dotnet-publish")
    .Does(() =>
    {
        var publishSettings = new DotNetCorePublishSettings { NoRestore = true, Configuration = configuration };
        var restoreSettings = new DotNetCoreRestoreSettings { LockedMode = true };
        var solutions = new[] 
        {
            "./projeto1/Projeto1.sln",
            "./projeto2/Projeto2.sln"
        };

        foreach(var path in solutions)
        {
            DotNetCoreRestore(path, restoreSettings);
            DotNetCorePublish(path, publishSettings);
        }
    });

Task("docker-build")
    .IsDependentOn("client-build-standalone")
    .IsDependentOn("dotnet-publish")
    .Does(() =>
    {
        FileWriteText(".env", $"GIT_ID={buildTag}");
        DockerComposeBuild(new DockerComposeBuildSettings
        {
            Files = new[] { "./docker-compose.yml" }
        });
    });

Task("docker-push")
    .IsDependentOn("docker-build")
    .Does(() =>
    {
        DockerPush($"edsondewes/teste-actions:projeto1-api-{buildTag}");
        DockerPush($"edsondewes/teste-actions:projeto1-console-{buildTag}");
        DockerPush($"edsondewes/teste-actions:projeto2-api-{buildTag}");
        DockerPush($"edsondewes/teste-actions:web-{buildTag}");
    });

RunTarget(target);
