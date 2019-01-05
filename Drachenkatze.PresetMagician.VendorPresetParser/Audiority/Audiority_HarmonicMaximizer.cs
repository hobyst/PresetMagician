using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Audiority
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Audiority_HarmonicMaximizer: Audiority, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1097091149};


        public void ScanBanks()
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"Audiority\Presets\Harmonic Maximizer");

            DoScan(RootBank, directory);
        }
    }
}