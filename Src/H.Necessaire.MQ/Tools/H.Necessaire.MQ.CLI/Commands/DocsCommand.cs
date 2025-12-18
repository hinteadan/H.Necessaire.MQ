using H.Necessaire.CLI.Commands;
using H.Necessaire.MQ.Tools.CodeAnalysis;
using H.Necessaire.MQ.Tools.DataContracts;
using H.Necessaire.MQ.Tools.Reporting;
using H.Necessaire.Runtime.ExternalCommandRunner;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.CLI.Commands
{
    internal class DocsCommand : CommandBase
    {
        public override async Task<OperationResult> Run()
        {
            using (new CodeAnalysisProgressiveScope((s, e) => { Log($"{e.PercentValue.ToString("0.00")} % Running Code Analysis - {e.CurrentActionName}..."); return Task.CompletedTask; }))
            using (new ProgressiveScope("docs", (s, e) => { Log($"{e.PercentValue.ToString("0.00")} % Running Docs - {e.CurrentActionName}..."); return Task.CompletedTask; }))
            {
                return await RunSubCommand();
            }
        }

        class DebugSubComnnad : SubCommandBase
        {
            ProgressReporter progressReporter;
            ImACodeAnalyzer codeAnalyzer;
            ImAReportBuilder<ModuleInfo[]> mermaidModuleInfoReporter;
            ExternalCommandRunner externalCommandRunner;
            public override void ReferDependencies(ImADependencyProvider dependencyProvider)
            {
                base.ReferDependencies(dependencyProvider);
                progressReporter = ProgressReporter.Get("docs") ?? new ProgressReporter();
                codeAnalyzer = dependencyProvider.Get<ImACodeAnalyzer>();
                mermaidModuleInfoReporter = dependencyProvider.Build<ImAReportBuilder<ModuleInfo[]>>(WellKnown.Builders.MermaidMd, defaultTo: null, typeof(ImAReportBuilder<>).Assembly);
                externalCommandRunner = dependencyProvider.Get<ExternalCommandRunner>();
            }

            const string hNecessaireMqAssembliesPrefix = "H.Necessaire.MQ";
            const string srcFolderRelativePath = "/Src/H.Necessaire.MQ/";
            static readonly FileInfo meramidCLIPath = new FileInfo(Path.Combine("C:", "Users", Environment.UserName, "AppData", "Roaming", "npm", "mmdc.cmd"));
            public override async Task<OperationResult> Run(params Note[] args)
            {
                int steps = 1;
                progressReporter.SetSourceInterval(new NumberInterval(0, steps));

                await progressReporter.RaiseOnProgress("Loading H.Necessaire.MQ Assemblies", 0);

                //await LoadAllNecessaireMqAssembliesInAppDomain();
                progressReporter.SetSourceInterval(new NumberInterval(0, steps));

                //var hNecessaireMq = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "H.Necessaire.MQ");

                ModuleInfo[] modules = await codeAnalyzer.ProcessModuleInfoForProjects(x => x.StartsWith("H.Necessaire.MQ.Bus"));

                DataBin report = await mermaidModuleInfoReporter.BuildReport(new ReportInfo<ModuleInfo[]>
                {
                    Data = modules,
                    Name = "Test Report",
                });


                using (ImADataBinStream reportStream = await report.OpenDataBinStream())
                {
                    string mermaidMd = await reportStream.DataStream.ReadAsStringAsync();


                    FileInfo mermaidMdFileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), report.GenerateHumanFriendlyFileName()));

                    await progressReporter.RaiseOnProgress($"Generating {mermaidMdFileInfo.FullName}", 0);

                    await File.WriteAllTextAsync(mermaidMdFileInfo.FullName, mermaidMd);

                    await progressReporter.RaiseOnProgress($"DONE Generating {mermaidMdFileInfo.FullName}", 0);

                    var result = await externalCommandRunner.Run(meramidCLIPath.FullName, "-i", mermaidMdFileInfo.FullName, "-o", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", Path.ChangeExtension(mermaidMdFileInfo.Name, "svg")));

                    mermaidMdFileInfo.Delete();
                }

                return OperationResult.Win();
            }

            async Task LoadAllNecessaireMqAssembliesInAppDomain()
            {
                int steps = 3;
                progressReporter.SetSourceInterval(new NumberInterval(0, steps));

                await progressReporter.RaiseOnProgress("Finding solution folder...", 0);
                string codebaseFolderPath = GetCodebaseFolderPath();
                await progressReporter.RaiseOnProgress($"DONE Finding solution folder ({codebaseFolderPath})", 1);

                await progressReporter.RaiseOnProgress("Finding H.Necessaire.MQ DLLs...", 1);
                FileInfo[] dlls = new DirectoryInfo(codebaseFolderPath).GetFiles($"{hNecessaireMqAssembliesPrefix}*.dll", SearchOption.AllDirectories);
                await progressReporter.RaiseOnProgress($"DONE Finding H.Necessaire.MQ DLLs. Found {dlls.Length}", 2);

                await progressReporter.RaiseOnProgress("Loading DLLs into AppDomain...", 2);
                progressReporter.SetSourceInterval(new NumberInterval(0, dlls.Length));
                int index = -1;
                foreach (FileInfo dll in dlls)
                {
                    index++;
                    await progressReporter.RaiseOnProgress($"Loading DLL ({dll.Name}) into AppDomain...", index);
                    new Action(() => { Assembly.LoadFile(dll.FullName); }).TryOrFailWithGrace();
                    await progressReporter.RaiseOnProgress($"DONE Loading DLL ({dll.Name}) into AppDomain", index + 1);
                }
                progressReporter.SetSourceInterval(new NumberInterval(0, steps));
                await progressReporter.RaiseOnProgress("DONE Loading DLLs into AppDomain", 3);
            }

            private static string GetCodebaseFolderPath()
            {
                string codeBase = Assembly.GetExecutingAssembly()?.Location ?? string.Empty;
                UriBuilder uri = new UriBuilder(codeBase);
                string dllPath = Uri.UnescapeDataString(uri.Path);
                int srcFolderIndex = dllPath.ToLowerInvariant().IndexOf(srcFolderRelativePath, StringComparison.InvariantCultureIgnoreCase);
                if (srcFolderIndex < 0)
                    return string.Empty;
                string srcFolderPath = Path.GetDirectoryName(dllPath.Substring(0, srcFolderIndex + srcFolderRelativePath.Length)) ?? string.Empty;
                return srcFolderPath;
            }
        }
    }
}
