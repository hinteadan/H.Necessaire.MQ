namespace H.Necessaire.MQ.Tools.CodeAnalysis
{
    public static class IoCExtensions
    {
        public static T WithHmqCodeAnalysisTools<T>(this T dependencyRegsitry) where T : ImADependencyRegistry
        {
            dependencyRegsitry.Register<ImACodeAnalyzer>(() => new Internals.CodeAnalyzer());
            return dependencyRegsitry;
        }
    }
}
