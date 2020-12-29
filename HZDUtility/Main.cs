using Decima;
using HZDUtility.Utility;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Decima.HZD;
using HZDUtility.Models;
using Application = System.Windows.Forms.Application;
using Model = HZDUtility.Models.Model;

namespace HZDUtility
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

            SetupOutfitList();
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

        private void SetupOutfitList()
        {
            lbOutfits.DisplayMember = "DisplayName";

            lbOutfits.DrawMode = DrawMode.OwnerDrawVariable;
            lbOutfits.ItemHeight = lbOutfits.Font.Height + 2;
            lbOutfits.DrawItem += (s, e) =>
            {
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
            SetStatus("Loading config");
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
            
            SetStatus("Generating outfit maps");
            DefaultMaps = await Logic.GenerateOutfitFiles();
            NewMaps = DefaultMaps.Select(x => x.Clone()).ToArray();

            SetStatus("Loading outfit list");
            var outfits = Logic.GenerateOutfitList(DefaultMaps);
            await UpdateDisplayNames(outfits);
            Outfits = outfits.OrderBy(x => x.DisplayName).ToList();

            lbOutfits.Items.Clear();
            foreach (var item in Outfits)
                lbOutfits.Items.Add(item);

            SetStatus("Loading models list");
            Models = (await Logic.GenerateModelList(DefaultMaps))
                .OrderBy(x => x.Name).ToList(); ;
            clbModels.Items.Clear();
            foreach (var item in Models)
                clbModels.Items.Add(item);

            _invalidConfig = false;
            SetStatus("Loading complete");
        }

        private void UpdateMapping(Outfit outfit, Model model)
        {
            //get all references with same outfit id from the default mapping
            var mapRefs = DefaultMaps.SelectMany(x => x.Outfits)
                .Where(x => x.Id.Equals(outfit.Id)).Select(x=>x.RefId)
                .ToHashSet();

            outfit.Modified = !outfit.Id.Equals(model.Id);

            //find the outfit in the new mapping by reference and update the model
            foreach (var map in NewMaps)
            {
                foreach (var reference in map.Outfits.Where(x=> mapRefs.Contains(x.RefId)))
                {
                    reference.Id.AssignFromOther(model.Id);
                }
            }
        }

        private void lbOutfits_SelectedValueChanged(object sender, EventArgs e)
        {
            _updatingLists = true;

            var lb = (ListBox)sender;

            var modelIds = lb.SelectedItems.Cast<Outfit>()
                .Select(x => x.Id).ToHashSet();

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
            btnPatch.Enabled = false;

            SetStatus("Generating patch...");
            var patch = await Logic.GeneratePatch(NewMaps);

            SetStatus("Copying patch...");
            await Logic.InstallPatch(patch);

            //await FileManager.Cleanup(Logic.Config.TempPath);

            SetStatus("Patch installed");

            btnPatch.Enabled = true;
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

        private async void btnDecima_Click(object sender, EventArgs e)
        {
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
            using (var ofd = new OpenFileDialog())
            {
                ofd.CheckFileExists = true;
                ofd.Multiselect = false;
                ofd.Filter = "Pack files (*.bin)|*.bin|All files (*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    SetStatus("Loading pack...");
                    NewMaps = await Logic.GenerateOutfitFilesFromPath(ofd.FileName);

                    //should be 1-1 relationship for the default outfits
                    var defaultRefs = new Dictionary<BaseGGUUID, BaseGGUUID>();
                    foreach (var sRef in DefaultMaps.SelectMany(x => x.Outfits))
                        defaultRefs[sRef.Id] = sRef.RefId;

                    var newRefs = NewMaps.SelectMany(x => x.Outfits)
                        .ToDictionary(x => x.RefId, x => x.Id);

                    foreach (var outfit in Outfits)
                    {
                        if (defaultRefs.TryGetValue(outfit.Id, out var defaultRefId))
                        {
                            outfit.Modified = newRefs.TryGetValue(defaultRefId, out var newModelId) && 
                                !outfit.Id.Equals(newModelId);
                        }
                    }

                    RefreshLists();

                    IoC.Config.Settings.LastOpen = ofd.FileName;
                    await SaveConfig();

                    SetStatus($"Loaded pack: {Path.GetFileName(ofd.FileName)}");
                }
            }
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
            btnReset.Enabled = false;

            await Initialize();
            RefreshLists();

            SetStatus("Reset complete");

            btnReset.Enabled = true;
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

        public async Task UpdateDisplayNames(List<Outfit> outfits)
        {
            foreach (var o in outfits)
            {
                o.DisplayName = await IoC.Localization.GetString(o.LocalNameFile, o.LocalNameId);
            }
        }
    }
}
