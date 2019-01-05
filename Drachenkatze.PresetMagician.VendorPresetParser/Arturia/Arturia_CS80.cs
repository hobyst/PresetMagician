using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public class Arturia_CS80: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1129535027 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"CS-80"};
            ScanPresets(instruments);
        }
    }
}