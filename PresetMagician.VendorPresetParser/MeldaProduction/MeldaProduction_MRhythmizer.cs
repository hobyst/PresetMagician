using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MRhythmizer : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297246329};

        protected override string PresetFile { get; } = "MRhythmizerpresets.xml";

        protected override string RootTag { get; } = "MRhythmizerpresetspresets";
    }
}