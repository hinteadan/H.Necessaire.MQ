using H.Necessaire.MQ.Tools.DataContracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Tools.CodeAnalysis.Internals
{
    internal class CodeAnalyzer : ImACodeAnalyzer
    {
        const string srcFolderRelativePath = "/Src/H.Necessaire.MQ/";

        public async Task<ModuleInfo[]> ProcessModuleInfoForProjects(Predicate<string> projectSelector)
        {
            int steps = 2;
            ProgressReporter progressReporter = CodeAnalysisProgressiveScope.GetReporter();
            progressReporter.SetSourceInterval(new NumberInterval(0, steps));

            await progressReporter.RaiseOnProgress("Building Solution Context", 0);
            MSBuildWorkspace ws = await NewContext();
            await progressReporter.RaiseOnProgress("DONE Building Solution Context", 1);

            using (ws)
            {
                Project[] projects = ws.CurrentSolution.Projects.Where(p => projectSelector(p.Name)).ToArray();
                if (!projects.Any())
                    return null;

                await progressReporter.RaiseOnProgress("Building Dependecy Tree", 1);
                ModuleInfo[] result = projects.Select(project => BuildModuleInfoFor(project, ws.CurrentSolution)).ToArray();
                await progressReporter.RaiseOnProgress("DONE Building Dependecy Tree", 2);
                return result;
            }
        }

        ModuleInfo BuildModuleInfoFor(Project project, Solution solution, bool includeReferences = true)
        {
            ModuleInfo result = new ModuleInfo
            {
                ID = project.Name,
            };

            if (!includeReferences || project.AllProjectReferences?.Any() != true)
                return result;

            result.DependsOn = new ModuleInfo[project.AllProjectReferences.Count];
            int index = -1;
            foreach (ProjectReference projectRef in project.AllProjectReferences)
            {
                index++;
                Project reffedProject = solution.Projects.SingleOrDefault(p => p.Id == projectRef.ProjectId);
                result.DependsOn[index] = BuildModuleInfoFor(reffedProject, solution);
            }

            return result;
        }

        async Task<MSBuildWorkspace> NewContext(string solutionFile = null)
        {
            MSBuildWorkspace ws = MSBuildWorkspace.Create();

            if (solutionFile.IsEmpty())
            {
                solutionFile = TryToFindSolutionFile();
            }

            if (solutionFile.IsEmpty())
            {
                OperationResult.Fail("Cannot find any solution file").ThrowOnFail();
            }

            await ws.OpenSolutionAsync(solutionFile);

            return ws;
        }

        static string TryToFindSolutionFile(Func<FileInfo[], FileInfo> multipleSolutionsSelector = null)
        {
            string codebaseFolderPath = GetCodebaseFolderPath();

            if (codebaseFolderPath.IsEmpty())
                return null;

            FileInfo[] slns = new DirectoryInfo(codebaseFolderPath).GetFiles("*.sln", SearchOption.AllDirectories);

            if (slns?.Any() != true)
                return null;

            if (slns.Length == 1)
                return slns[0].FullName;

            if (multipleSolutionsSelector != null)
                return multipleSolutionsSelector(slns).FullName;

            return
                slns
                .OrderBy(s => s.DirectoryName.Length)
                .First()
                .FullName;
        }

        static string GetCodebaseFolderPath()
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
