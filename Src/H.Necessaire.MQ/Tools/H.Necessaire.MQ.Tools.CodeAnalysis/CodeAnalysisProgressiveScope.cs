namespace H.Necessaire.MQ.Tools.CodeAnalysis
{
    public class CodeAnalysisProgressiveScope : ProgressiveScope
    {
        public CodeAnalysisProgressiveScope(AsyncEventHandler<ProgressEventArgs> onProgress = null) : base("CodeAnalysisProgressiveScope", onProgress)
        {
        }

        public static ProgressReporter GetReporter() => ProgressReporter.Get("CodeAnalysisProgressiveScope") ?? new ProgressReporter();
    }
}
