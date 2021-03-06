using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MAutopanMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1735931700};

        protected override string PresetFile { get; } = "MMultiBandAutopanpresets.xml";

        protected override string RootTag { get; } = "MMultiBandAutopanpresets";
    }
}