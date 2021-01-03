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

        public void SetStatus(string text, bool error = false)
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
                        tpbStatus.Value = current;
                        tpbStatus.Maximum = max;
                    }
                }
            });
        }

        private async void Main_Load(object sender, EventArgs e)
        {
            SetStatus("Loading config...");
            await LoadConfigs();

            IoC.Bind(new Archiver(new[] { IoC.Config.PatchFile }));
            IoC.Bind(new Localization(ELanguage.English));
            var notif = new Notifications(SetStatus, SetProgress)
            {
                CacheUpdate = UpdateCacheStatus
            };
            IoC.Bind(notif);

            tbGameDir.EnableTypingEvent = false;
            tbGameDir.Text = IoC.Settings.GamePath;
            tbGameDir.EnableTypingEvent = true;

            Modules = new List<ModuleBase>()
            {
                new OutfitsControl(),
                new UpgradesControl()
            };

            foreach (var module in Modules.AsEnumerable().Reverse())
            {
                var tab = new TabPage();

                tab.Text = module.ModuleName;
                tab.Controls.Add(module);
                module.Dock = DockStyle.Fill;

                tcMain.TabPages.Insert(0, tab);
            }

            UpdatePatchStatus();
            UpdateCacheStatus();

            tcMain.SelectedIndex = 0;
            if (!await Initialize())
                tcMain.SelectedIndex = tcMain.TabPages.Count - 1;
        }

        private async Task<bool> Initialize()
        {
            var settingsValid = UpdateGameDirStatus();
            settingsValid = UpdateArchiverStatus() && settingsValid;

            if (!settingsValid)
                return false;

            IoC.Notif.ShowUnknownProgress();

            foreach (var module in Modules)
            {
                await module.Initialize();
            }

            _initialized = true;
            
            IoC.Notif.HideProgress();
            SetStatus("Loading complete");
            return true;
        }

        private async void btnPatch_Click(object sender, EventArgs e)
        {
            using var _ = new ControlLock(btnPatch);

            SetStatus("Generating patch...");
            IoC.Notif.ShowUnknownProgress();

            if (Modules.Any() && !Modules.All(x => x.ValidateChanges()))
            {
                IoC.Notif.HideProgress();
                SetStatus("Patch install aborted");
                return;
            }

            using (var oldPatch = new FileRenamer(Configs.PatchPath))
            {
                var patcher = new Patcher();

                var dir = await patcher.SetupPatchDir();
                foreach (var module in Modules)
                {
                    SetStatus($"Generating patch ({module.ModuleName})...");
                    await module.CreatePatch(dir);
                }

                SetStatus("Generating patch (rebuild prefetch)...");
                await Prefetch.RebuildPrefetch(dir);

                var patch = await patcher.PackPatch(dir);

                SetStatus("Copying patch...");
                await patcher.InstallPatch(patch);

                oldPatch.Delete();

                //await FileManager.Cleanup(IoC.Config.TempPath);

                UpdatePatchStatus();
            }

            IoC.Notif.HideProgress();
            SetStatus("Patch installed");
        }

        private async void btnArchiver_Click(object sender, EventArgs e)
        {
            using var _ = new ControlLock(btnArchiver);

            if (!UpdateGameDirStatus())
                return;
            
            SetStatus("Copying Oodle library...");
            await IoC.Archiver.GetLibrary();
            SetStatus("Oodle updated");
            
            if (UpdateArchiverStatus() && !_initialized)
                await Initialize();
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

            IoC.Notif.ShowUnknownProgress();

            foreach (var module in Modules)
                await module.LoadPatch(ofd.FileName);

            IoC.Settings.LastPackOpen = ofd.FileName;

            IoC.Notif.HideProgress();
            SetStatus($"Loaded pack: {Path.GetFileName(ofd.FileName)}");
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

        private bool UpdateGameDirStatus()
        {
            var dir = Configs.GamePackDir;
            var valid = dir != null && Directory.Exists(dir);
            lblGameDir.ForeColor = valid ? SystemColors.ControlText : _errorColor;

            if (!valid)
                SetStatus("Missing Game Folder", true);

            return valid;
        }
        private bool UpdateArchiverStatus()
        {
            var validLib = IoC.Archiver.CheckArchiverLib();
            lblArchiverLib.Text = validLib ? "OK" : "Missing";
            lblArchiverLib.ForeColor = validLib ? _okColor : _errorColor;

            if (!validLib)
                SetStatus("Missing Oodle support library", true);

            return validLib;
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
                });
            });
        }

        private async void btnClearCache_Click(object sender, EventArgs e)
        {
            await FileManager.Cleanup(IoC.Config.CachePath);
            UpdateCacheStatus();
        }
    }
}
