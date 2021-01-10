using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.Configuration;
using AloysAdjustments.Data;
using AloysAdjustments.Logic;
using AloysAdjustments.Utility;
using Decima;

namespace AloysAdjustments.Modules.Outfits
{
    public partial class OutfitsControl : ModuleBase
    {
        private bool _updatingLists;
        private bool _loading;

        private OutfitsLogic OutfitLogic { get; }
        private CharacterLogic CharacterLogic { get; }

        private HashSet<Outfit> DefaultOutfits { get; set; }
        private ReadOnlyCollection<Outfit> Outfits { get; set; }
        private ReadOnlyCollection<Model> Models { get; set; }
        
        public override string ModuleName => "Outfits";

        public OutfitsControl()
        {
            _loading = true;

            IoC.Bind(Configs.LoadModuleConfig<OutfitConfig>(ModuleName));

            OutfitLogic = new OutfitsLogic();
            CharacterLogic = new CharacterLogic();

            InitializeComponent();
            
            UpdateMode();
            SetupLists();

            _loading = false;
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

        public override async Task Initialize()
        {
            ResetSelected.Enabled = false;
            IoC.Notif.ShowUnknownProgress();

            IoC.Notif.ShowStatus("Loading outfit list...");
            DefaultOutfits = (await OutfitLogic.GenerateOutfits()).ToHashSet();
            
            var outfits = DefaultOutfits.Select(x => x.Clone()).ToList();
            await UpdateOutfitDisplayNames(outfits);
            Outfits = outfits.OrderBy(x => x.DisplayName).ToList().AsReadOnly();

            lbOutfits.Items.Clear();
            foreach (var item in Outfits)
                lbOutfits.Items.Add(item);

            if (IoC.Settings.SwapCharacterMode)
                await LoadCharacterModelList();
            else
                await LoadOutfitModelList();

            UpdateModelDisplayNames(Outfits, Models);

            clbModels.Items.Clear();
            foreach (var item in Models)
                clbModels.Items.Add(item);
        }

        private async Task LoadCharacterModelList()
        {
            IoC.Notif.ShowStatus("Loading characters list...");
            var models = await CharacterLogic.Search.GetCharacterModels(IoC.Settings.ShowAllCharacters);
            Models = models.OrderBy(x => x.ToString())
                .Cast<Model>().ToList().AsReadOnly();
        }

        private async Task LoadOutfitModelList()
        {
            IoC.Notif.ShowStatus("Loading models list...");
            var models = await OutfitLogic.GenerateModelList();

            //sort models to match outfits
            var outfitSorting = Outfits.Select((x, i) => (x, i)).ToSoftDictionary(x => x.x.ModelId, x => x.i);
            Models = models.OrderBy(x => outfitSorting.TryGetValue(x.Id, out var sort) ? sort : int.MaxValue)
                .ToList().AsReadOnly();
        }

        public async Task UpdateOutfitDisplayNames(List<Outfit> outfits)
        {
            foreach (var o in outfits)
            {
                o.SetDisplayName(await IoC.Localization.GetString(o.LocalNameFile, o.LocalNameId));
            }
        }

        public void UpdateModelDisplayNames(IList<Outfit> outfits, IList<Model> models)
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

        public override async Task LoadPatch(string path)
        {
            IoC.Notif.ShowStatus("Loading outfits...");

            var characterMode = await CharacterLogic.IsCharacterModeFile(path);
            var patchOutfits = await OutfitLogic.GenerateOutfits(path, false);
            
            if (!patchOutfits.Any())
                return;

            var loadedOutfits = patchOutfits.ToHashSet();

            IoC.Settings.SwapCharacterMode = characterMode;
            UpdateMode();

            await Initialize();

            var variantMapping = new Dictionary<BaseGGUUID, BaseGGUUID>();
            if (IoC.Settings.SwapCharacterMode)
            {
                variantMapping = await CharacterLogic.GetVariantMapping(path, OutfitLogic);
            }

            foreach (var outfit in Outfits)
            {
                if (loadedOutfits.TryGetValue(outfit, out var loadedOutfit))
                {
                    if (!variantMapping.TryGetValue(loadedOutfit.ModelId, out var varId))
                        varId = loadedOutfit.ModelId;

                    outfit.Modified = !outfit.ModelId.Equals(varId);
                    outfit.ModelId.AssignFromOther(varId);
                }
            }

            RefreshLists();
        }

        public override async Task CreatePatch(string patchDir)
        {
            if (IoC.Settings.SwapCharacterMode)
            {
                await CharacterLogic.CreatePatch(patchDir, Outfits, 
                    Models.Cast<CharacterModel>(), OutfitLogic);
            }
            else
            {
                await OutfitLogic.CreatePatch(patchDir, Outfits);
            }
        }
        
        private void lbOutfits_SelectedValueChanged(object sender, EventArgs e)
        {
            _updatingLists = true;

            var lb = (ListBox)sender;
            ResetSelected.Enabled = lb.SelectedIndex >= 0;

            var modelIds = lb.SelectedItems.Cast<Outfit>()
                .Select(x => x.ModelId).ToHashSet();

            var checkState = modelIds.Count > 1 ? CheckState.Indeterminate : CheckState.Checked;

            for (int i = 0; i < clbModels.Items.Count; i++)
            {
                if (modelIds.Contains(Models[i].Id))
                    clbModels.SetItemCheckState(i, checkState);
                else
                    clbModels.SetItemCheckState(i, CheckState.Unchecked);
            }

            _updatingLists = false;
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
            DefaultOutfits.TryGetValue(outfit, out var defaultOutfit);

            outfit.Modified = !defaultOutfit.ModelId.Equals(model.Id);
            outfit.ModelId.AssignFromOther(model.Id);
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

        private async Task Reload()
        {
            await Initialize();
            RefreshLists();

            IoC.Notif.HideProgress();
        }

        protected override async Task Reset_Click()
        {
            using var _ = new ControlLock(Reset);

            await Reload();
            IoC.Notif.ShowStatus("Reset complete");
        }

        protected override Task ResetSelected_Click()
        {
            if (lbOutfits.SelectedIndex < 0)
                return Task.CompletedTask;
            
            var selected = lbOutfits.SelectedItems.Cast<Outfit>().ToList();

            foreach (var outfit in selected)
            {
                if (DefaultOutfits.TryGetValue(outfit, out var defaultOutfit))
                {
                    outfit.Modified = false;
                    outfit.ModelId.AssignFromOther(defaultOutfit.ModelId);
                }
            }

            lbOutfits.Invalidate();
            lbOutfits_SelectedValueChanged(lbOutfits, EventArgs.Empty);

            return Task.CompletedTask;
        }

        private void RefreshLists()
        {
            lbOutfits.ClearSelected();
            lbOutfits.Invalidate();
        }

        private async void cbShowAll_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            IoC.Settings.ShowAllCharacters = cbShowAll.Checked;

            UpdateMode();

            await Reload();
            IoC.Notif.ShowStatus($"Showing: {(IoC.Settings.ShowAllCharacters ? "All" : "Unique")} characters");
        }

        private async void btnMode_Click(object sender, EventArgs e)
        {
            if (_loading) return;
            IoC.Settings.SwapCharacterMode = !IoC.Settings.SwapCharacterMode;

            UpdateMode();

            await Reload();
            IoC.Notif.ShowStatus($"Mode changed to: {btnMode.Text}");
        }

        private void UpdateMode()
        {
            var charMode = IoC.Settings.SwapCharacterMode;

            btnMode.Text = charMode ? "Character Swaps" : "Armor Swaps";
            lblModels.Text = charMode ? "Character Mapping" : "Model Mapping";

            cbShowAll.Visible = charMode;
            cbShowAll.Checked = IoC.Settings.ShowAllCharacters;
        }
    }
}
