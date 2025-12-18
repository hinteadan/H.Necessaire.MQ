using System;

namespace H.Necessaire.MQ.Tools.DataContracts
{
    public class ModuleInfo : EphemeralTypeBase, IStringIdentity
    {
        public ModuleInfo() { AsOf = CreatedAt = ValidFrom = DateTime.UtcNow; ValidFor = null; }

        public string ID { get; set; }

        public ModuleInfo[] DependsOn { get; set; }

        public Note[] Notes { get; set; }

        public override string ToString() => ID;
    }
}
