using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic;
using AloysAdjustments.Modules;
using AloysAdjustments.Modules.Misc;
using AloysAdjustments.Modules.Outfits;
using AloysAdjustments.Modules.Settings;
using AloysAdjustments.UI;
using Decima;
using Decima.HZD;
using AloysAdjustments.Utility;
using Newtonsoft.Json;
using Application = System.Windows.Forms.Application;

namespace AloysAdjustments
{
    public partial class Main : Form
    {
        private const string ConfigPath = "config.json";
        
        private bool _initialized = false;

        private List<ModuleBase> Modules { get; set; }
        private SettingsControl Settings { get; set; }
        
        public Main()
        {
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            IoC.Bind(new Notifications(SetStatus, SetProgress));

            InitializeComponent();
            
            RTTI.SetGameMode(GameType.HZD);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            SetStatus($"Error: {e.ExceptionObject}", true);
            Errors.WriteError(e.ExceptionObject);
        }

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            SetStatus($"Error: {e.Exception.Message}", true);
            Errors.WriteError(e.Exception);
        }

        public void SetStatus(string text, bool error)
        {
            this.TryBeginInvoke(() =>
            {
                tssStatus.Text = text;
                tssStatus.ForeColor = error ? UIColors.ErrorColor : SystemColors.ControlText;
            });
        }

        public void SetProgress(int current, int max, bool unknown, bool visible)
        {
            this.TryBeginInvoke(() =>
            {
                tpbStatus.Visible = visible;
                if (visible)
                {
                    tpbStatus.Style = unknown ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
                    if (!unknown)
                    {
                        tpbStatus.Maximum = max;
                        tpbStatus.Value = current;
                    }
                }
            });
        }

        private async void Main_Load(object sender, EventArgs e)
        {
            IoC.Notif.ShowStatus("Loading config...");
            await LoadConfigs();

            IoC.Bind(new Archiver(new[] { IoC.Config.PatchFile }));
            IoC.Bind(new Localization(ELanguage.English));

            Settings = new SettingsControl();
            Modules = new List<ModuleBase>()
            {
                new OutfitsControl(),
                new UpgradesControl(),
                new MiscControl()
            };

            IoC.Notif.ShowStatus("Loading modules...");
            foreach (var module in Modules.AsEnumerable().Concat(new[] { Settings }).Reverse())
            {
                var tab = new TabPage();
                
                tab.UseVisualStyleBackColor = true;
                tab.Text = module.ModuleName;
                tab.Controls.Add(module);
                module.Dock = DockStyle.Fill;

                tcMain.TabPages.Insert(0, tab);
            }

            await Settings.Initialize();
            Settings.SettingsOkay += Settings_SettingsOkay;

            tcMain.SelectedIndex = 0;
            if (!await Initialize())
            {
                tcMain.SelectedIndex = tcMain.TabPages.Count - 1;
            }

            //await new Updater().Cleanup();
            //await new Updater().PerformUpdate();
        }

        private async void Settings_SettingsOkay()
        {
            if (!_initialized)
                await Initialize();
        }

        private async Task<bool> Initialize()
        {
            if (!Settings.ValidateAll())
                return false;

            IoC.Notif.ShowUnknownProgress();

            IoC.Notif.ShowStatus("Removing old version...");
            await Compatibility.CleanupOldVersions();
            //remove failed patches
            await FileManager.CleanupFile(Configs.PatchPath, true);

            foreach (var module in Modules)
                await module.Initialize();

            _initialized = true;

            if (File.Exists(Configs.PatchPath))
                await LoadExistingPack(Configs.PatchPath, true);

            IoC.Notif.HideProgress();
            IoC.Notif.ShowStatus("Loading complete");
            return true;
        }

        private async void btnPatch_Click(object sender, EventArgs e)
        {
            using var _ = new ControlLock(btnPatch);

            IoC.Notif.ShowStatus("Generating patch...");
            IoC.Notif.ShowUnknownProgress();

            if (Modules.Any() && !Modules.All(x => x.ValidateChanges()))
            {
                IoC.Notif.HideProgress();
                IoC.Notif.ShowStatus("Patch install aborted");
                return;
            }

            //remove failed patches
            await FileManager.CleanupFile(Configs.PatchPath, true);

            using (var oldPatch = new FileRenamer(Configs.PatchPath))
            {
                var patcher = new Patcher();

                var dir = await patcher.SetupPatchDir();
                foreach (var module in Modules)
                {
                    IoC.Notif.ShowStatus($"Generating patch ({module.ModuleName})...");
                    await module.CreatePatch(dir);
                }

                IoC.Notif.ShowStatus("Generating patch (rebuild prefetch)...");
                await Prefetch.RebuildPrefetch(dir);

                var patch = await patcher.PackPatch(dir);

                IoC.Notif.ShowStatus("Copying patch...");
                await patcher.InstallPatch(patch);

                oldPatch.Delete();

#if !DEBUG
                await FileManager.Cleanup(IoC.Config.TempPath);
#endif

                Settings.UpdatePatchStatus();
            }

            IoC.Notif.HideProgress();
            IoC.Notif.ShowStatus("Patch installed");
        }

        private async void btnLoadPatch_Click(object sender, EventArgs e)
        {
            using var _ = new ControlLock(btnLoadPatch);
            using var ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                Multiselect = false,
                Filter = "Pack files (*.bin)|*.bin|All files (*.*)|*.*"
            };

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            await LoadExistingPack(ofd.FileName, false);
        }

        private async Task LoadExistingPack(string path, bool initial)
        {
            IoC.Notif.ShowUnknownProgress();

            foreach (var module in Modules)
                await module.LoadPatch(path);
            
            if (!initial)
            {
                IoC.Settings.LastPackOpen = path;

                IoC.Notif.HideProgress();
                IoC.Notif.ShowStatus($"Loaded pack: {Path.GetFileName(path)}");
            }
        }

        public async Task LoadConfigs()
        {
            var json = await File.ReadAllTextAsync(ConfigPath);
            IoC.Bind(await Task.Run(() => JsonConvert.DeserializeObject<Config>(json)));
            IoC.Bind(await SettingsManager.Load());
        }

        private void tcMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcMain.SelectedIndex >= 0 && tcMain.SelectedIndex < Modules.Count)
            {
                btnReset.Enabled = true;

                for (int i = 0; i < Modules.Count; i++)
                {
                    if (i == tcMain.SelectedIndex)
                        EnableModule(Modules[i]);
                    else
                        DisableModule(Modules[i]);
                }
            }
            else
            {
                btnReset.Enabled = false;
                btnResetSelected.Enabled = false;
            }
        }

        private void EnableModule(IModule module)
        {
            module.Reset.PropertyValueChanged = (p, v) => Relay_PropertyValueChanged(btnReset, p, v);
            //module.Reset.FirePropertyChanges();

            module.ResetSelected.PropertyValueChanged = (p, v) => Relay_PropertyValueChanged(btnResetSelected, p, v);
            module.ResetSelected.FirePropertyChanges();
        }

        private void DisableModule(IModule module)
        {
            module.Reset.PropertyValueChanged = null;
            module.ResetSelected.PropertyValueChanged = null;
        }

        private void Relay_PropertyValueChanged(Control control, string propertyName, object value)
        {
            var pi = control.GetType().GetProperty(propertyName);
            pi?.SetValue(control, value);
        }

        private void btnResetSelected_Click(object sender, EventArgs e)
        {
            if (tcMain.SelectedIndex >= 0 && tcMain.SelectedIndex < Modules.Count)
                Modules[tcMain.SelectedIndex].ResetSelected.OnClick();
        }
        private void btnReset_Click(object sender, EventArgs e)
        {
            if (tcMain.SelectedIndex >= 0 && tcMain.SelectedIndex < Modules.Count)
                Modules[tcMain.SelectedIndex].Reset.OnClick();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Updater?.TryLaunchUpdater(false);
        }
    }
}
