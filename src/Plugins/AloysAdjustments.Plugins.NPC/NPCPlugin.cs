using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.NPC;
using AloysAdjustments.Plugins.Outfits.Data;
using AloysAdjustments.UI;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Plugins.Outfits
{
    public class NPCPlugin : InteractivePlugin, INotifyPropertyChanged
    {
        public override string PluginName => "NPC Models";

        private CharacterGenerator CharacterGen { get; }
        private OutfitPatcher Patcher { get; }

        public ObservableCollection<ValuePair<Model>> Npcs { get; set; }
        public ICollectionView NpcsView { get; set; }
        public ObservableCollection<Model> Models { get; set; }
        public ICollectionView ModelsView { get; set; }

        public ReadOnlyCollection<OutfitModelFilter> Filters { get; set; }

        public IList SelectedNpcModels { get; set; }
        public ValuePair<Model> SelectedModelMapping { get; set; }

        public int FilterValue { get; set; }

        public NPCPlugin()
        {
            IoC.Bind(Configs.LoadModuleConfig<OutfitConfig>(PluginName));

            Reset = new ControlRelay(() => Task.CompletedTask);
            ResetSelected = new ControlRelay(() => Task.CompletedTask);

            CharacterGen = new CharacterGenerator();
            Patcher = new OutfitPatcher();
            
            Filters = OutfitModelFilter.All.ToList().AsReadOnly();

            LoadSettings();

            PluginControl = new NPCPluginView();
            PluginControl.DataContext = this;

            Models = new ObservableCollection<Model>();
            ModelsView = CollectionViewSource.GetDefaultView(Models);

            Npcs = new ObservableCollection<ValuePair<Model>>();
            NpcsView = CollectionViewSource.GetDefaultView(Npcs);

            ModelsView.Filter = Filter;
        }

        private void LoadSettings()
        {
            var filter = IoC.Settings.OutfitModelFilter;
            if (filter == 0)
                filter = OutfitModelFilter.Characters.Value;

            //foreach (var f in Filters.Where(x => x.IsFlag(filter)))
            //    ccbModelFilter.SelectedItems.Add(f);
        }

        public override Task LoadPatch(string path)
        {
            return Task.CompletedTask;
        }

        public override void ApplyChanges(Patch patch)
        {
        }

        public override async Task Initialize()
        {
            ResetSelected.Enabled = false;
            IoC.Notif.ShowUnknownProgress();

            await UpdateModelList();
            await UpdateNpcList();
        }

        private async Task UpdateModelList()
        {
            Models.Clear();
            var models = await LoadCharacterModelList(true);
            foreach (var model in models)
            {
                model.DisplayName = model.ToString();
                Models.Add(model);
            }

            models = await LoadCharacterModelList(false);
            foreach (var model in models)
            {
                model.DisplayName = model.ToString();
                Models.Add(model);
            }
        }
        private async Task UpdateNpcList()
        {
            Npcs.Clear();
            var models = await LoadCharacterModelList(true);
            foreach (var model in models)
            {
                model.DisplayName = model.ToString();
                Npcs.Add(new ValuePair<Model>(model, model));
            }

            models = await LoadCharacterModelList(false);
            foreach (var model in models)
            {
                model.DisplayName = model.ToString();
                Npcs.Add(new ValuePair<Model>(model, model));
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

        public void UpdateModelDisplayNames(IList<ValuePair<Model>> models)
        {
            foreach (var m in models)
            {
                m.Value.DisplayName = m.Value.ToString();
                m.Default.DisplayName = m.Default.ToString();
            }
        }

        public bool Filter(object obj)
        {
            var model = (CharacterModel)obj;
            if (FilterValue == OutfitModelFilter.Characters.Value)
                return model.UniqueCharacter;
            return true;
        }

        public void OnPropertyChanged(string propertyName, object before, object after)
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
