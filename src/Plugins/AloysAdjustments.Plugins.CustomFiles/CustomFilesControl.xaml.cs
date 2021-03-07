using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Xsl;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.CustomFiles.Configuration;
using AloysAdjustments.Utility;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Path = System.IO.Path;

namespace AloysAdjustments.Plugins.CustomFiles
{
    /// <summary>
    /// Interaction logic for CustomeFilesControl.xaml
    /// </summary>
    public partial class CustomFilesControl : INotifyPropertyChanged
    {
        public override string PluginName => "Custom Files";
        
        private ModFileManager ModManager { get; }

        public ObservableCollection<Mod> Mods { get; set; }

        public RelayCommand AddFolderCommand => new RelayCommand(AddFolder);
        public RelayCommand AddFileCommand => new RelayCommand(AddFile);

        public CustomFilesControl()
        {
            ModManager = new ModFileManager();

            Mods = new ObservableCollection<Mod>();

            InitializeComponent();
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
