using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Anotar.Catel;
using Drachenkatze.PresetMagician.VSTHost.VST;
using GSF;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Common
{
    public class VC2Parser: RecursiveBankDirectoryParser
    {
        protected Regex XmlHeaderReplacerRegex;
        protected Func<string, string> PreProcessXmlFunc = null;
        
        public VC2Parser(IVstPlugin vstPlugin, string extension, ObservableCollection<Preset> presets):
            base(vstPlugin, extension, presets)
        {
            XmlHeaderReplacerRegex = new Regex(@"<\?xml.*?\?>", RegexOptions.Compiled);
        }

        public void SetPreProcessXmlFunction(Func<string, string> func)
        {
            PreProcessXmlFunc = func;
        }

        protected override void ProcessFile(string fileName, Preset preset)
        {
            var data = File.ReadAllText(fileName);

            if (PreProcessXmlFunc != null)
            {
                data = PreProcessXmlFunc.Invoke(data);
            }
            var xmlWithoutHeader = XmlHeaderReplacerRegex.Replace(data, "");

            preset.PresetData = WriteVC2(xmlWithoutHeader).ToArray();
        }

        public static MemoryStream WriteVC2(string pluginData)
        {
            var ms = new MemoryStream();
            
            ms.Write(new byte[] { 0x56, 0x43, 0x32, 0x21 }, 0, 4);
            byte[] data = Encoding.UTF8.GetBytes(pluginData);
            ms.Write(LittleEndian.GetBytes(data.Length), 0, 4);
            ms.Write(data, 0, data.Length);
            ms.WriteByte(0);

            return ms;
        }
    }
}