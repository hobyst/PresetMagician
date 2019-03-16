using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class u_he_Runciter : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1382232375};

        protected override string GetProductName()
        {
            return "Runciter";
        }
        
        protected override string GetDataDirectoryName()
        {
            return "Uhbik.data";
        }
    }
}