﻿using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Enums;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser
{
    [UsedImplicitly]
    public class VoidPresetParser : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int>
        {
            1951355500,
            1919243824, // u-he Zrev
            1364228170,
            1098206310, // Audiority Low Filter
            1098208102, // Audiority Side Filter
            1483101268, // Xfer OTT
            1936223602, // ValhallaDSP Shimmer
            1181828456, // ValhallaDSP FreqEcho
            1919243828, // TAL Reverb 4
            1665682481, // Tal Chorus LX
            1296985972, // MeldaProduction MNotepad
            1297052499, // MeldaProduction Oscilloscope
            1297245539, // MeldaProduction Recorder
            1297306467, // MeldaProduction StereoScope
            1297380718 // MeldaProduction Tuner
        };
        
        public override PresetParserPriorityEnum Priority { get; } = PresetParserPriorityEnum.VOID_PRIORITY;

        public override string Remarks { get; set; } =
            "Plugin doesn't seem to have preset loading/saving capabilities";
    }
}