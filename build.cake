using System;
using System.IO;

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Package")
    .Does(() => { });

Task("Build")
    .IsDependentOn("BuildWindows")
    .IsDependentOn("BuildLinux")
    .Does(() => { });

Task("BuildWindows").Does(() =>
{
    if (!IsRunningOnWindows())
        return;
    
    MSBuild(
        "Source/Launcher.Windows/Launcher.Windows.csproj",
        configurator => configurator
            .SetConfiguration("Release")
            .SetVerbosity(Verbosity.Minimal)
    );
});

Task("BuildLinux").Does(() =>
{
    if (!IsRunningOnUnix())
        return;

    XBuild(
        "Source/Launcher.Linux/Launcher.Linux.csproj",
        configurator => configurator
            .SetConfiguration("Release")
            .SetVerbosity(Verbosity.Minimal)
    );
});

Task("Package")
    .IsDependentOn("Build")
    .Does(() => { });

RunTarget("Default");
