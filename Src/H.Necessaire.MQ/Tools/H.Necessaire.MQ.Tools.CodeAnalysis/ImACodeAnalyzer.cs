using H.Necessaire.MQ.Tools.DataContracts;
using System;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Tools.CodeAnalysis
{
    public interface ImACodeAnalyzer
    {
        Task<ModuleInfo[]> ProcessModuleInfoForProjects(Predicate<string> projectsSelector);
    }
}
