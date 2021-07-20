using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Steam;
using AloysAdjustments.UI;
using AloysAdjustments.Updates;
using AloysAdjustments.Utility;
using Ookii.Dialogs.Wpf;
using MessageBox = System.Windows.MessageBox;
using UIColors = AloysAdjustments.UI.UIColors;

namespace AloysAdjustments.Modules
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl
    {
        private const string SteamGameName = "Horizon Zero Dawn";

        public event EmptyEventHandler SettingsOkay;

        public override string PluginName => "Settings";

        public Updater Updater { get; }

        public SettingsControl()
        {
            InitializeComponent();

            Reset.Enabled = false;
            ResetSelected.Enabled = false;

            Updater = new Updater();
            UpdateVersion();
        }

        public override Task LoadPatch(string path) => throw new NotImplementedException();
        public override void ApplyChanges(Patch patch) => throw new NotImplementedException();

        public override async Task Initialize()
        {
            IoC.Notif.CacheUpdate = UpdateCacheStatus;

            await InitializeFirstRun();

            UpdatePatchStatus();
            UpdateCacheStatus();
            CheckUpdates().Forget();

            tbGameDir.EnableTypingEvent = false;
            tbGameDir.Text = IoC.Settings.GamePath;
            tbGameDir.EnableTypingEvent = true;
        }

        private async Task InitializeFirstRun()
        {
            if (Configs.GamePackDir != null)
                return;

            var gameDir = new GameSearch().FindSteamGameDir(SteamGameName);
            if (gameDir == null)
                return;

            IoC.Settings.GamePath = gameDir;
            if (!UpdateGameDirStatus())
                IoC.Settings.GamePath = "";

            await IoC.Archiver.GetLibrary();
        }

        public bool ValidateAll()
        {
            var settingsValid = UpdateGameDirStatus();
            settingsValid = UpdateArchiverStatus() && settingsValid;
            return settingsValid;
        }

        private void tbGameDir_TextChanged(object sender, TextChangedEventArgs e)
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
            var ofd = new VistaFolderBrowserDialog();

            if (ofd.ShowDialog() == true)
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
            await Async.Run(() => Paths.Cleanup(IoC.Config.CachePath));
            UpdateCacheStatus();
        }

        private void btnDeletePack_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to delete {IoC.Config.PatchFile}?", $"Delete File",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
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
            lblGameDir.Foreground = valid ? SystemColors.WindowTextBrush : UIColors.ErrorBrush;

            if (!valid)
                IoC.Notif.ShowError("Missing Game Folder");

            return valid;
        }
        private bool UpdateArchiverStatus()
        {
            var validLib = IoC.Archiver.CheckArchiverLib();
            lblArchiverLib.Text = validLib ? "OK" : "Missing";
            lblArchiverLib.Foreground = validLib ? UIColors.OkBrush : UIColors.ErrorBrush;

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

            btnDeletePack.IsEnabled = valid;

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

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    lblCacheSize.Text = $"{(size / 1024):n0} KB";
                    btnClearCache.IsEnabled = size > 0;
                }));
            }).ConfigureAwait(false);
        }

        private void UpdateVersion()
        {
            lblCurrentVersion.Text = IoC.CurrentVersion?.ToString(3) ?? "Unknown";
        }

        private async Task CheckUpdates()
        {
            try
            {
                lblUpdateStatus.Foreground = SystemColors.WindowTextBrush;
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
                    btnUpdates.Content = "Get Update";
                    IoC.Notif.ShowAppStatus($"Update available v{updates.LastVersion}");
                }
                else
                {
                    lblUpdateStatus.Text = "Running latest version";
                    btnUpdates.Content = "Check Update";
                    IoC.Notif.ShowAppStatus("");
                }

                lblLatestVersion.Text = updates.LastVersion?.ToString() ?? "Unknown";
            }
            catch (Exception ex)
            {
                lblUpdateStatus.Foreground = UIColors.ErrorBrush;
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

            lblUpdateStatus.Foreground = SystemColors.WindowTextBrush;
            lblUpdateStatus.Text = $"Downloading update...";
            IoC.Notif.ShowProgress(0);

            try
            {
                await Updater.PrepareUpdate(x => IoC.Notif.ShowProgress(x));

                lblUpdateStatus.Text = "Download complete, restart to update";
                btnUpdates.Content = "Restart";
                IoC.Notif.ShowAppStatus($"Update ready");
            }
            catch (Exception ex)
            {
                lblUpdateStatus.Foreground = UIColors.ErrorBrush;
                lblUpdateStatus.Text = $"Error: {ex.Message}";
                Errors.WriteError(ex);
            }

            IoC.Notif.HideProgress();
        }
    }
}
