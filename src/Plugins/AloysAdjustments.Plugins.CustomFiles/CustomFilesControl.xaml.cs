using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Utility;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;

namespace AloysAdjustments.Plugins.CustomFiles
{
    /// <summary>
    /// Interaction logic for CustomeFilesControl.xaml
    /// </summary>
    public partial class CustomeFilesControl : INotifyPropertyChanged
    {
        public override string PluginName => "Custom Files";

        private CustomFilesLogic Logic { get; }

        private List<CustomFile> Files { get; set; } = new List<CustomFile>();
        public ICollectionView FilesView { get; }

        public RelayCommand AddFolderCommand => new RelayCommand(AddFolder);
        public RelayCommand AddFileCommand => new RelayCommand(AddFile);

        public CustomeFilesControl()
        {
            Logic = new CustomFilesLogic();
            
            FilesView = new ListCollectionView(Files);
            FilesView.SortDescriptions.Add(new SortDescription(nameof(CustomFile.Valid), ListSortDirection.Descending));
            FilesView.SortDescriptions.Add(new SortDescription(nameof(CustomFile.Name), ListSortDirection.Ascending));

            InitializeComponent();
        }

        public override Task LoadPatch(string path)
        {
            return Task.CompletedTask;
        }

        public override void ApplyChanges(Patch patch)
        {
            if (Files.Any())
            {
                Logic.AddFilesToPatch(patch, Files);
            }
        }

        public override async Task Initialize()
        {
            await Async.Run(Logic.Initialize);
        }

        private void AddFolder()
        {
            var ofd = new VistaFolderBrowserDialog();
            if (ofd.ShowDialog() == true)
            {
                AddFiles(Logic.GetFilesFromDir(ofd.SelectedPath));
            }
        }

        private void AddFile()
        {
            var ofd = new OpenFileDialog()
            {
                Filter = "Supported files (*.bin, *.zip)|*.bin;*.zip|All files (*.*)|*.*",
                Multiselect = true
            };
            if (ofd.ShowDialog() == true)
            {
                AddFiles(ofd.FileNames.SelectMany(Logic.GetFiles));
            }
        }

        private void AddFiles(IEnumerable<CustomFile> files)
        {
            foreach (var f in files)
                Files.Add(f);
            FilesView.Refresh();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
