using System.Threading.Tasks;

namespace H.Necessaire.MQ.Tools.Reporting
{
    public interface ImAReportBuilder<TData>
    {
        Task<DataBin> BuildReport(ReportInfo<TData> reportInfo);
    }
}
