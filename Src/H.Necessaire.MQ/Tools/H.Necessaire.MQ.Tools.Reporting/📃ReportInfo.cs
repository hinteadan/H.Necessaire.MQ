namespace H.Necessaire.MQ.Tools.Reporting
{
    public class ReportInfo<TData>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Note[] Notes { get; set; }

        public TData Data { get; set; }
    }
}
