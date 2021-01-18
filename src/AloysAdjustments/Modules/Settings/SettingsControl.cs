using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic;
using AloysAdjustments.UI;
using AloysAdjustments.Updates;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Modules.Settings
{
    public partial class SettingsControl : ModuleBase
    {
        public event EmptyEventHandler SettingsOkay;

        public override string ModuleName => "Settings";

        public Updater Updater { get; }

        public SettingsControl()
        {
            InitializeComponent();

            Updater = new Updater();
            UpdateVersion();
        }

        public override Task LoadPatch(string path) => throw new NotImplementedException();
        public override void ApplyChanges(Patch patch) => throw new NotImplementedException();

        public override Task Initialize()
        {
            IoC.Notif.CacheUpdate = UpdateCacheStatus;

            UpdatePatchStatus();
            UpdateCacheStatus();
            CheckUpdates().ConfigureAwait(false);

            tbGameDir.EnableTypingEvent = false;
            tbGameDir.Text = IoC.Settings.GamePath;
            tbGameDir.EnableTypingEvent = true;

            return Task.CompletedTask;
        }

        public bool ValidateAll()
        {
            var settingsValid = UpdateGameDirStatus();
            settingsValid = UpdateArchiverStatus() && settingsValid;
            return settingsValid;
        }
        
        private void tbGameDir_TextChanged(object sender, EventArgs e)
        {
            IoC.Settings.GamePath = tbGameDir.Text;
        }
        private void tbGameDir_TypingFinished(object sender, EventArgs e)
        {
            IoC.Archiver.ClearCache();
            if (UpdateGameDirStatus())
                SettingsOkay?.Invoke();
        }

        private void btnGameDir_Click(object sender, EventArgs e)
        {
            using var ofd = new FolderBrowserDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tbGameDir.EnableTypingEvent = false;
                tbGameDir.Text = ofd.SelectedPath;
                tbGameDir.EnableTypingEvent = true;

                if (UpdateGameDirStatus())
                    SettingsOkay?.Invoke();
            }
        }

        private async void btnArchiver_ClickCommand(object sender, EventArgs e) => await Relay.To(sender, e, btnArchiver_Click);
        private async Task btnArchiver_Click(object sender, EventArgs e)
        {
            using var _ = new ControlLock(btnArchiver);

            if (!UpdateGameDirStatus())
                return;

            IoC.Notif.ShowStatus("Copying Oodle library...");
            await IoC.Archiver.GetLibrary();
            IoC.Notif.ShowStatus("Oodle updated");

            if (UpdateArchiverStatus())
                SettingsOkay?.Invoke();
        }

        private async void btnClearCache_ClickCommand(object sender, EventArgs e) => await Relay.To(sender, e, btnClearCache_Click);
        private async Task btnClearCache_Click(object sender, EventArgs e)
        {
            await Async.Run(() => FileManager.Cleanup(IoC.Config.CachePath));
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
            lblGameDir.ForeColor = valid ? SystemColors.ControlText : UIColors.ErrorColor;

            if (!valid)
                IoC.Notif.ShowError("Missing Game Folder");

            return valid;
        }
        private bool UpdateArchiverStatus()
        {
            var validLib = IoC.Archiver.CheckArchiverLib();
            lblArchiverLib.Text = validLib ? "OK" : "Missing";
            lblArchiverLib.ForeColor = validLib ? UIColors.OkColor : UIColors.ErrorColor;

            if (!validLib)
                IoC.Notif.ShowError("Missing Oodle support library");

            return validLib;
        }

        public bool UpdatePatchStatus()
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
            Async.Run(() =>
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
            }).ConfigureAwait(false);
        }

        private void UpdateVersion()
        {
            var version = Assembly.GetEntryAssembly()?.GetName().Version;
            lblCurrentVersion.Text = version?.ToString(3) ?? "Unknown";
        }

        private async Task CheckUpdates()
        {
            try
            {
                lblUpdateStatus.ForeColor = SystemColors.ControlText;
                lblUpdateStatus.Text = $"Checking for latest version...";

                try
                {
                    await Updater.Cleanup();
                }
                catch { }

                var updates = await Updater.CheckForUpdates();
                
                if (updates.CanUpdate)
                {
                    lblUpdateStatus.Text = "New version available";
                    btnUpdates.Text = "Get Update";
                }
                else
                {
                    lblUpdateStatus.Text = "Running latest version";
                    btnUpdates.Text = "Check Update";
                }

                lblLatestVersion.Text = updates.LastVersion?.ToString() ?? "Unknown";
            }
            catch (Exception ex)
            {
                lblUpdateStatus.ForeColor = UIColors.ErrorColor;
                lblUpdateStatus.Text = $"Error: {ex.Message}";
                lblLatestVersion.Text = "Unknown";
                Errors.WriteError(ex);
            }
        }

        private async void btnUpdates_ClickCommand(object sender, EventArgs e) => await Relay.To(sender, e, btnUpdates_Click);
        private async Task btnUpdates_Click(object sender, EventArgs e)
        {
            if (Updater.Prepared)
            {
                Updater.TryLaunchUpdater(true);
                Environment.Exit(0);
                return;
            }

            if (!Updater.Status.CanUpdate)
            {
                await CheckUpdates();
                return;
            }
            
            lblUpdateStatus.ForeColor = SystemColors.ControlText;
            lblUpdateStatus.Text = $"Downloading update...";
            IoC.Notif.ShowProgress(0);

            try
            {
                await Updater.PrepareUpdate(x => IoC.Notif.ShowProgress(x));

                lblUpdateStatus.Text = "Download complete, restart to update";
                btnUpdates.Text = "Restart";
            }
            catch(Exception ex)
            {
                lblUpdateStatus.ForeColor = UIColors.ErrorColor;
                lblUpdateStatus.Text = $"Error: {ex.Message}";
                Errors.WriteError(ex);
            }

            IoC.Notif.HideProgress();
        }
    }
}
