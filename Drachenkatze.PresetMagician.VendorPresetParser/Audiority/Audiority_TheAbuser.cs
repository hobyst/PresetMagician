using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Audiority
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Audiority_TheAbuser: Audiority, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1631745363};


        public void ScanBanks()
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                @"Audiority\Presets\The Abuser");

            DoScan(RootBank, directory);
        }
    }
}