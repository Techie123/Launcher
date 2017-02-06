// Arguments

var _target = Argument("target", "Default");

// PREPARATION

var _buildOutput = "Build";
var _installerOutput = _buildOutput + "/Installers";

// TASKS

Task("BuildWindows")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    NuGetRestore(
        "Source/Launcher.Windows/Launcher.Windows.csproj",
        new NuGetRestoreSettings
        {
            PackagesDirectory = "Source/packages"
        }
    );

    MSBuild(
        "Source/Launcher.Windows/Launcher.Windows.csproj",
        configurator => configurator
            .SetConfiguration("Release")
            .SetVerbosity(Verbosity.Minimal)
    );
});

Task("BuildLinux")
    .WithCriteria(() => IsRunningOnUnix())
    .Does(() =>
{
    NuGetRestore(
        "Source/Launcher.Linux/Launcher.Linux.csproj",
        new NuGetRestoreSettings
        {
            PackagesDirectory = "Source/packages"
        }
    );

    XBuild(
        "Source/Launcher.Linux/Launcher.Linux.csproj",
        configurator => configurator
            .SetConfiguration("Release")
            .SetVerbosity(Verbosity.Minimal)
    );
});

Task("PackageWindows")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    var installerdir = _installerOutput + "/Windows";
    CreateDirectory(installerdir);
    CleanDirectory(installerdir);

    // Create bundle of binaries
    Zip(_buildOutput + "/Windows/Release/Launcher", installerdir + "/Launcher.zip");

    // Create installer
    var datadir = installerdir + "/HearthstoneMod";
    CreateDirectory(datadir);

    CopyDirectory(_buildOutput + "/Windows/Release/Launcher", datadir + "/Launcher");
    CopyFileToDirectory("Installers/Windows/HearthstoneMod.nsi", installerdir);

    MakeNSIS(installerdir + "/HearthstoneMod.nsi");
    DeleteFile(installerdir + "/HearthstoneMod.nsi");
    DeleteDirectory(datadir, true);
});

Task("PackageLinux")
    .IsDependentOn("BuildLinux")
    .WithCriteria(() => IsRunningOnUnix())
    .Does(() =>
{
    var installerdir = _installerOutput + "/Linux";
    CreateDirectory(installerdir);
    CleanDirectory(installerdir);

    // Create bundle of binaries
    Zip(_buildOutput + "/Linux/Release/Launcher", installerdir + "/Launcher.zip");

    // Create .run installer
    var rundir = installerdir + "/run";
    CreateDirectory(rundir);

    CopyDirectory(_buildOutput + "/Linux/Release/Launcher", rundir + "/HearthstoneMod/Launcher");
    CopyFileToDirectory("Installers/Linux/hearthstone-mod", rundir);
    CopyFileToDirectory("Installers/Linux/RUN/hearthstone-mod-uninstall", rundir);
    CopyFileToDirectory("Installers/Linux/hearthstone-mod.desktop", rundir);
    CopyFileToDirectory("Installers/Linux/RUN/postinstall.sh", rundir);

    StartProcess("ThirdParty/makeself/makeself.sh", "--keep-umask " + rundir + " " + installerdir + "/hearthstone-mod.run 'Hearthstone Mod Installer' ./postinstall.sh");
    DeleteDirectory(rundir, true);

    // Create .deb installer
    var debdir = installerdir + "/deb";
    CreateDirectory(debdir);

    CopyDirectory(_buildOutput + "/Linux/Release/Launcher", debdir + "/opt/HearthstoneMod/Launcher");
    CopyDirectory("Installers/Linux/DEBIAN", debdir + "/DEBIAN");
    CreateDirectory(debdir + "/usr/bin/");
    CopyFileToDirectory("Installers/Linux/hearthstone-mod", debdir + "/usr/bin/");
    CreateDirectory(debdir + "/usr/share/applications/");
    CopyFileToDirectory("Installers/Linux/hearthstone-mod.desktop", debdir + "/usr/share/applications/");

    StartProcess("dpkg", "--build " + debdir + " " + installerdir + "/hearthstone-mod.deb");
    DeleteDirectory(debdir, true);
});

// TASK TARGETS

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Package");

Task("Build")
    .IsDependentOn("BuildWindows")
    .IsDependentOn("BuildLinux");

Task("Package")
    .IsDependentOn("Build")
    .IsDependentOn("PackageWindows")
    .IsDependentOn("PackageLinux");

// EXECUTION

RunTarget(_target);
