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
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
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

        private ModFileLoader Loader { get; }

        public ObservableCollection<Mod> Mods { get; set; }

        public RelayCommand AddFolderCommand => new RelayCommand(AddFolder);
        public RelayCommand AddFileCommand => new RelayCommand(AddFile);

        public CustomFilesControl()
        {
            Loader = new ModFileLoader();
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
            await Async.Run(Loader.Initialize);

            for (int i = 0; i < 10; i++)
                Mods.Add(new Mod() { Name = i.ToString() });
        }

        private void AddFolder()
        {
            var ofd = new VistaFolderBrowserDialog();
            if (ofd.ShowDialog() != true)
                return;

            var mod = Loader.LoadPath(ofd.SelectedPath);
            if (mod == null || !mod.Files.Any())
            {
                MessageBox.Show("Unable to find any files in the folder", "No files found",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AddMods(mod);
        }

        private void AddFile()
        {
            var ofd = new OpenFileDialog()
            {
                Filter = "Supported files (*.bin, *.zip)|*.bin;*.zip|All files (*.*)|*.*",
                Multiselect = true
            };
            if (ofd.ShowDialog() != true)
                return;

            var mods = ofd.FileNames.Select(x => (x, Loader.LoadPath(x))).ToList();
            var invalid = mods.Where(x => x.Item2 == null || !x.Item2.Files.Any()).ToList();
            if (invalid.Any())
            {
                MessageBox.Show(
                    $"Unable to load file(s):\r\n{String.Join("\r\n", invalid.Select(x=>Path.GetFileName(x.x)))}", 
                    "Failed to load some files",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            AddMods(mods.Where(x => x.Item2?.Files.Any() == true)
                .Select(x => x.Item2).ToArray());
        }

        private void AddMods(params Mod[] mods)
        {
            foreach (var mod in mods)
                Mods.Add(mod);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
