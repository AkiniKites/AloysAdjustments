using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.CustomFiles.Configuration;
using AloysAdjustments.Plugins.NPC;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;

namespace AloysAdjustments.Plugins.CustomFiles
{
    public class CustomFilesPlugin : InteractivePlugin, INotifyPropertyChanged
    {
        public override string PluginName => "Custom Files";

        private ModFileManager ModManager { get; }

        public ObservableCollection<Mod> Mods { get; set; }
        public IList SelectedMods { get; set; }

        public RelayCommand AddFolderCommand => new RelayCommand(AddFolder);
        public RelayCommand AddFileCommand => new RelayCommand(AddFile);

        public CustomFilesPlugin()
        {
            ModManager = new ModFileManager();

            Mods = new ObservableCollection<Mod>();
            Mods.CollectionChanged += ModsOnCollectionChanged;

            PluginControl = new CustomFilesView();
            PluginControl.DataContext = this;
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
            Settings.BindModuleSettings<ModSettings>(PluginName);

            await ModManager.Initialize();
            
            foreach (var mod in ModManager.Mods)
                Mods.Add(mod);
        }

        private void AddFolder()
        {
            var ofd = new VistaFolderBrowserDialog();
            if (ofd.ShowDialog() != true)
                return;

            var mod = ModManager.AddFolder(ofd.SelectedPath);
            AddMod(mod);
        }

        private void AddFile()
        {
            var ofd = new OpenFileDialog()
            {
                Filter = "Supported files (*.bin, *.zip)|*.bin;*.zip|All files (*.*)|*.*"
            };
            if (ofd.ShowDialog() != true)
                return;

            var mod = ModManager.AddFile(ofd.FileName);
            AddMod(mod);
        }

        private void AddMod(Mod mod)
        {
            Mods.Add(mod);
        }

        private void UpdateFileOverride()
        {
            for (int i = 0; i < Mods.Count; i++)
            {
                var files = Mods[i].Files.Keys.ToList();

                bool overrides = false;
                for (int j = 0; j < i; j++)
                {
                    if (files.Any(x => Mods[j].Files.ContainsKey(x)))
                    {
                        Mods[j].Status |= ModStatus.OverridenFiles;
                        Mods[i].Status = ModStatus.OverridesFiles;
                        overrides = true;
                    }
                }

                if (!overrides)
                    Mods[i].Status = ModStatus.Normal;
            }
        }

        private void OnSelectedMods()
        {
            if (SelectedMods.Count != 1)
            {
                foreach (var mod in Mods)
                    mod.Status = ModStatus.Normal;
                return;
            }

            var files = ((Mod)SelectedMods[0]).Files;
            var before = true;

            foreach (var mod in Mods)
            {
                if (ReferenceEquals(mod, SelectedMods[0]))
                {
                    before = false;
                    continue;
                }

                if (mod.Files.Any(x => files.ContainsKey(x.Key)))
                    mod.SelectedStatus = before ? ModStatus.OverridenFiles : ModStatus.OverridesFiles;
                else
                    mod.SelectedStatus = ModStatus.Normal;
            }
        }

        private void ModsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < Mods.Count; i++)
                Mods[i].Order = i;
        }

        public void OnPropertyChanged(string propertyName, object before, object after)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            switch (propertyName)
            {
                case nameof(SelectedMods):
                    OnSelectedMods();
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
