﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Drachenkatze.PresetMagician.GUI.GUI;
using Drachenkatze.PresetMagician.GUI.Properties;
using Drachenkatze.PresetMagician.GUI.ViewModels;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Newtonsoft.Json;
using Platform.Text;
using Portable.Licensing;
using Portable.Licensing.Validation;
using SplashScreen = Drachenkatze.PresetMagician.GUI.GUI.SplashScreen;
using CommandLine;
using CommandLine.Text;
using Drachenkatze.PresetMagician.Utils;
using System.Security.Permissions;
using System.Windows.Documents;
using Drachenkatze.PresetMagician.GUI.Models;
using Newtonsoft.Json.Linq;

namespace Drachenkatze.PresetMagician.GUI
{
    public class SystemCodeInfo
    {
        public SystemCodeInfo()
        {
        }

        public String MachineName
        {
            get
            {
                return Environment.MachineName;
            }
        }

        public String SystemCode
        {
            get
            {
                return getSystemHash();
            }
        }

        public String getSystemHash()
        {
            ManagementObject os = new ManagementObject("Win32_OperatingSystem=@");
            string serial = (string)os["SerialNumber"];

            return HashUtils.getFormattedSHA256Hash(serial);
        }
    }

    public partial class App : Application
    {
        private SplashScreen splash;

        private delegate void StringParameterDelegate(string value);

        private readonly object stateLock = new object();
        private Options options;

        private async Task DoSomeWork()
        {
            for (int i = 0; i < 100; i++)
            {
                splash.setSplashMessage("Initializing Kittens..." + i);
                Thread.Sleep(50);
            }
        }

        public void processCommandLine(string[] args)
        {
            var parserResult = CommandLine.Parser.Default.ParseArguments<Options>(args);

            parserResult.WithParsed<Options>(options => this.options = options);
            parserResult.WithNotParsed<Options>(errs =>
            {
                var helpText = HelpText.AutoBuild(parserResult, h => h, e =>
                {
                    //Console.WriteLine(e.HelpText.)
                    return e;
                });
                Console.WriteLine(helpText);
            });

            if (options.ForceRegistration)
            {
                var regWindow = new RegistrationWindow();
                regWindow.Show();
            }
        }

        private static void DispatcherExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            LogException(args.Exception, "Exception caught by DispatcherExceptionHandler\n");
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            LogException((Exception)args.ExceptionObject, "Exception caught by UnhandledExceptionHandler\n");
        }

        private static void UnobservedTaskException()
        {
        }

        private static void LogException(Exception e, string type)
        {
            var now = DateTime.Now;
            var dateString = now.ToString("s").Replace(':', '_');

            var outputFile = dateString + " ExceptionLog.txt";
            var outputPath = Path.Combine(getAppDataDir().FullName, @"ExceptionDumps\");
            var fullFileName = Path.Combine(outputPath, outputFile);

            Directory.CreateDirectory(outputPath);
            FileStream s = new FileStream(fullFileName, FileMode.Create);
            byte[] typeMessage = Encoding.ASCII.GetBytes(type);
            byte[] exceptionMessage = Encoding.ASCII.GetBytes(e.ToString());
            byte[] stackTrace = Encoding.ASCII.GetBytes(e.StackTrace.ToString());
            s.Write(typeMessage, 0, typeMessage.Length);
            s.Write(exceptionMessage, 0, exceptionMessage.Length);
            s.Write(stackTrace, 0, stackTrace.Length);

            s.Close();

            MessageBox.Show("An unhandled exception occured. The log has been written to " + fullFileName);
        }

        public void App_start(object sender, StartupEventArgs e)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);
            processCommandLine(e.Args);

            /*splash = new SplashScreen();
            splash.Show();*/

            App.vstPaths = new VSTPathViewModel();
            App.vstPlugins = new VSTPluginViewModel();
            App.vstPresets = new VSTPresetViewModel();
            App.vstHost = new VstHost();

            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            readVSTPathsFromConfig();

            
        }

        

        

        

        public static DirectoryInfo getAppDataDir()
        {
            DirectoryInfo appData = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Drachenkatze\PresetMagician"));

            if (!appData.Exists)
            {
                appData.Create();
            }
            return appData;
        }

        private void readVSTPathsFromConfig()
        {
            Settings set = Settings.Default;

            IEnumerable<string> vstPaths = set.VstPluginPaths.Split(',');

            foreach (string i in vstPaths)
            {
                try
                {
                    App.vstPaths.VstPaths.Add(new DirectoryInfo(i));
                }
                catch (ArgumentException)
                {
                }
            }
        }

        public void App_Exit(object sender, ExitEventArgs e)
        {
            IEnumerable<string> vstPaths;

            vstPaths = from path in App.vstPaths.VstPaths select path.FullName;

            Settings set = Settings.Default;

            set.VstPluginPaths = String.Join(",", vstPaths);

            set.Save();
        }

        public static String getSystemInfo()
        {
            SystemCodeInfo systemInfo = new SystemCodeInfo();

            string output = JsonConvert.SerializeObject(systemInfo);
            return TextConversion.ToBase64String(Encoding.ASCII.GetBytes(output));
        }

        public static void setStatusBar(string status)
        {
            TextBlock textBlock = (TextBlock)Application.Current.MainWindow.FindName("statusMessage");

            if (textBlock != null)
            {
                textBlock.Text = status;
            }
        }

        public static void activateTab(int index)
        {
            TabControl tabControl = (TabControl)Application.Current.MainWindow.FindName("MainTabs");

            tabControl.SelectedIndex = index;
        }

        public static async Task<string> submitPlugins(List<Plugin> pluginsToReport)
        {
            JObject o = JObject.FromObject(new
            {
                pluginSubmission = new
                {
                    email = App.license.Customer.Email,
                    plugins =
                        from p in pluginsToReport
                        orderby p.VstPlugin.PluginId
                        select new
                        {
                            vendorName = p.VstPlugin.PluginVendor,
                            pluginName = p.VstPlugin.PluginName,
                            pluginId = p.VstPlugin.PluginId
                        }
                }
            });

            var submitUrl = Drachenkatze.PresetMagician.GUI.Properties.Resources.ReportPluginsURL;
#if DEBUG
            submitUrl = Drachenkatze.PresetMagician.GUI.Properties.Resources.ReportPluginsURLDebug;
#endif

            HttpContent content = new StringContent(o.ToString());

            HttpClient client = new HttpClient();

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            var response = await client.PostAsync(submitUrl, content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return "Submitted successfully";
            }
            else
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return "An error occured: " + responseString;
            }
        }

        public static VSTPathViewModel vstPaths { get; set; }
        public static VSTPluginViewModel vstPlugins { get; set; }
        public static VSTPresetViewModel vstPresets { get; set; }
        public static VstHost vstHost { get; set; }
    }
}