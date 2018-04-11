#load "solution.cake"
#addin nuget:?package=Cake.Git
#addin nuget:?package=Nuget.Core
#addin nuget:?package=DotNetZip
#addin nuget:https://www.aspenlaub.net/nuget/?package=Aspenlaub.Net.GitHub.CSharp.Shatilaya

using Folder = Aspenlaub.Net.GitHub.CSharp.Pegh.Entities.Folder;
using FolderUpdater = Aspenlaub.Net.GitHub.CSharp.Pegh.Components.FolderUpdater;
using FolderUpdateMethod = Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces.FolderUpdateMethod;
using ErrorsAndInfos = Aspenlaub.Net.GitHub.CSharp.Pegh.Entities.ErrorsAndInfos;
using FolderExtensions = Aspenlaub.Net.GitHub.CSharp.Pegh.FolderExtensions;
using Regex = System.Text.RegularExpressions.Regex;
using ComponentProvider = Aspenlaub.Net.GitHub.CSharp.Shatilaya.ComponentProvider;

masterDebugBinFolder = MakeAbsolute(Directory(masterDebugBinFolder)).FullPath;
masterReleaseBinFolder = MakeAbsolute(Directory(masterReleaseBinFolder)).FullPath;

var target = Argument("target", "Default");
var toolsVersion = 14;

var artifactsFolder = MakeAbsolute(Directory("./artifacts")).FullPath;
var debugArtifactsFolder = MakeAbsolute(Directory("./artifacts/Debug")).FullPath;
var releaseArtifactsFolder = MakeAbsolute(Directory("./artifacts/Release")).FullPath;
var objFolder = MakeAbsolute(Directory("./temp/obj")).FullPath;
var testResultsFolder = MakeAbsolute(Directory("./TestResults")).FullPath;
var tempFolder = MakeAbsolute(Directory("./temp")).FullPath;
var repositoryFolder = MakeAbsolute(DirectoryPath.FromString(".")).FullPath;

var buildCakeFileName = MakeAbsolute(Directory(".")).FullPath + "/build.cake";
var tempCakeBuildFileName = tempFolder + "/build.cake.new";

var solutionId = solution.Substring(solution.LastIndexOf('/') + 1).Replace(".sln", "");
var currentGitBranch = GitBranchCurrent(DirectoryPath.FromString("."));
var latestBuildCakeUrl = "https://raw.githubusercontent.com/aspenlaub/Shatilaya/master/build.cake?g=" + System.Guid.NewGuid();
var componentProvider = new ComponentProvider();

Setup(ctx => { 
  Information("Solution is: " + solution);
  Information("Solution ID is: " + solutionId);
  Information("Target is: " + target);
  Information("Artifacts folder is: " + artifactsFolder);
  Information("Current GIT branch is: " + currentGitBranch.FriendlyName);
  Information("Build cake is: " + buildCakeFileName);
  Information("Latest build cake URL is: " + latestBuildCakeUrl);
});

Task("UpdateBuildCake")
  .Description("Update build.cake")
  .Does(() => {
    var oldContents = System.IO.File.ReadAllText(buildCakeFileName);
    if (!System.IO.Directory.Exists(tempFolder)) {
      System.IO.Directory.CreateDirectory(tempFolder);
    }
    if (System.IO.File.Exists(tempCakeBuildFileName)) {
      System.IO.File.Delete(tempCakeBuildFileName);
    }
    using (var webClient = new System.Net.WebClient()) {
      webClient.DownloadFile(latestBuildCakeUrl, tempCakeBuildFileName);
    }
    if (Regex.Replace(oldContents, @"\s", "") != Regex.Replace(System.IO.File.ReadAllText(tempCakeBuildFileName), @"\s", "")) {
      System.IO.File.Delete(buildCakeFileName);
      System.IO.File.Move(tempCakeBuildFileName, buildCakeFileName); 
      throw new Exception("Your build.cake file has been updated. Please check it in and then retry running it.");
    } else {
      System.IO.File.Delete(tempCakeBuildFileName);
	}
  });

Task("Clean")
  .Description("Clean up artifacts and intermediate output folder")
  .Does(() => {
    CleanDirectory(artifactsFolder); 
    CleanDirectory(objFolder); 
  });

Task("Restore")
  .Description("Restore nuget packages")
  .Does(() => {
    var configFile = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\NuGet\nuget.config";   
    if (!System.IO.File.Exists(configFile)) {
       throw new Exception(string.Format("Nuget configuration file \"{0}\" not found", configFile));
    }
    NuGetRestore(solution, new NuGetRestoreSettings { ConfigFile = configFile });
  });

Task("UpdateNuspec")
  .Description("Update nuspec if necessary")
  .Does(() => {
    var solutionFileFullName = solution.Replace('/', '\\');
    var nuSpecFile = solutionFileFullName.Replace(".sln", ".nuspec");
	var nuSpecErrorsAndInfos = new ErrorsAndInfos();
    var headTipIdSha = componentProvider.GitUtilities.HeadTipIdSha(new Folder(repositoryFolder));
	componentProvider.NuSpecCreator.CreateNuSpecFileIfRequiredOrPresent(true, solutionFileFullName, new List<string> { headTipIdSha }, nuSpecErrorsAndInfos);
    if (nuSpecErrorsAndInfos.Errors.Any()) {
	  throw new Exception(string.Join("\r\n", nuSpecErrorsAndInfos.Errors));
	}
  });

Task("VerifyThatThereAreNoUncommittedChanges")
  .Description("Verify that there are no uncommitted changes")
  .Does(() => {
    var uncommittedErrorsAndInfos = new ErrorsAndInfos();
    componentProvider.GitUtilities.VerifyThatThereAreNoUncommittedChanges(new Folder(repositoryFolder), uncommittedErrorsAndInfos);
    if (uncommittedErrorsAndInfos.Errors.Any()) {
	  throw new Exception(string.Join("\r\n", uncommittedErrorsAndInfos.Errors));
	}
  });

Task("DebugBuild")
  .Description("Build solution in Debug and clean up intermediate output folder")
  .Does(() => {
    MSBuild(solution, settings 
      => settings
        .SetConfiguration("Debug")
        .SetVerbosity(Verbosity.Minimal)
		.UseToolVersion(MSBuildToolVersion.NET46)
        .WithProperty("Platform", "Any CPU")
        .WithProperty("OutDir", debugArtifactsFolder)
    );
    CleanDirectory(objFolder); 
  });

Task("RunTestsOnDebugArtifacts")
  .Description("Run unit tests on Debug artifacts")
  .Does(() => {
    var msTestExe = componentProvider.ExecutableFinder.FindMsTestExe(toolsVersion);
	if (msTestExe == "") {
      VSTest(debugArtifactsFolder + "/*.Test.dll", new VSTestSettings() { Logger = "trx", InIsolation = true });
	} else {
      MSTest(debugArtifactsFolder + "/*.Test.dll", new MSTestSettings() { NoIsolation = false });
	}
    CleanDirectory(testResultsFolder); 
    DeleteDirectory(testResultsFolder, new DeleteDirectorySettings { Recursive = false, Force = false });
  });

Task("CopyDebugArtifacts")
  .WithCriteria(() => currentGitBranch.FriendlyName == "master")
  .Description("Copy Debug artifacts to master Debug binaries folder")
  .Does(() => {
    var updater = new FolderUpdater();
	var updaterErrorsAndInfos = new ErrorsAndInfos();
    updater.UpdateFolder(new Folder(debugArtifactsFolder.Replace('/', '\\')), new Folder(masterDebugBinFolder.Replace('/', '\\')), 
      FolderUpdateMethod.Assemblies, updaterErrorsAndInfos);
    if (updaterErrorsAndInfos.Errors.Any()) {
	  throw new Exception(string.Join("\r\n", updaterErrorsAndInfos.Errors));
	}
  });

Task("ReleaseBuild")
  .Description("Build solution in Release and clean up intermediate output folder")
  .Does(() => {
    MSBuild(solution, settings 
      => settings
        .SetConfiguration("Release")
        .SetVerbosity(Verbosity.Minimal)
		.UseToolVersion(MSBuildToolVersion.NET46)
        .WithProperty("Platform", "Any CPU")
        .WithProperty("OutDir", releaseArtifactsFolder)
    );
    CleanDirectory(objFolder); 
  });

Task("RunTestsOnReleaseArtifacts")
  .Description("Run unit tests on Release artifacts")
  .Does(() => {
    var msTestExe = componentProvider.ExecutableFinder.FindMsTestExe(toolsVersion);
	if (msTestExe == "") {
      VSTest(releaseArtifactsFolder + "/*.Test.dll", new VSTestSettings() { Logger = "trx", InIsolation = true });
	} else {
      MSTest(releaseArtifactsFolder + "/*.Test.dll", new MSTestSettings() { NoIsolation = false });
	}
    CleanDirectory(testResultsFolder); 
    DeleteDirectory(testResultsFolder, new DeleteDirectorySettings { Recursive = false, Force = false });
  });

Task("CopyReleaseArtifacts")
  .WithCriteria(() => currentGitBranch.FriendlyName == "master")
  .Description("Copy Release artifacts to master Release binaries folder")
  .Does(() => {
    var updater = new FolderUpdater();
	var updaterErrorsAndInfos = new ErrorsAndInfos();
    updater.UpdateFolder(new Folder(releaseArtifactsFolder.Replace('/', '\\')), new Folder(masterReleaseBinFolder.Replace('/', '\\')), 
      FolderUpdateMethod.Assemblies, updaterErrorsAndInfos);
    if (updaterErrorsAndInfos.Errors.Any()) {
	  throw new Exception(string.Join("\r\n", updaterErrorsAndInfos.Errors));
	}
  });

Task("CreateNuGetPackage")
  .WithCriteria(() => currentGitBranch.FriendlyName == "master")
  .Description("Create nuget package in the master Release binaries folder")
  .Does(() => {
	var folder = new Folder(masterReleaseBinFolder);
	if (!FolderExtensions.LastWrittenFileFullName(folder).EndsWith("nupkg")) {
      var nuGetPackSettings = new NuGetPackSettings {
        BasePath = "./src/", 
        OutputDirectory = masterReleaseBinFolder, 
        IncludeReferencedProjects = true,
        Properties = new Dictionary<string, string> { { "Configuration", "Release" } }
      };

      NuGetPack("./src/" + solutionId + ".csproj", nuGetPackSettings);
	}
  });

Task("PushNuGetPackage")
  .WithCriteria(() => currentGitBranch.FriendlyName == "master")
  .Description("Push nuget package")
  .Does(() => {
	var nugetPackageToPushFinder = componentProvider.NugetPackageToPushFinder;
	string packageFileFullName, feedUrl, apiKey;
	var finderErrorsAndInfos = new ErrorsAndInfos();
	nugetPackageToPushFinder.FindPackageToPush(new Folder(masterReleaseBinFolder.Replace('/', '\\')), new Folder(repositoryFolder.Replace('/', '\\')), solution.Replace('/', '\\'), out packageFileFullName, out feedUrl, out apiKey, finderErrorsAndInfos);
    if (finderErrorsAndInfos.Errors.Any()) {
	  throw new Exception(string.Join("\r\n", finderErrorsAndInfos.Errors));
	}
    if (packageFileFullName != "" && feedUrl != "" && apiKey != "") {
      Information("Pushing " + packageFileFullName + " to " + feedUrl + "..");
      NuGetPush(packageFileFullName, new NuGetPushSettings { Source = feedUrl });
    }
  });

Task("CleanRestoreUpdateNuspec")
  .IsDependentOn("Clean").IsDependentOn("Restore").IsDependentOn("UpdateNuspec").Does(() => {
  });

Task("BuildAndTestDebugAndRelease")
  .IsDependentOn("DebugBuild").IsDependentOn("RunTestsOnDebugArtifacts").IsDependentOn("CopyDebugArtifacts")
  .IsDependentOn("ReleaseBuild").IsDependentOn("RunTestsOnReleaseArtifacts").IsDependentOn("CopyReleaseArtifacts").Does(() => {
  });

Task("IgnoreOutdatedBuildCakePendingChangesAndDoNotPush")
  .IsDependentOn("CleanRestoreUpdateNuspec").IsDependentOn("BuildAndTestDebugAndRelease").IsDependentOn("CreateNuGetPackage").Does(() => {
  });

Task("IgnoreOutdatedBuildCakePendingChanges")
  .IsDependentOn("IgnoreOutdatedBuildCakePendingChangesAndDoNotPush").IsDependentOn("PushNuGetPackage").Does(() => {
  });

Task("IgnoreOutdatedBuildCakeAndDoNotPush")
  .IsDependentOn("CleanRestoreUpdateNuspec").IsDependentOn("VerifyThatThereAreNoUncommittedChanges")
  .IsDependentOn("BuildAndTestDebugAndRelease").IsDependentOn("CreateNuGetPackage")
  .Does(() => {
  });

Task("Default")
  .IsDependentOn("UpdateBuildCake").IsDependentOn("CleanRestoreUpdateNuspec").IsDependentOn("VerifyThatThereAreNoUncommittedChanges")
  .IsDependentOn("BuildAndTestDebugAndRelease").IsDependentOn("CreateNuGetPackage").IsDependentOn("PushNuGetPackage").Does(() => {
  });

RunTarget(target);