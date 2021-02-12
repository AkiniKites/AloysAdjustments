using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.Outfits.Data;
using AloysAdjustments.UI;
using AloysAdjustments.Utility;
using Decima;

namespace AloysAdjustments.Plugins.Outfits
{
    public partial class OutfitsControl : InteractivePluginBase, INotifyPropertyChanged
    {
        private bool _updatingLists;
        private bool _loading;
        
        private CharacterGenerator CharacterGen { get; }
        private OutfitPatcher Patcher { get; }
        
        public ReadOnlyCollection<ValuePair<Model>> Models { get; set; }

        public ReadOnlyCollection<OutfitModelFilter> Filters { get; set; }

        public override string PluginName => "Outfits";

        public OutfitsControl()
        {
            _loading = true;

            IoC.Bind(Configs.LoadModuleConfig<OutfitConfig>(PluginName));
            
            CharacterGen = new CharacterGenerator();
            Patcher = new OutfitPatcher();

            InitializeComponent();

            Filters = OutfitModelFilter.All.ToList().AsReadOnly();

            LoadSettings();

            _loading = false;
        }

        private void LoadSettings()
        {
            cbAllOutfits.IsChecked = IoC.Settings.ApplyToAllOutfits;
            var filter = IoC.Settings.OutfitModelFilter;
            if (filter == 0)
                filter = OutfitModelFilter.Armor.Value | OutfitModelFilter.Characters.Value;

            foreach (var f in Filters.Where(x => x.IsFlag(filter)))
                ccbModelFilter.SelectedItems.Add(f);
        }

        public override async Task Initialize()
        {
            ResetSelected.Enabled = false;
            IoC.Notif.ShowUnknownProgress();
            
            await UpdateModelList();

            //start loading characters in background
            Async.Run(() => CharacterGen.GetCharacterModels(true)).Forget();
        }

        private async Task UpdateModelList()
        {
            var filter = IoC.Settings.OutfitModelFilter;
            if (filter == 0)
                filter = OutfitModelFilter.Armor.Value | OutfitModelFilter.Characters.Value;

            await Async.Run(() =>
            {
                var models = new List<Model>();
                if (OutfitModelFilter.Characters.IsFlag(filter))
                    models.AddRange(LoadCharacterModelList(false));
                if (OutfitModelFilter.AllCharacters.IsFlag(filter))
                    models.AddRange(LoadCharacterModelList(true));

                Models = models.Select(x => new ValuePair<Model>(x, x))
                    .ToList().AsReadOnly();
            });

            UpdateModelDisplayNames(Models);
        }

        private IEnumerable<Model> LoadCharacterModelList(bool all)
        {
            IoC.Notif.ShowStatus("Loading characters list...");
            var models = CharacterGen.GetCharacterModels(all);
            return models.OrderBy(x => x.ToString().Contains("DLC") ? 1 : 0).ThenBy(x => x.ToString());
        }

        public void UpdateModelDisplayNames(IList<ValuePair<Model>> models)
        {
            foreach (var m in models)
            {
                m.Value.DisplayName = m.Value.ToString();
                m.Default.DisplayName = m.Default.ToString();
            }
        }

        public override async Task LoadPatch(string path)
        {
            //IoC.Notif.ShowStatus("Loading outfits...");

            //var patchOutfits = await OutfitGen.GenerateOutfits(f =>
            //    IoC.Archiver.LoadFileAsync(path, f));
            //if (!patchOutfits.Any())
            //    return;

            //await Initialize();

            //var loadedOutfits = patchOutfits.ToHashSet();
            //var variantMapping = await CharacterGen.GetVariantMapping(path, OutfitGen);
            //LoadOutfits(loadedOutfits, variantMapping);

            //RefreshOutfitList();
        }
        
        public override void ApplyChanges(Patch patch)
        {
            var models = CharacterGen.GetCharacterModels(true);

            //Patcher.CreatePatch(patch, DefaultOutfits.ToList(), Outfits, models);
        }

        private void lbOutfits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _updatingLists = true;

            ResetSelected.Enabled = lbOutfits.SelectedIndex >= 0;

            var selectedModelIds = GetSelectedOutfits().Select(x => x.ModelId).ToHashSet();

            foreach (var model in Models)
            {
                if (selectedModelIds.Contains(model.Value.Id))
                    model.Value.Checked = selectedModelIds.Count > 1 ? null : (bool?)true;
                else
                    model.Value.Checked = false;
            }

            _updatingLists = false;
        }

        private void clbModels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (e.AddedItems.Count > 0 ? e.AddedItems[0] : null) as Model;

            if (_updatingLists || selected == null)
                return;
            _updatingLists = true;
            
            selected.Checked = true;
            foreach (var model in Models)
            {
                if (!ReferenceEquals(selected, model))
                    model.Value.Checked = false;
            }

            foreach (var outfit in GetSelectedOutfits())
                UpdateMapping(outfit, selected);

            _updatingLists = false;
        }

        private void UpdateMapping(Outfit outfit, Model model)
        {
            //DefaultOutfits.TryGetValue(outfit, out var defaultOutfit);

            //outfit.Modified = !defaultOutfit.ModelId.Equals(model.Id);
            outfit.ModelId.AssignFromOther(model.Id);
        }

        protected override async Task Reset_Click()
        {
            using var _ = new ControlLock(Reset);

            await Initialize();
            RefreshOutfitList();

            IoC.Notif.ShowStatus("Reset complete");
            IoC.Notif.HideProgress();
        }

        protected override Task ResetSelected_Click()
        {
            if (lbOutfits.SelectedIndex < 0)
                return Task.CompletedTask;

            var selected = GetSelectedOutfits();

            foreach (var outfit in selected)
            {
                //if (DefaultOutfits.TryGetValue(outfit, out var defaultOutfit))
                //{
                //    outfit.Modified = false;
                //    outfit.ModelId.AssignFromOther(defaultOutfit.ModelId);
                //}
            }

            lbOutfits_SelectionChanged(lbOutfits, null);

            return Task.CompletedTask;
        }

        private void RefreshOutfitList()
        {
            lbOutfits.UnselectAll();
        }

        private List<Outfit> GetSelectedOutfits()
        {
            return lbOutfits.SelectedItems.Cast<Outfit>().ToList();
        }

        private void cbAllOutfits_Checked(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            IoC.Settings.ApplyToAllOutfits = cbAllOutfits.IsChecked == true;
        }

        private bool _disableFilterEvents = false;
        private void ccbModelFilter_ItemSelectionChanged(object sender, Xceed.Wpf.Toolkit.Primitives.ItemSelectionChangedEventArgs e)
        {
            if (_disableFilterEvents || _loading)
                return;
            _disableFilterEvents = true;
            
            var checkedItem = (OutfitModelFilter)e.Item;
            if (checkedItem == OutfitModelFilter.Characters)
                ccbModelFilter.SelectedItems.Remove(OutfitModelFilter.AllCharacters);
            if (checkedItem == OutfitModelFilter.AllCharacters)
                ccbModelFilter.SelectedItems.Remove(OutfitModelFilter.Characters);

            _disableFilterEvents = false;
        }

        private async void ccbModelFilter_ClosedCommand(object sender, RoutedEventArgs e)
            => await Relay.To(sender, e, ccbModelFilter_Closed);
        private async Task ccbModelFilter_Closed(object sender, RoutedEventArgs e)
        {
            var filter = ccbModelFilter.SelectedItems.Cast<OutfitModelFilter>().Sum(x => x.Value);
            if (filter == 0)
            {
                filter = OutfitModelFilter.Characters.Value | OutfitModelFilter.Armor.Value;
                foreach (var f in Filters.Where(x => x.IsFlag(filter)))
                    ccbModelFilter.SelectedItems.Add(f);
            }

            IoC.Settings.OutfitModelFilter = filter;

            await UpdateModelList();

            IoC.Notif.ShowStatus("");
            IoC.Notif.HideProgress();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
