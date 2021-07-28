using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Modules;
using AloysAdjustments.Plugins;
using AloysAdjustments.UI;
using AloysAdjustments.Utility;
using Decima;
using Decima.HZD;
using Microsoft.Win32;
using Newtonsoft.Json;
using Control = System.Windows.Controls.Control;
using ControlLock = AloysAdjustments.UI.ControlLock;
using IInteractivePlugin = AloysAdjustments.Plugins.IInteractivePlugin;
using Localization = AloysAdjustments.Logic.Localization;

namespace AloysAdjustments
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool _initialized = false;

        private List<IInteractivePlugin> Plugins { get; set; }
        private SettingsControl Settings { get; set; }
        public PluginManager PluginManager { get; }
        public IInteractivePlugin ActivePlugin { get; set; }

        public MainWindow()
        {
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            PropertyChanged += (s, e) => Debug.WriteLine(e.PropertyName);

            IoC.Bind(new Notifications(SetStatus, SetAppStatus, SetProgress));
            IoC.Bind(new Uuid());
            Configs.LoadConfigs();

            PluginManager = new PluginManager();

            InitializeComponent();

            WindowMemory.ActivateWindow(this, "Main");
        }
        
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            SetStatus($"Error: {e.ExceptionObject}", true);
            SetProgress(0, 0, false, false);
            Errors.WriteError(e.ExceptionObject);
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            SetStatus($"Error: {e.Exception.Message}", true);
            SetProgress(0, 0, false, false);
            Errors.WriteError(e.Exception);
        }

        public void SetAppStatus(string text)
        {
            text ??= "";
            text = SingleLineConverter.Convert(text);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                tssAppStatus.Text = text;
            }));
        }

        public void SetStatus(string text, bool error)
        {
            text ??= "";
            text = SingleLineConverter.Convert(text);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                tssStatus.Text = text;
                tssStatus.Foreground = error ? 
                    new SolidColorBrush(UIColors.ErrorColor) : 
                    SystemColors.WindowTextBrush;
            }));
        }

        public void SetProgress(int current, int max, bool unknown, bool visible)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                tssAppStatus.Visibility = !visible ? Visibility.Visible : Visibility.Hidden;
                tpbStatus.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
                if (visible)
                {
                    tpbStatus.IsIndeterminate = unknown;
                    if (!unknown)
                    {
                        tpbStatus.Maximum = max;
                        tpbStatus.Value = current;
                    }
                }
            }));
        }

        private async void Main_LoadCommand(object sender, EventArgs e) => await Relay.To(sender, e, Main_Load);
        private async Task Main_Load(object sender, EventArgs e)
        {
            IoC.Notif.ShowStatus("Loading config...");

            IoC.Bind(new Archiver());
            IoC.Bind(new Localization(ELanguage.English));

            Settings = new SettingsControl();
            Plugins = (
                from plugin in PluginManager.LoadAll<IInteractivePlugin>()
                let idx = IoC.Config.PluginLoadOrder.IndexOf(plugin.PluginName)
                let order = idx < 0 ? int.MaxValue : idx
                orderby order, plugin.PluginName
                select plugin).ToList();

            IoC.Notif.ShowStatus("Loading Plugins...");
            foreach (var module in Plugins.AsEnumerable().Concat(new[] { Settings }).Reverse())
            {
                var tab = new TabItem();
                
                tab.Header = module.PluginName;
                tab.Content = module.PluginControl;

                tcMain.Items.Insert(0, tab);
            }

            await Settings.Initialize();
            Settings.SettingsOkay += Settings_SettingsOkayCommand;

            IoC.Notif.ShowUnknownProgress();

            await RunCompatibility();

            tcMain.SelectedIndex = 0;
            if (!await InitializePlugins())
            {
                tcMain.SelectedIndex = tcMain.Items.Count - 1;
            }

            IoC.Notif.HideProgress();
            IoC.Notif.ShowStatus("Loading complete");

            await TryApplyCommands();
            await TryPatchAndExit();
        }

        private async Task TryApplyCommands()
        {
            if (!IoC.CmdOptions.Commands.Any())
                return;

            CmdOutput.Reset();
            var cmds = IoC.CmdOptions.Commands.ToArray();
            for (int i = 0; i < cmds.Length; i+=2)
            {
                var pluginName = cmds[i];
                var cmd = cmds[i+1];

                var plugin = Plugins.FirstOrDefault(x => string.Equals(x.PluginName, pluginName, StringComparison.OrdinalIgnoreCase));
                if (plugin != null)
                    await plugin.CommandAction(cmd);
            }
        }

        private async Task TryPatchAndExit()
        {
            if (!IoC.CmdOptions.BuildPatch)
                return;

            await btnPatch_Click(null, null);
            System.Windows.Application.Current.Shutdown();
        }

        private async void Settings_SettingsOkayCommand() => await Relay.To(Settings_SettingsOkay);
        private async Task Settings_SettingsOkay()
        {
            if (!_initialized)
                await InitializePlugins();

            IoC.Notif.HideProgress();
            IoC.Notif.ShowStatus("Loading complete");
        }
        
        private async Task RunCompatibility()
        {
            await Async.Run(() =>
            {
                IoC.Notif.ShowStatus("Compatibility fixes...");
                AppCompatibility.RunMigrations();
            });

            IoC.Settings.Version = IoC.CurrentVersion.ToString(3);
        }

        private async Task<bool> InitializePlugins()
        {
            if (!Settings.ValidateAll())
                return false;

            await Async.Run(() =>
            {
                IoC.Notif.ShowStatus("Removing old version...");
                //remove failed / old patches
                AppCompatibility.CleanupOldVersions();
                FileBackup.CleanupBackups(Configs.PatchPath);
            });

            IoC.Notif.ShowStatus("Initializing Plugins...");
            foreach (var module in Plugins)
                await module.Initialize();

            _initialized = true;

            if (File.Exists(Configs.PatchPath))
                await LoadExistingPack(Configs.PatchPath, true);
            
            return true;
        }

        private async void btnPatch_ClickCommand(object sender, EventArgs e) => await Relay.To(sender, e, btnPatch_Click);
        private async Task btnPatch_Click(object sender, EventArgs e)
        {
            using var _ = new ControlLock(btnPatch);

            IoC.Notif.ShowStatus("Generating patch...");
            IoC.Notif.ShowUnknownProgress();

            if (Plugins.Any() && !Plugins.All(x => x.ValidateChanges()))
            {
                IoC.Notif.HideProgress();
                IoC.Notif.ShowStatus("Pack install aborted");
                return;
            }

            var sw = new Stopwatch(); sw.Start();

            var success = await Async.Run(CreatePatch);

            Settings.UpdatePatchStatus();

            IoC.Notif.HideProgress();
            if (success)
                IoC.Notif.ShowStatus($"Patch installed ({sw.Elapsed.TotalMilliseconds:n0} ms)");
        }

        private bool CreatePatch()
        {
            //remove failed patches
            FileBackup.CleanupBackups(Configs.PatchPath);

            using (var oldPatch = new FileBackup(Configs.PatchPath))
            {
                var patcher = new Patcher();

                var patch = patcher.StartPatch();
                foreach (var module in Plugins)
                {
                    IoC.Notif.ShowStatus($"Generating patch ({module.PluginName})...");
                    module.ApplyChanges(patch);
                }

                IoC.Notif.ShowStatus("Generating plugin patches...");
                patcher.ApplyCustomPatches(patch, PluginManager);

                FileCompatibility.SaveVersion(patch);

                IoC.Notif.ShowStatus("Generating patch (rebuild prefetch)...");

                var p = Prefetch.Load(patch);
                if (p.Rebuild(patch))
                    p.Save();

                if (!Directory.Exists(patch.WorkingDir))
                {
                    IoC.Notif.ShowStatus("No changes found, aborting pack creation.");
                    
                    return false;
                }

                IoC.Notif.ShowStatus("Generating patch (packing)...");
                patcher.PackPatch(patch);
                
                IoC.Notif.ShowStatus("Copying patch...");
                patcher.InstallPatch(patch);

                oldPatch.Delete();

#if !DEBUG
                Paths.Cleanup(IoC.Config.TempPath);
#endif
            }

            return true;
        }

        private async void btnLoadPatch_ClickCommand(object sender, EventArgs e) => await Relay.To(sender, e, btnLoadPatch_Click);
        private async Task btnLoadPatch_Click(object sender, EventArgs e)
        {
            using var _ = new ControlLock(btnLoadPatch);
            var ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                Multiselect = false,
                Filter = "Pack files (*.bin)|*.bin|All files (*.*)|*.*"
            };

            if (ofd.ShowDialog() != true)
                return;

            await LoadExistingPack(ofd.FileName, false);
        }

        private async Task LoadExistingPack(string path, bool initial)
        {
            IoC.Notif.ShowUnknownProgress();

            foreach (var module in Plugins)
                await module.LoadPatch(path);

            if (!initial)
            {
                IoC.Settings.LastPackOpen = path;

                IoC.Notif.HideProgress();
                IoC.Notif.ShowStatus($"Loaded pack: {Path.GetFileName(path)}");
            }
        }

        private void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tcMain.SelectedIndex >= 0 && tcMain.SelectedIndex < Plugins.Count)
                ActivePlugin = Plugins[tcMain.SelectedIndex];
            else
                ActivePlugin = Settings;
        }

        private void btnResetSelected_Click(object sender, EventArgs e)
        {
            ActivePlugin?.ResetSelected?.OnClick();
            ActivePlugin?.ResetSelected?.ClickCommand?.Execute(null);
        }
        private void btnReset_Click(object sender, EventArgs e)
        {
            ActivePlugin?.Reset?.OnClick();
            ActivePlugin?.Reset?.ClickCommand?.Execute(null);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Updater?.TryLaunchUpdater(false);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
