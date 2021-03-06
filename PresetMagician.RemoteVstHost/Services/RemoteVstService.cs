﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Security;
using System.ServiceModel;
using Drachenkatze.PresetMagician.Utils;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Models.Audio;
using PresetMagician.Core.Models.MIDI;
using PresetMagician.RemoteVstHost.Faults;
using PresetMagician.Utils.Logger;
using PresetMagician.VstHost.VST;

namespace PresetMagician.RemoteVstHost.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single,
        UseSynchronizationContext = true, IncludeExceptionDetailInFaults = true)]
    public class RemoteVstService : IRemoteVstService
    {
        private static readonly VstHost.VST.VstHost _vstHost = new VstHost.VST.VstHost(true);
        private readonly Dictionary<Guid, RemoteVstPlugin> _plugins = new Dictionary<Guid, RemoteVstPlugin>();

        public bool Ping()
        {
            App.Ping();
            return true;
        }

        private FaultException<T> GetFaultException<T>() where T : IGenericFault, new()
        {
            var internalFault = new T();
            return new FaultException<T>(internalFault, new FaultReason(internalFault.Message));
        }

        private FaultException<T> GetFaultException<T>(Exception innerException) where T : IGenericFault, new()
        {
            var internalFault = new T {InnerException = innerException};
            return new FaultException<T>(internalFault,
                new FaultReason($"{innerException.GetType().FullName}: {innerException.Message}"));
        }

        public Guid RegisterPlugin(string dllPath, bool backgroundProcessing = true)
        {
            App.Ping();
            var guid = Guid.NewGuid();
            var logFile = VstUtils.GetWorkerPluginLog(Process.GetCurrentProcess().Id, guid);

            var plugin = new RemoteVstPlugin
            {
                DllPath = dllPath, BackgroundProcessing = backgroundProcessing,
                Logger = new MiniDiskLogger(logFile)
            };

            _plugins.Add(guid, plugin);

            return guid;
        }

        public void UnregisterPlugin(Guid guid)
        {
            App.Ping();

            var plugin = GetPluginByGuid(guid);
            if (plugin.IsLoaded)
            {
                _vstHost.UnloadVst(plugin);
            }

            _plugins.Remove(guid);


            if (plugin.Logger is MiniDiskLogger miniDiskLogger)
            {
                if (File.Exists(miniDiskLogger.LogFilePath))
                {
                    File.Delete(miniDiskLogger.LogFilePath);
                }
            }
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public void LoadPlugin(Guid guid, bool debug = false)
        {
            App.Ping();
            var plugin = GetPluginByGuid(guid);
            plugin.Logger.Debug($"LoadPlugin()");

            try
            {
                _vstHost.LoadVst(plugin, debug);
            }
            catch (EntryPointNotFoundException)
            {
                throw GetFaultException<NoEntryPointFoundFault>();
            }
            catch (AccessViolationException)
            {
                throw GetFaultException<AccessViolationFault>();
            }
            catch (Exception gex)
            {
                throw GetFaultException<GenericFault>(gex);
            }
        }

        public void PatchPluginToAudioOutput(Guid guid, AudioOutputDevice device, int latency)
        {
            var plugin = GetPluginByGuid(guid);

            try
            {
                _vstHost.PatchPluginToAudioOutput(plugin, device, latency);
            }
            catch (Exception e)
            {
                throw GetFaultException<GenericFault>(e);
            }
        }

        public void PerformIdleLoop(Guid guid, int loops)
        {
            var plugin = GetPluginByGuid(guid);

            _vstHost.PerformIdleLoop(plugin, loops);
        }

        public void UnpatchPluginFromAudioOutput()
        {
            try
            {
                _vstHost.UnpatchPluginFromAudioOutput();
            }
            catch (Exception e)
            {
                throw GetFaultException<GenericFault>(e);
            }
        }

        public void PatchPluginToMidiInput(Guid guid, MidiInputDevice device)
        {
            var plugin = GetPluginByGuid(guid);

            try
            {
                _vstHost.PatchPluginToMidiInput(plugin, device);
            }
            catch (Exception e)
            {
                throw GetFaultException<GenericFault>(e);
            }
        }

        public void UnpatchPluginFromMidiInput()
        {
            try
            {
                _vstHost.UnpatchPluginFromMidiInput();
            }
            catch (Exception e)
            {
                throw GetFaultException<GenericFault>(e);
            }
        }

        public void UnloadPlugin(Guid guid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(guid);

            plugin.Logger.Debug($"UnloadPlugin()");
            if (plugin.IsLoaded)
            {
                _vstHost.UnloadVst(plugin);
            }
        }

        public void ReloadPlugin(Guid guid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(guid);
            _vstHost.ReloadPlugin(plugin);
        }

        public void DisableTimeInfo(Guid guid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(guid);
            _vstHost.DisableTimeInfo(plugin);
        }

        public void EnableTimeInfo(Guid guid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(guid);
            _vstHost.DisableTimeInfo(plugin);
        }

        private RemoteVstPlugin GetPluginByGuid(Guid guid)
        {
            App.Ping();
            if (!_plugins.ContainsKey(guid))
            {
                throw GetFaultException<PluginNotRegisteredFault>();
            }

            return _plugins[guid];
        }

        public bool OpenEditorHidden(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw GetFaultException<PluginNotLoadedFault>();
            }

            bool result;

            try
            {
                result = plugin.OpenEditorHidden();
            }
            catch (Exception e)
            {
                throw GetFaultException<GenericFault>(e);
            }

            return result;
        }

        public bool OpenEditor(Guid pluginGuid, bool topmost = true)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw GetFaultException<PluginNotLoadedFault>();
            }

            bool result;

            try
            {
                result = plugin.OpenEditor(topmost);
            }
            catch (Exception e)
            {
                throw GetFaultException<GenericFault>(e);
            }

            return result;
        }

        public void CloseEditor(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw GetFaultException<PluginNotLoadedFault>();
            }

            plugin.CloseEditor();
        }

        public byte[] CreateScreenshot(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw GetFaultException<PluginNotLoadedFault>();
            }

            if (!plugin.IsEditorOpen)
            {
                throw GetFaultException<PluginEditorNotOpenFault>();
            }

            var ms = new MemoryStream();

            try
            {
                var bitmap = plugin.CreateScreenshot();

                if (bitmap == null)
                {
                    return null;
                }

                bitmap.Save(ms, ImageFormat.Png);
            }
            catch (Exception e)
            {
                throw GetFaultException<GenericFault>(e);
            }

            return ms.ToArray();
        }


        public string GetPluginName(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw GetFaultException<PluginNotLoadedFault>();
            }

            return plugin.PluginContext.PluginCommandStub.GetEffectName();
        }

        public string GetEffectivePluginName(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw GetFaultException<PluginNotLoadedFault>();
            }

            var pluginName = plugin.PluginContext.PluginCommandStub.GetEffectName();

            if (string.IsNullOrEmpty(pluginName))
            {
                pluginName = plugin.PluginContext.PluginCommandStub.GetProductString();

                if (string.IsNullOrEmpty(pluginName))
                {
                    // Extreme fallback: Use plugin DLL name
                    pluginName = plugin.DllFilename.Replace(".dll", "");
                }
            }

            return pluginName;
        }

        public int GetPluginVendorVersion(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw GetFaultException<PluginNotLoadedFault>();
            }

            return plugin.PluginContext.PluginCommandStub.GetVendorVersion();
        }

        public string GetPluginProductString(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw GetFaultException<PluginNotLoadedFault>();
            }

            return plugin.PluginContext.PluginCommandStub.GetProductString();
        }

        public string GetPluginVendor(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw GetFaultException<PluginNotLoadedFault>();
            }

            var pluginVendor = plugin.PluginContext.PluginCommandStub.GetVendorString();

            if (string.IsNullOrEmpty(pluginVendor))
            {
                pluginVendor = "Unknown";
            }

            return pluginVendor;
        }

        public List<PluginInfoItem> GetPluginInfoItems(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw GetFaultException<PluginNotLoadedFault>();
            }

            return plugin.GetPluginInfoItems(plugin.PluginContext);
        }

        public VstPluginInfoSurrogate GetPluginInfo(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw GetFaultException<PluginNotLoadedFault>();
            }

            var vstInfo = new VstPluginInfoSurrogate(plugin.PluginContext.PluginInfo);


            return vstInfo;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public void SetProgram(Guid pluginGuid, int program)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);
            plugin.Logger.Debug($"SetProgram()");
            if (!plugin.IsLoaded)
            {
                throw GetFaultException<PluginNotLoadedFault>();
            }

            try
            {
                plugin.PluginContext.PluginCommandStub.SetProgram(program);
            }
            catch (AccessViolationException)
            {
                Console.WriteLine("Got access violation");
                throw GetFaultException<AccessViolationFault>();
            }
        }

        public string GetCurrentProgramName(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw GetFaultException<PluginNotLoadedFault>();
            }

            return plugin.PluginContext.PluginCommandStub.GetProgramName();
        }

        public byte[] GetChunk(Guid pluginGuid, bool isPreset)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw GetFaultException<PluginNotLoadedFault>();
            }

            return plugin.PluginContext.PluginCommandStub.GetChunk(isPreset);
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public void SetChunk(Guid pluginGuid, byte[] data, bool isPreset)
        {
            if (data == null)
            {
                throw GetFaultException<PresetDataNullFault>();
            }

            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);
            if (!plugin.IsLoaded)
            {
                throw GetFaultException<PluginNotLoadedFault>();
            }

            try
            {
                plugin.PluginContext.PluginCommandStub.SetChunk(data, isPreset);
            }
            catch (AccessViolationException)
            {
                throw GetFaultException<AccessViolationFault>();
            }
        }

        public float GetParameter(Guid pluginGuid, int parameterIndex)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);
            if (!plugin.IsLoaded)
            {
                throw GetFaultException<PluginNotLoadedFault>();
            }


            return plugin.PluginContext.PluginCommandStub.GetParameter(parameterIndex);
        }

        public void ExportNksAudioPreview(Guid pluginGuid, PresetExportInfo preset, byte[] presetData,
            int initialDelay)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);
            if (!plugin.IsLoaded)
            {
                throw GetFaultException<PluginNotLoadedFault>();
            }

            try
            {
                var exporter = new NKSExport(_vstHost);
                exporter.ExportPresetAudioPreviewRealtime(plugin, preset, presetData, initialDelay);
            }
            catch (Exception e)
            {
                throw GetFaultException<GenericFault>(e);
            }
        }

        public void ExportNks(Guid pluginGuid, PresetExportInfo preset, byte[] presetData)
        {
            App.Ping();
            try
            {
                var exporter = new NKSExport(_vstHost);
                exporter.ExportNKSPreset(preset, presetData);
            }
            catch (Exception e)
            {
                throw GetFaultException<GenericFault>(e);
            }
        }

        public bool Exists(string file)
        {
            App.Ping();

            try
            {
                return File.Exists(file);
            }
            catch (Exception e)
            {
                throw GetFaultException<GenericFault>(e);
            }
        }

        public long GetSize(string file)
        {
            App.Ping();

            try
            {
                var fileInfo = new FileInfo(file);
                return fileInfo.Length;
            }
            catch (Exception e)
            {
                throw GetFaultException<GenericFault>(e);
            }
        }

        public string GetHash(string file)
        {
            App.Ping();
            try
            {
                var data = File.ReadAllBytes(file);
                var hash = HashUtils.getIxxHash(data);
                GC.Collect();
                return hash;
            }
            catch (Exception e)
            {
                throw GetFaultException<GenericFault>(e);
            }
        }

        public DateTime GetLastModifiedDate(string file)
        {
            App.Ping();
            try
            {
                return File.GetLastWriteTime(file);
            }
            catch (Exception e)
            {
                throw GetFaultException<GenericFault>(e);
            }
        }

        public byte[] GetContents(string file)
        {
            App.Ping();

            try
            {
                return File.ReadAllBytes(file);
            }
            catch (Exception e)
            {
                throw GetFaultException<GenericFault>(e);
            }
        }

        public void KillSelf()
        {
            App.KillSelf();
        }
    }
}