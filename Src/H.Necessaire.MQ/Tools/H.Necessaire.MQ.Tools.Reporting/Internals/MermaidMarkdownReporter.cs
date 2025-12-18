using H.Necessaire.MQ.Tools.DataContracts;
using H.Necessaire.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Tools.Reporting.Internals
{
    [ID(WellKnown.Builders.MermaidMd)]
    internal class MermaidMarkdownReporter : ImAReportBuilder<ModuleInfo[]>
    {
        static DataBinFormatInfo dataBinFormat => new DataBinFormatInfo
        {
            Encoding = Encoding.UTF8.WebName,
            Extension = "md",
            ID = "MermaidMarkdownUTF8",
            MimeType = "text/markdown; charset=UTF-8",
        };

        public Task<DataBin> BuildReport(ReportInfo<ModuleInfo[]> reportInfo)
        {
            DataBinMeta meta = new DataBinMeta
            {
                Name = reportInfo.Name,
                Description = reportInfo.Description,
                Notes = reportInfo.Notes.Push(reportInfo.ToJsonObject().NoteAs("ReportInfoAsJson")),
                Format = dataBinFormat,
            };

            return meta.ToBin(async meta => await BuildReport<ModuleInfo[]>(meta, BuildModuleInfoReport)).AsTask();
        }

        async Task<Stream> BuildModuleInfoReport(ModuleInfo[] modulesInfos)
        {
            if (modulesInfos?.Any() != true)
                return Stream.Null;

            StringBuilder printer
                = new StringBuilder("```mermaid")
                .AppendLine()
                .AppendLine("---")
                .AppendLine("config:")
                .AppendLine("   look: handDrawn")
                .AppendLine("   theme: neutral")
                .AppendLine("---")
                .AppendLine("classDiagram")
                ;

            foreach (ModuleInfo moduleInfo in modulesInfos)
            {
                PrintModuleInfo(moduleInfo, printer);
            }

            printer.Append("```");

            MemoryStream stream = new MemoryStream();

            await printer.ToString().WriteToStreamAsync(stream);

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        void PrintModuleInfo(ModuleInfo moduleInfo, StringBuilder printer)
        {
            printer.Append("    class ").Append(moduleInfo.ID).Append(" { }").AppendLine();

            if (moduleInfo.DependsOn?.Any() != true)
                return;

            foreach (ModuleInfo refModule in moduleInfo.DependsOn)
            {
                PrintModuleInfo(refModule, printer);
                printer.Append("    ").Append(moduleInfo.ID).Append(" <|-- ").Append(refModule.ID).AppendLine();
            }
        }

        async Task<ImADataBinStream> BuildReport<TData>(DataBinMeta meta, Func<TData, Task<Stream>> reportContentBuilder)
        {
            ReportInfo<TData> reportInfo = meta.Notes.Get("ReportInfoAsJson").JsonToObject<ReportInfo<TData>>();
            Stream content = await reportContentBuilder(reportInfo.Data);
            return content.ToDataBinStream();
        }
    }
}
