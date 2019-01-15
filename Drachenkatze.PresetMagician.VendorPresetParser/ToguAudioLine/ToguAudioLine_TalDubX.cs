using System;
using System.Collections.Generic;
using System.IO;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.ToguAudioLine
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class ToguAudioLine_TalDubX : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1685414488};

        private static readonly string ParseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"ToguAudioLine\TAL-Dub-X\presets");
        
        public override string BankLoadingNotes { get; set; } = $"Presets are loaded from {ParseDirectory}. First sub-folder defines the bank.";

        public void ScanBanks()
        {
            DoScan(RootBank, ParseDirectory);
        }
        
        protected void DoScan(PresetBank rootBank, string directory)
        {
            var vc2parser = new VC2Parser(Plugin, "taldub",Presets);
            vc2parser.DoScan(rootBank, directory);
        }
    }
  
}