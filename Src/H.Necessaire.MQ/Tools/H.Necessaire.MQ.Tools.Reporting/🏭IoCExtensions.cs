namespace H.Necessaire.MQ.Tools.Reporting
{
    public static class IoCExtensions
    {
        public static T WithHmqReportingTools<T>(this T dependencyRegsitry) where T : ImADependencyRegistry
        {
            //dependencyRegsitry.Register<ImACodeAnalyzer>(() => new Internals.CodeAnalyzer());
            return dependencyRegsitry;
        }
    }
}
