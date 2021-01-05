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

        private readonly Color _errorColor = Color.FromArgb(204, 0, 0);
        private readonly Color _okColor = Color.ForestGreen;
        
        private bool _initialized = false;

        private List<ModuleBase> Modules { get; set; }
        
        public Main()
        {
            InitializeComponent();
            
            RTTI.SetGameMode(GameType.HZD);

            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            SetStatus($"Error: {e.ExceptionObject}", true);
            File.AppendAllText("error.log", $"{e.ExceptionObject}\r\n");
        }

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            SetStatus($"Error: {e.Exception.Message}", true);
            File.AppendAllText("error.log", $"{e.Exception}\r\n");
        }

        public void SetStatus(string text, bool error)
        {
            this.TryBeginInvoke(() =>
            {
                tssStatus.Text = text;
                tssStatus.ForeColor = error ? _errorColor : SystemColors.ControlText;
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
            var notif = new Notifications(SetStatus, SetProgress)
            {
                CacheUpdate = UpdateCacheStatus
            };
            IoC.Bind(notif);

            IoC.Notif.ShowStatus("Loading config...");
            await LoadConfigs();

            IoC.Bind(new Archiver(new[] { IoC.Config.PatchFile }));
            IoC.Bind(new Localization(ELanguage.English));


            tbGameDir.EnableTypingEvent = false;
            tbGameDir.Text = IoC.Settings.GamePath;
            tbGameDir.EnableTypingEvent = true;

            Modules = new List<ModuleBase>()
            {
                new OutfitsControl(),
                new UpgradesControl(),
                new MiscControl()
            };

            IoC.Notif.ShowStatus("Loading modules...");
            foreach (var module in Modules.AsEnumerable().Reverse())
            {
                var tab = new TabPage();
                
                tab.UseVisualStyleBackColor = true;
                tab.Text = module.ModuleName;
                tab.Controls.Add(module);
                module.Dock = DockStyle.Fill;

                tcMain.TabPages.Insert(0, tab);
            }

            UpdatePatchStatus();
            UpdateCacheStatus();

            tcMain.SelectedIndex = 0;
            if (!await Initialize())
            {
                tcMain.SelectedIndex = tcMain.TabPages.Count - 1;
            }
        }

        private async Task<bool> Initialize()
        {
            var settingsValid = UpdateGameDirStatus();
            settingsValid = UpdateArchiverStatus() && settingsValid;

            if (!settingsValid)
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

                //await FileManager.Cleanup(IoC.Config.TempPath);

                UpdatePatchStatus();
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
        
        private void tbGameDir_TextChanged(object sender, EventArgs e)
        {
            IoC.Settings.GamePath = tbGameDir.Text;
        }
        private async void tbGameDir_TypingFinished(object sender, EventArgs e)
        {
            IoC.Archiver.ClearCache();
            if (UpdateGameDirStatus() && !_initialized)
                await Initialize();
        }

        private async void btnGameDir_Click(object sender, EventArgs e)
        {
            using var ofd = new FolderBrowserDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tbGameDir.EnableTypingEvent = false;
                tbGameDir.Text = ofd.SelectedPath;
                tbGameDir.EnableTypingEvent = true;

                if (UpdateGameDirStatus() && !_initialized)
                    await Initialize();
            }
        }

        private async void btnArchiver_Click(object sender, EventArgs e)
        {
            using var _ = new ControlLock(btnArchiver);

            if (!UpdateGameDirStatus())
                return;

            IoC.Notif.ShowStatus("Copying Oodle library...");
            await IoC.Archiver.GetLibrary();
            IoC.Notif.ShowStatus("Oodle updated");

            if (UpdateArchiverStatus() && !_initialized)
                await Initialize();
        }

        private async void btnClearCache_Click(object sender, EventArgs e)
        {
            await FileManager.Cleanup(IoC.Config.CachePath);
            UpdateCacheStatus();
        }

        private void btnDeletePack_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to delete {IoC.Config.PatchFile}?", $"Delete File", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            if (File.Exists(Configs.PatchPath))
                File.Delete(Configs.PatchPath);

            UpdatePatchStatus();
        }

        private bool UpdateGameDirStatus()
        {
            var dir = Configs.GamePackDir;
            var valid = dir != null && Directory.Exists(dir);
            lblGameDir.ForeColor = valid ? SystemColors.ControlText : _errorColor;

            if (!valid)
                IoC.Notif.ShowError("Missing Game Folder");

            return valid;
        }
        private bool UpdateArchiverStatus()
        {
            var validLib = IoC.Archiver.CheckArchiverLib();
            lblArchiverLib.Text = validLib ? "OK" : "Missing";
            lblArchiverLib.ForeColor = validLib ? _okColor : _errorColor;

            if (!validLib)
                IoC.Notif.ShowError("Missing Oodle support library");

            return validLib;
        }

        private bool UpdatePatchStatus()
        {
            var valid = File.Exists(Configs.PatchPath);
            if (valid)
                lblPackStatus.Text = $"Pack installed: {IoC.Config.PatchFile}";
            else
                lblPackStatus.Text = "Pack not installed";

            btnDeletePack.Enabled = valid;

            return valid;
        }

        private void UpdateCacheStatus()
        {
            Task.Run(() =>
            {
                var size = 0L;
                var dir = new DirectoryInfo(IoC.Config.CachePath);
                if (dir.Exists)
                    size = dir.GetFiles("*.json", SearchOption.AllDirectories).Sum(x => x.Length);

                this.TryBeginInvoke(() =>
                {
                    lblCacheSize.Text = $"{(size / 1024):n0} KB";
                    btnClearCache.Enabled = size > 0;
                });
            });
        }
    }
}
