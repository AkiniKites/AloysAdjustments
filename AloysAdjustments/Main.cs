using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.Data;
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

        private readonly Color _errorColor = Color.FromArgb(255, 51, 51);
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
        }

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            SetStatus($"Error: {e.Exception.Message}", true);
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
            await LoadConfig();

            IoC.Bind(new Decima());
            IoC.Bind(new Localization(ELanguage.English));
            IoC.SetStatus = x => SetStatus(x, false);
            IoC.SetError = x => SetStatus(x, true);

            tbGameDir.EnableTypingEvent = false;
            tbGameDir.Text = IoC.Config.Settings.GamePath;
            tbGameDir.EnableTypingEvent = true;

            Modules = new List<IModule>()
            {
                new OutfitsControl(btnReset, btnResetSelected)
            };

            foreach (var module in Modules.AsEnumerable().Reverse())
            {
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
            var settingsValid = UpdateGameDirStatus() && UpdateDecimaStatus();
            if (!settingsValid)
                return false;

            foreach (var module in Modules)
            {
                await module.Initialize();
            }

            return true;
        }

        private async void btnPatch_Click(object sender, EventArgs e)
        {
            using var _ = new ControlLock(btnPatch);

            if (File.Exists(Path.Combine(Configs.GamePackDir, IoC.Config.PatchFile)))
                File.Delete(Path.Combine(Configs.GamePackDir, IoC.Config.PatchFile));

            SetStatus("Generating patch...");
            var patcher = new Patcher();

            var dir = await patcher.SetupPatchDir();
            foreach (var module in Modules)
                await module.CreatePatch(dir);
            var patch = await patcher.PackPatch(dir);

            SetStatus("Copying patch...");
            await new Patcher().InstallPatch(patch);

            //await FileManager.Cleanup(IoC.Config.TempPath);

            SetStatus("Patch installed");
        }

        private async void btnDecima_Click(object sender, EventArgs e)
        {
            using var _ = new ControlLock(btnDecima);

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
                await module.Load(ofd.FileName);

            IoC.Config.Settings.LastOpen = ofd.FileName;
            await SaveConfig();

            SetStatus($"Loaded pack: {Path.GetFileName(ofd.FileName)}");
        }

        private void tbGameDir_TextChanged(object sender, EventArgs e)
        {
            IoC.Config.Settings.GamePath = tbGameDir.Text;
        }
        private async void tbGameDir_TypingFinished(object sender, EventArgs e)
        {
            await SaveConfig();
            if (UpdateGameDirStatus() && _initialized)
                await Initialize();
        }

        private bool UpdateGameDirStatus()
        {
            var valid = Directory.Exists(Configs.GamePackDir);
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

            var validLib = IoC.Decima.CheckDecimaExe();
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

                if (UpdateGameDirStatus() && _initialized)
                    await Initialize();
            }
        }
        
        public async Task LoadConfig()
        {
            var json = await File.ReadAllTextAsync(ConfigPath);
            IoC.Bind<Config>(await Task.Run(() => JsonConvert.DeserializeObject<Config>(json)));
        }

        public async Task SaveConfig()
        {
            var json = JsonConvert.SerializeObject(IoC.Config, Formatting.Indented);
            await File.WriteAllTextAsync(ConfigPath, json);
        }

        private void tcMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcMain.SelectedIndex >= 0 && tcMain.SelectedIndex < Modules.Count)
            {
                for (int i = 0; i < Modules.Count; i++)
                {
                    if (i == tcMain.SelectedIndex)
                        Modules[i].Activate();
                    else
                        Modules[i].DeActivate();
                }
            }
        }
    }
}
