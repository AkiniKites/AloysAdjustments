using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.Common;
using AloysAdjustments.Plugins.Common.Data;
using AloysAdjustments.Plugins.NPC.Characters;
using AloysAdjustments.UI;
using AloysAdjustments.Utility;
using EnumsNET;

namespace AloysAdjustments.Plugins.NPC
{
    [Flags]
    public enum ModelFilter
    {
        [Description("Main Characters")]
        MainCharacters = 1,
        [Description("All Characters")]
        AllCharacters = 2
    }

    public class NpcPlugin : InteractivePlugin, INotifyPropertyChanged
    {
        public override string PluginName => "NPC Models";

        private CharacterGenerator CharacterGen { get; }
        private NpcPatcher Patcher { get; }

        public ValuePair<Model> AllNpcStub { get; set; }
        public ObservableCollection<ValuePair<Model>> Npcs { get; set; }
        public ICollectionView NpcsView { get; set; }
        public ObservableCollection<Model> Models { get; set; }
        public ICollectionView ModelsView { get; set; }
        
        public IList SelectedNpcModels { get; set; }
        public Model SelectedModelMapping { get; set; }

        private ModelFilter _filterValue;
        public ModelFilter FilterValue
        {
            get => _filterValue;
            set
            {
                if (_filterValue == value)
                    return;

                var newVal = value;
                var oldVal = _filterValue;

                if (newVal.HasFlag(ModelFilter.MainCharacters) && oldVal.HasFlag(ModelFilter.AllCharacters))
                    newVal = newVal.RemoveFlags(ModelFilter.AllCharacters);
                if (newVal.HasFlag(ModelFilter.AllCharacters) && oldVal.HasFlag(ModelFilter.MainCharacters))
                    newVal = newVal.RemoveFlags(ModelFilter.MainCharacters);
                _filterValue = newVal;

                OnPropertyChanged(nameof(FilterValue), oldVal, newVal);
            }
        }

        public bool ApplyToAll { get; set; }

        public NpcPlugin()
        {
            IoC.Bind(Configs.LoadModuleConfig<CharacterConfig>(PluginName));

            Reset = new ControlRelay(OnResetAll);
            ResetSelected = new ControlRelay(OnResetSelected);

            CharacterGen = new CharacterGenerator();
            Patcher = new NpcPatcher();

            PluginControl = new NpcPluginView();
            PluginControl.DataContext = this;

            Models = new ObservableCollection<Model>();
            ModelsView = CollectionViewSource.GetDefaultView(Models);
            ModelsView.Filter = Filter;
            ModelsView.SortDescriptions.Add(new SortDescription(nameof(Model.DisplayName), ListSortDirection.Ascending));

            Npcs = new ObservableCollection<ValuePair<Model>>();
            NpcsView = CollectionViewSource.GetDefaultView(Npcs);
            NpcsView.Filter = NpcFilter;
            NpcsView.SortDescriptions.Add(new SortDescription("Default.DisplayName", ListSortDirection.Ascending));

            var allNpc = new Model() {DisplayName = "All Outfits"};
            AllNpcStub = new ValuePair<Model>(allNpc, allNpc);
        }

        private void LoadSettings()
        {
            ApplyToAll = IoC.Settings.ApplyToAllNpcs;
            FilterValue = (ModelFilter)IoC.Settings.NpcModelFilter;
        }

        public override async Task LoadPatch(string path)
        {
            IoC.Notif.ShowStatus("Loading npcs...");

            var map = await Patcher.LoadMap(path);
            if (!map.Any())
                return;

            await Initialize();

            using (var defer = NpcsView.DeferRefresh())
            {
                var models = Models.ToDictionary(x => x.Id, x => x);
                foreach (var npc in Npcs.Where(x => x != AllNpcStub))
                {
                    if (map.TryGetValue(npc.Default.Id, out var newId))
                    {
                        npc.Value = models[newId];
                    }
                }
            }

            OnApplyToAll();
        }

        public override void ApplyChanges(Patch patch)
        {
            Patcher.CreatePatch(patch, Npcs.Where(x => x != AllNpcStub).ToList());
        }

        public override async Task Initialize()
        {
            ResetSelected.Enabled = false;
            IoC.Notif.ShowUnknownProgress();

            Models.Clear();
            await UpdateModelList(x => Models.Add(x));

            Npcs.Clear();
            await UpdateModelList(x => Npcs.Add(new ValuePair<Model>(x, x)));
            Npcs.Add(AllNpcStub);

            LoadSettings();
        }

        private async Task UpdateModelList(Action<Model> add)
        {
            var models = await LoadCharacterModelList(true);
            foreach (var model in models)
            {
                model.DisplayName = model.ToString();
                add(model);
            }

            models = await LoadCharacterModelList(false);
            foreach (var model in models)
            {
                model.DisplayName = model.ToString();
                add(model);
            }
        }

        private async Task<List<CharacterModel>> LoadCharacterModelList(bool unique)
        {
            return await Async.Run(() =>
                {
                    IoC.Notif.ShowStatus("Loading characters list...");
                    return CharacterGen.GetCharacterModels(unique);
                });
        }

        public bool Filter(object obj)
        {
            var model = (CharacterModel)obj;
            if (FilterValue.HasFlag(ModelFilter.MainCharacters))
                return model.UniqueCharacter;
            return true;
        }
        public bool NpcFilter(object obj)
        {
            if (ApplyToAll)
                return obj == AllNpcStub;
            if (obj != AllNpcStub)
            {
                var model = (CharacterModel)((ValuePair<Model>)obj).Default;
                if (FilterValue.HasFlag(ModelFilter.MainCharacters))
                    return model.UniqueCharacter;
                return true;
            }

            return false;
        }

        private void OnNpcSelectionChanged()
        {
            ResetSelected.Enabled = SelectedNpcModels?.Count > 0;

            var selectedModelIds = GetSelectedNpcs()
                .Where(x => x != AllNpcStub)
                .Select(x => x.Value.Id).ToHashSet();

            foreach (var model in Models)
            {
                if (selectedModelIds.Contains(model.Id))
                    model.Checked = selectedModelIds.Count > 1 ? null : (bool?)true;
                else
                    model.Checked = false;
            }
        }
        private List<ValuePair<Model>> GetSelectedNpcs()
        {
            if (IoC.Settings.ApplyToAllNpcs)
                return Npcs.ToList();
            return SelectedNpcModels?.Cast<ValuePair<Model>>().ToList() ?? new List<ValuePair<Model>>();
        }

        private void OnModelsSelectionChanged()
        {
            if (SelectedModelMapping == null)
                return;

            SelectedModelMapping.Checked = true;
            foreach (var model in Models)
            {
                if (!ReferenceEquals(SelectedModelMapping, model))
                    model.Checked = false;
            }

            foreach (var outfit in GetSelectedNpcs())
                UpdateMapping(outfit, SelectedModelMapping);
        }

        private void UpdateMapping(ValuePair<Model> npc, Model model)
        {
            npc.Value = model;
        }
        
        private void OnApplyToAll()
        {
            IoC.Settings.ApplyToAllNpcs = ApplyToAll;
            if (ApplyToAll)
            {
                var tmp = new CharacterModel() { Id = Guid.NewGuid() };
                var val = Npcs.Any(x => x != AllNpcStub && x.Modified) ? tmp : AllNpcStub.Default;
                AllNpcStub.Value = val;
            }
            NpcsView.Refresh();

            OnNpcSelectionChanged();
        }

        private void OnFilterValue()
        {
            IoC.Settings.NpcModelFilter = (int)FilterValue;
            NpcsView.Refresh();
            ModelsView.Refresh();
        }

        private Task OnResetSelected()
        {
            foreach (var npc in GetSelectedNpcs())
                npc.Value = npc.Default;

            return Task.CompletedTask;
        }

        private Task OnResetAll()
        {
            foreach (var npc in Npcs)
                npc.Value = npc.Default;

            return Task.CompletedTask;
        }

        public void OnPropertyChanged(string propertyName, object before, object after)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            switch (propertyName)
            {
                case nameof(SelectedNpcModels):
                    OnNpcSelectionChanged();
                    break;
                case nameof(SelectedModelMapping):
                    OnModelsSelectionChanged();
                    break;
                case nameof(ApplyToAll):
                    OnApplyToAll();
                    break;
                case nameof(FilterValue):
                    OnFilterValue();
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
