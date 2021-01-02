﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.Data;
using AloysAdjustments.Logic;
using AloysAdjustments.UI;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Modules
{
    public partial class OutfitsControl : ModuleBase
    {
        private bool _updatingLists;

        private OutfitsLogic Logic { get; }

        private OutfitFile[] DefaultMaps { get; set; }
        private OutfitFile[] NewMaps { get; set; }

        private List<Outfit> Outfits { get; set; }
        private List<Model> Models { get; set; }
        
        public override string ModuleName => "Outfits";

        public OutfitsControl()
        {
            Logic = new OutfitsLogic();

            InitializeComponent();

            SetupLists();
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

        public override async Task LoadPatch(string path)
        {
            await Initialize();

            IoC.SetStatus("Loading outfits...");
            NewMaps = await Logic.GenerateOutfitFilesFromPath(path, false);

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
        }

        public override async Task CreatePatch(string patchDir)
        {
            var updatedMaps = new List<OutfitFile>();
            foreach (var map in NewMaps)
            {
                var defaultMap = DefaultMaps.First(x => x.File == map.File)
                    .Outfits.ToDictionary(x=>x.RefId, x=>x.ModelId);

                foreach (var outfit in map.Outfits)
                {
                    if (!defaultMap[outfit.RefId].Equals(outfit.ModelId))
                    {
                        updatedMaps.Add(map);
                        break;
                    }
                }
            }

            await Logic.CreatePatch(patchDir, updatedMaps);
        }

        public override async Task Initialize()
        {
            ResetSelected.Enabled = false;

            IoC.SetStatus("Generating outfit maps...");
            DefaultMaps = await Logic.GenerateOutfitFiles();
            NewMaps = DefaultMaps.Select(x => x.Clone()).ToArray();

            IoC.SetStatus("Loading outfit list...");
            var outfits = Logic.GenerateOutfitList(NewMaps);
            await UpdateOutfitDisplayNames(outfits);
            Outfits = outfits.OrderBy(x => x.DisplayName).ToList();

            lbOutfits.Items.Clear();
            foreach (var item in Outfits)
                lbOutfits.Items.Add(item);

            IoC.SetStatus("Loading models list...");
            var models = await Logic.GenerateModelList();
            //sort models to match outfits
            var outfitSorting = Outfits.Select((x, i) => (x, i)).ToDictionary(x => x.x.ModelId, x => x.i);
            Models = models.OrderBy(x => outfitSorting.TryGetValue(x.Id, out var sort) ? sort : int.MaxValue).ToList();
            UpdateModelDisplayNames(Outfits, Models);

            clbModels.Items.Clear();
            foreach (var item in Models)
                clbModels.Items.Add(item);
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

        private void lbOutfits_SelectedValueChanged(object sender, EventArgs e)
        {
            _updatingLists = true;

            var lb = (ListBox)sender;
            ResetSelected.Enabled = lb.SelectedIndex >= 0;

            var selected = lb.SelectedItems.Cast<Outfit>().ToHashSet();
            
            var modelIds = NewMaps.SelectMany(x=>x.Outfits)
                .Where(x=>selected.Contains(x)).Select(x => x.ModelId)
                .ToHashSet();

            for (int i = 0; i < clbModels.Items.Count; i++)
            {
                if (modelIds.Contains(Models[i].Id))
                    clbModels.SetItemCheckState(i, modelIds.Count > 1 ? CheckState.Indeterminate : CheckState.Checked);
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
            //get all matching outfits from the default mapping
            var origOutfits = DefaultMaps.SelectMany(x => x.Outfits)
                .Where(x => x.Equals(outfit)).ToHashSet();
            
            //find the outfit in the new mapping by reference and update the model
            foreach (var newOutfit in NewMaps.SelectMany(x => x.Outfits)
                .Where(x => origOutfits.Contains(x)))
            {
                origOutfits.TryGetValue(newOutfit, out var orig);

                newOutfit.Modified = !orig.ModelId.Equals(model.Id);
                newOutfit.ModelId.AssignFromOther(model.Id);

                if (newOutfit.Equals(outfit))
                {
                    outfit.ModelId.AssignFromOther(model.Id);
                    outfit.Modified = newOutfit.Modified;
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

        protected override async void Reset_Click()
        {
            using var _ = new ControlLock(Reset);

            await Initialize();
            RefreshLists();

            IoC.SetStatus("Reset complete");
        }

        protected override void ResetSelected_Click()
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
    }
}
