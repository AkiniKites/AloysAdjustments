using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Decima;
using Decima.HZD;
using AloysAdjustments.Models;
using AloysAdjustments.Utility;
using Newtonsoft.Json;
using Application = System.Windows.Forms.Application;
using Model = AloysAdjustments.Models.Model;

namespace AloysAdjustments
{
    public partial class Main : Form
    {
        private const string ConfigPath = "config.json";

        private readonly Color _errorColor = Color.FromArgb(255, 51, 51);

        private bool _updatingLists = false;
        private bool _invalidConfig = true;

        private Logic Logic { get; set; }

        private OutfitFile[] DefaultMaps { get; set; }
        private OutfitFile[] NewMaps { get; set; }

        private List<Outfit> Outfits { get; set; }
        private List<Model> Models { get; set; }

        public Main()
        {
            InitializeComponent();

            SetupLists();
            RTTI.SetGameMode(GameType.HZD);

            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            SetStatus($"Error: {e.ExceptionObject}", true);
        }

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            SetStatus($"Error: {e.Exception.Message}", true);
        }

        private void SetupLists()
        {
            clbModels.DisplayMember = "DisplayName";

            lbOutfits.DisplayMember = "DisplayName";
            lbOutfits.DrawMode = DrawMode.OwnerDrawVariable;
            lbOutfits.ItemHeight = lbOutfits.Font.Height + 2;
            lbOutfits.DrawItem += (s, e) =>
            {
                if (e.Index < 0)
                    return;

                var l = (ListBox)s;
                
                if (e.State.HasFlag(DrawItemState.Selected))
                {
                    e.DrawBackground();
                }
                else
                {
                    var backColor = Outfits?.Any() == true && Outfits[e.Index].Modified ? Color.LightSkyBlue : e.BackColor;

                    using (var b = new SolidBrush(backColor))
                        e.Graphics.FillRectangle(b, e.Bounds);
                }
                
                using (var b = new SolidBrush(e.ForeColor))
                {
                    var text = l.GetItemText(l.Items[e.Index]);
                    e.Graphics.DrawString(text, e.Font, b, e.Bounds, StringFormat.GenericDefault);
                }
                e.DrawFocusRectangle();
            };
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
            Logic = new Logic();
            IoC.Bind(new Decima());
            IoC.Bind(new Localization(ELanguage.English));

            tbGameDir.EnableTypingEvent = false;
            tbGameDir.Text = IoC.Config.Settings.GamePath;
            tbGameDir.EnableTypingEvent = true;

            await Initialize();
        }

        private async Task Initialize()
        {
            var game = UpdateGameDirStatus();
            var decima = UpdateDecimaStatus();

            if (!game)
            {
                SetStatus("Missing Game Folder", true);
                return;
            }
            if (!decima)
            {
                SetStatus("Missing Decima", true);
                return;
            }

            btnResetSelected.Enabled = false;

            SetStatus("Generating outfit maps...");
            DefaultMaps = await Logic.GenerateOutfitFiles();
            NewMaps = DefaultMaps.Select(x => x.Clone()).ToArray();

            SetStatus("Loading outfit list...");
            var outfits = Logic.GenerateOutfitList(NewMaps);
            await UpdateOutfitDisplayNames(outfits);
            Outfits = outfits.OrderBy(x => x.DisplayName).ToList();

            lbOutfits.Items.Clear();
            foreach (var item in Outfits)
                lbOutfits.Items.Add(item);

            SetStatus("Loading models list...");
            var models = await Logic.GenerateModelList();
            //sort models to match outfits
            var outfitSorting = Outfits.Select((x, i) => (x, i)).ToDictionary(x => x.x.ModelId, x => x.i);
            Models = models.OrderBy(x => outfitSorting.TryGetValue(x.Id, out var sort) ? sort : int.MaxValue).ToList();
            UpdateModelDisplayNames(Outfits, Models);

            clbModels.Items.Clear();
            foreach (var item in Models)
                clbModels.Items.Add(item);

            _invalidConfig = false;
            SetStatus("Loading complete");
        }
        
        private void lbOutfits_SelectedValueChanged(object sender, EventArgs e)
        {
            _updatingLists = true;

            var lb = (ListBox)sender;
            btnResetSelected.Enabled = lb.SelectedIndex >= 0;

            var modelIds = lb.SelectedItems.Cast<Outfit>()
                .Select(x => x.ModelId).ToHashSet();

            for (int i = 0; i < clbModels.Items.Count; i++)
            {
                if (modelIds.Contains(Models[i].Id))
                    clbModels.SetItemCheckState(i, modelIds.Count > 1 ? CheckState.Indeterminate : CheckState.Checked);
                else
                    clbModels.SetItemCheckState(i, CheckState.Unchecked);
            }

            _updatingLists = false;
        }

        private async void btnPatch_Click(object sender, EventArgs e)
        {
            using var _ = new ControlLock(btnPatch);

            if (File.Exists(Path.Combine(Configs.GamePackDir, IoC.Config.PatchFile)))
                File.Delete(Path.Combine(Configs.GamePackDir, IoC.Config.PatchFile));

            SetStatus("Generating patch...");
            var patch = await new Patcher().GeneratePatch(NewMaps);

            SetStatus("Copying patch...");
            await new Patcher().InstallPatch(patch);

            //await FileManager.Cleanup(IoC.Config.TempPath);

            SetStatus("Patch installed");
        }

        private void clbModels_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_updatingLists)
                return;
            _updatingLists = true;

            if (e.CurrentValue == CheckState.Indeterminate)
                e.NewValue = CheckState.Checked;

            if (e.NewValue == CheckState.Checked)
            {
                for (int i = 0; i < clbModels.Items.Count; i++)
                {
                    if (i != e.Index)
                        clbModels.SetItemCheckState(i, CheckState.Unchecked);
                }

                var model = Models[e.Index];

                foreach (var outfit in lbOutfits.SelectedItems.Cast<Outfit>())
                    UpdateMapping(outfit, model);
                
                lbOutfits.Invalidate();
            }

            _updatingLists = false;
        }

        private void UpdateMapping(Outfit outfit, Model model)
        {
            //get all matching outfits from the default mapping
            var origOutfits = DefaultMaps.SelectMany(x => x.Outfits)
                .Where(x => x.Equals(outfit)).ToHashSet();

            //find the outfit in the new mapping by reference and update the model
            foreach (var newOutfit in NewMaps.SelectMany(x => x.Outfits)
                .Where(x => origOutfits.Contains(x)))
            {
                newOutfit.Modified = !newOutfit.ModelId.Equals(model.Id);
                newOutfit.ModelId.AssignFromOther(model.Id);
            }
        }

        private async void btnDecima_Click(object sender, EventArgs e)
        {
            using var _ = new ControlLock(btnDecima);

            SetStatus("Downloading Decima...");
            await IoC.Decima.Download();
            SetStatus("Copying Decima library...");
            await IoC.Decima.GetLibrary();
            SetStatus("Decima updated");
            
            if (UpdateDecimaStatus() && _invalidConfig)
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

            await Initialize();

            SetStatus("Loading pack...");
            NewMaps = await Logic.GenerateOutfitFilesFromPath(ofd.FileName);

            var newOutfits = NewMaps.SelectMany(x => x.Outfits).ToHashSet();

            foreach (var outfit in Outfits)
            {
                if (newOutfits.TryGetValue(outfit, out var newOutfit))
                {
                    outfit.Modified = !outfit.ModelId.Equals(newOutfit.ModelId);
                    outfit.ModelId.AssignFromOther(newOutfit.ModelId);
                }
            }

            RefreshLists();

            IoC.Config.Settings.LastOpen = ofd.FileName;
            await SaveConfig();

            SetStatus($"Loaded pack: {Path.GetFileName(ofd.FileName)}");
        }

        private void lbOutfits_KeyDown(object sender, KeyEventArgs e)
        {
            var lb = (ListBox)sender;

            if (e.KeyCode == Keys.A && e.Control)
            {
                for (int i = 0; i < lb.Items.Count; i++)
                    lb.SetSelected(i, true);
                e.SuppressKeyPress = true;
            }
        }

        private async void btnReset_Click(object sender, EventArgs e)
        {
            using var _ = new ControlLock(btnReset);
            
            await Initialize();
            RefreshLists();

            SetStatus("Reset complete");
        }
        
        private void btnResetSelected_Click(object sender, EventArgs e)
        {
            if (lbOutfits.SelectedIndex < 0)
                return;

            var defaultOutfits = DefaultMaps.SelectMany(x => x.Outfits).ToHashSet();
            var selected = lbOutfits.SelectedItems.Cast<Outfit>().ToList();

            foreach (var outfit in selected)
            {
                if (defaultOutfits.TryGetValue(outfit, out var defaultOutfit))
                {
                    outfit.Modified = false;
                    outfit.ModelId.AssignFromOther(defaultOutfit.ModelId);
                }
            }

            lbOutfits.Invalidate();
            lbOutfits_SelectedValueChanged(lbOutfits, EventArgs.Empty);
        }

        private void RefreshLists()
        {
            lbOutfits.ClearSelected();
            lbOutfits.Invalidate();
        }

        private void tbGameDir_TextChanged(object sender, EventArgs e)
        {
            IoC.Config.Settings.GamePath = tbGameDir.Text;
        }
        private async void tbGameDir_TypingFinished(object sender, EventArgs e)
        {
            await SaveConfig();
            if (UpdateGameDirStatus() && _invalidConfig)
                await Initialize();
        }

        private bool UpdateGameDirStatus()
        {
            if (Logic.CheckGameDir())
            {
                lblGameDir.Text = "Game Folder";
                lblGameDir.ForeColor = SystemColors.ControlText;
                return true;
            }
            else
            {
                lblGameDir.Text = "Game Folder - Invalid";
                lblGameDir.ForeColor = _errorColor;
                return false;
            }
        }
        private bool UpdateDecimaStatus()
        {
            if (IoC.Decima.CheckDecima())
            {
                lblDecima.Text = "Decima";
                lblDecima.ForeColor = SystemColors.ControlText;
                return true;
            }
            else
            {
                lblDecima.Text = "Decima - Invalid";
                lblDecima.ForeColor = _errorColor;
                return false;
            }
        }

        private async void btnGameDir_Click(object sender, EventArgs e)
        {
            using var ofd = new FolderBrowserDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tbGameDir.EnableTypingEvent = false;
                tbGameDir.Text = ofd.SelectedPath;
                tbGameDir.EnableTypingEvent = true;

                if (UpdateGameDirStatus() && _invalidConfig)
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

        public async Task UpdateOutfitDisplayNames(List<Outfit> outfits)
        {
            foreach (var o in outfits)
            {
                o.SetDisplayName(await IoC.Localization.GetString(o.LocalNameFile, o.LocalNameId));
            }
        }

        public void UpdateModelDisplayNames(List<Outfit> outfits, List<Model> models)
        {
            var names = outfits.ToSoftDictionary(x => x.ModelId, x => x.DisplayName);

            foreach (var m in models)
            {
                if (names.TryGetValue(m.Id, out var outfitName))
                    m.DisplayName = $"{outfitName} ({m})";
                else
                    m.DisplayName = m.ToString();
            }
        }
    }
}
