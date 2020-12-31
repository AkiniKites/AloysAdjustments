using System;
using System.Collections.Generic;
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
        private const string SettingsPath = "settings.json";

        private readonly Color _errorColor = Color.FromArgb(204, 0, 0);
        private readonly Color _okColor = Color.ForestGreen;
        
        private bool _initialized = true;

        private List<IModule> Modules { get; set; }
        
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

        private async void Main_Load(object sender, EventArgs e)
        {
            SetStatus("Loading config...");
            await LoadConfigs();

            IoC.Bind(new Logic.Decima());
            IoC.Bind(new Localization(ELanguage.English));
            IoC.SetStatus = x => SetStatus(x, false);
            IoC.SetError = x => SetStatus(x, true);

            tbGameDir.EnableTypingEvent = false;
            tbGameDir.Text = IoC.Settings.GamePath;
            tbGameDir.EnableTypingEvent = true;

            Modules = new List<IModule>()
            {
                new OutfitsControl(),
                new UpgradesControl()
            };

            foreach (var module in Modules.AsEnumerable().Reverse())
            {
                module.Reset = btnReset;
                module.ResetSelected = btnResetSelected;

                var tab = new TabPage();

                tab.Text = module.ModuleName;
                tab.Controls.Add(module.ModuleControl);
                module.ModuleControl.Dock = DockStyle.Fill;

                tcMain.TabPages.Insert(0, tab);
            }

            tcMain.SelectedIndex = 0;
            if (!await Initialize())
                tcMain.SelectedIndex = tcMain.TabPages.Count - 1;
        }

        private async Task<bool> Initialize()
        {
            var settingsValid = UpdateGameDirStatus();
            settingsValid = UpdateDecimaStatus() && settingsValid;

            if (!settingsValid)
                return false;

            foreach (var module in Modules)
            {
                await module.Initialize();
            }

            SetStatus("Loading complete");

            return true;
        }

        private async void btnPatch_Click(object sender, EventArgs e)
        {
            using var _ = new ControlLock(btnPatch);
            using var oldPatch = new FileRenamer(Configs.PatchPath);

            SetStatus("Generating patch...");
            var patcher = new Patcher();

            var dir = await patcher.SetupPatchDir();
            foreach (var module in Modules)
                await module.CreatePatch(dir);
            var patch = await patcher.PackPatch(dir);

            SetStatus("Copying patch...");
            await patcher.InstallPatch(patch);

            oldPatch.Delete();

            await FileManager.Cleanup(IoC.Config.TempPath);
            
            SetStatus("Patch installed");
        }

        private async void btnDecima_Click(object sender, EventArgs e)
        {
            using var _ = new ControlLock(btnDecima);

            if (!UpdateGameDirStatus())
                return;

            SetStatus("Downloading Decima...");
            await IoC.Decima.Download();
            SetStatus("Copying Decima library...");
            await IoC.Decima.GetLibrary();
            SetStatus("Decima updated");
            
            if (UpdateDecimaStatus() && _initialized)
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

            foreach (var module in Modules)
                await module.LoadPatch(ofd.FileName);

            IoC.Settings.LastOpen = ofd.FileName;
            await SaveSettings();

            SetStatus($"Loaded pack: {Path.GetFileName(ofd.FileName)}");
        }

        private void tbGameDir_TextChanged(object sender, EventArgs e)
        {
            IoC.Settings.GamePath = tbGameDir.Text;
        }
        private async void tbGameDir_TypingFinished(object sender, EventArgs e)
        {
            await SaveSettings();
            if (UpdateGameDirStatus() && _initialized)
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
        private bool UpdateDecimaStatus()
        {
            var validExe = IoC.Decima.CheckDecimaExe();
            lblDecimaExe.Text = validExe ? "OK" : "Missing";
            lblDecimaExe.ForeColor = validExe ? _okColor : _errorColor;

            var validLib = IoC.Decima.CheckDecimaLib();
            lblDecimaLib.Text = validLib ? "OK" : "Missing";
            lblDecimaLib.ForeColor = validLib ? _okColor : _errorColor;

            if (!validExe || !validLib)
                SetStatus("Missing Decima Extractor", true);

            return validExe && validLib;
        }

        private async void btnGameDir_Click(object sender, EventArgs e)
        {
            using var ofd = new FolderBrowserDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tbGameDir.EnableTypingEvent = false;
                tbGameDir.Text = ofd.SelectedPath;
                tbGameDir.EnableTypingEvent = true;

                await SaveSettings();
                if (UpdateGameDirStatus() && _initialized)
                    await Initialize();
            }
        }
        
        public async Task LoadConfigs()
        {
            var json = await File.ReadAllTextAsync(ConfigPath);
            IoC.Bind(await Task.Run(() => JsonConvert.DeserializeObject<Config>(json)));

            if (File.Exists(SettingsPath))
            {
                json = await File.ReadAllTextAsync(SettingsPath);
                IoC.Bind(await Task.Run(() => JsonConvert.DeserializeObject<Settings>(json)));
            }
            else
            {
                IoC.Bind(new Settings());
            }
        }

        public async Task SaveSettings()
        {
            var json = JsonConvert.SerializeObject(IoC.Settings, Formatting.Indented);
            await File.WriteAllTextAsync(SettingsPath, json);
        }

        private void tcMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcMain.SelectedIndex >= 0 && tcMain.SelectedIndex < Modules.Count)
            {
                btnReset.Enabled = true;

                for (int i = 0; i < Modules.Count; i++)
                {
                    if (i == tcMain.SelectedIndex)
                        Modules[i].Activate();
                    else
                        Modules[i].DeActivate();
                }
            }
            else
            {
                btnReset.Enabled = false;
                btnResetSelected.Enabled = false;
            }
        }
    }
}
