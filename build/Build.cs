using System.Threading.Tasks;
using System;
using System.IO;
using Amg.Build;
using Amg.Build.Builtin;
using System.Diagnostics;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Linq;

public partial class BuildTargets
{
    static int Main(string[] args) => Runner.Run<BuildTargets>(args);
    
    private static readonly Serilog.ILogger Logger = Serilog.Log.Logger.ForContext(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    string productName => name;
    string year => DateTime.UtcNow.ToString("yyyy");
    string copyright => $"Copyright (c) {company} {year}";
    string configuration => "Debug";

    string Root { get; set; } = ".".Absolute();
    string OutDir => Root.Combine("out", configuration);
    string PackagesDir => OutDir.Combine("packages");
    string SrcDir => Root;
    string CommonAssemblyInfoFile => OutDir.Combine("CommonAssemblyInfo.cs");
    string VersionPropsFile => OutDir.Combine("Version.props");

    string SlnFile => SrcDir.Combine($"{name}.sln");
    string LibDir => SrcDir.Combine(name);

    [Once]
	protected virtual Amg.Build.Builtin.Dotnet Dotnet => new Amg.Build.Builtin.Dotnet();
    
	[Once]
	protected virtual Amg.Build.Builtin.Git Git => new Amg.Build.Builtin.Git();

    [Once]
    protected virtual ITool Msbuild => new Tool(@"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe")
        .WithArguments("-verbosity:minimal");

    [Once]
    [Description("Nuget restore")]
    public virtual async Task Restore()
    {
        await Nuget.Run("restore");
    }

    [Once]
    protected virtual async Task GenerateCode()
    {
        await WriteAssemblyInformationFile();
        await WriteVersionPropsFile();
    }

    [Once] [Description("Build")]
    public virtual async Task Build()
    {
        await Task.WhenAll(Restore(), GenerateCode());
        await Msbuild.Run(SlnFile);
    }
	
	[Once] 
    protected virtual async Task<string> WriteAssemblyInformationFile()
    {
        var v = await Git.GetVersion();
        return await CommonAssemblyInfoFile.WriteAllTextIfChangedAsync(
$@"// Generated. Changes will be lost.
[assembly: System.Reflection.AssemblyCopyright({copyright.Quote()})]
[assembly: System.Reflection.AssemblyCompany({company.Quote()})]
[assembly: System.Reflection.AssemblyProduct({productName.Quote()})]
[assembly: System.Reflection.AssemblyVersion({v.AssemblySemVer.Quote()})]
[assembly: System.Reflection.AssemblyFileVersion({v.AssemblySemFileVer.Quote()})]
[assembly: System.Reflection.AssemblyInformationalVersion({v.InformationalVersion.Quote()})]
");
    }

    [Once] 
	protected virtual async Task<string> WriteVersionPropsFile()
    {
        var v = await Git.GetVersion();
        return await VersionPropsFile.WriteAllTextIfChangedAsync(
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
    <PropertyGroup>
        <VersionPrefix>{v.MajorMinorPatch}</VersionPrefix>
        <VersionSuffix>{v.NuGetPreReleaseTagV2}</VersionSuffix>
    </PropertyGroup>
</Project>

");
    }

    static async Task Start(string command)
    {
        var cmd = new Tool("cmd.exe").WithArguments("/c", "start");
        await cmd.Run(" ", command);
    }

    [Once]
    protected virtual Task<ITool> NunitConsole() => NugetTool("nunit.consolerunner", "nunit3-console.exe");

    [Once][Description("run unit tests")]
    public virtual async Task Test()
    {
        await Task.WhenAll(NunitConsole(), Build());
        var tests = OutDir.Glob().Include("**/bin/*.Test.dll");
        Console.WriteLine(tests.Join());
        await (await NunitConsole())
            .WithArguments(
                "--output", OutDir.Combine("test-result.xml"),
                "--noresult"
            )
            .Run(tests.EnumerateFiles().ToArray());
    }

    ITool Nuget => new Tool("Nuget.exe");

    [Once]
    protected virtual async Task<ITool> NugetTool(string name, string exe)
    {
        var r = await Nuget.Run("install", name, "-ForceEnglishOutput");

        var m = Regex.Match(r.Output, @"Installing package '([^']+)' to '([^']+)'.");
        var packageId = m.Groups[1].Value;
        var dir = m.Groups[2].Value;
        string version = null;

        m = Regex.Match(r.Output, @"Successfully installed '([^ ]+) ([^ ]+)' to");
        if (m.Success)
        {
            version = m.Groups[2].Value;
        }
        else
        {
            m = Regex.Match(r.Output, $@"Package ""{packageId.ToLower()}\.([^""]+)"" is already installed.", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                version = m.Groups[1].Value;
            }
        }

        var installDir = dir.Combine(new[] { packageId, version }.Join("."));

        return new Tool(installDir.Combine("tools", exe));
    }

    [Once][Description("run")]
    public virtual async Task Run()
    {
        await new Tool("pskill").DoNotCheckExitCode().Run("hagen");
        var hagen = (OutDir.Combine(name, "bin", $"{name}.exe"));
        Logger.Information(hagen);
        if (hagen.IsOutOfDate(SrcDir.Glob("**")
            .Include("hagen/**")
            .Exclude("out")
            .EnumerateFiles()))
        {
            await Build();
        }
        Start(hagen);
    }

    [Once][Default]
	public virtual async Task Default()
	{
		await Test();
	}

    [Once][Description("Open in Visual Studio")]
    public virtual async Task OpenInVisualStudio()
    {
        await Task.WhenAll(Restore(), GenerateCode());
        Start(SlnFile);
    }
}
