using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Modules;
using AloysAdjustments.UI;
using AloysAdjustments.Utility;
using BrightIdeasSoftware;

namespace AloysAdjustments.Plugins.CustomFiles
{
    public partial class CustomFilesControl : ModuleBase
    {
        public override string PluginName => "Custom Files";

        private CustomFilesLogic Logic { get; }

        private List<CustomFile> Files = new List<CustomFile>();
        
        public CustomFilesControl()
        {
            InitializeComponent();
            
            lvFiles.Columns.Add(new OLVColumn()
            {
                Text = "File Name",
                Width = 500,
                AspectName = "Name"
            });
            lvFiles.Columns.Add(new OLVColumn()
            {
                Text = "Valid",
                AspectName = "Valid",
            });
            lvFiles.Columns.Add(new OLVColumn()
            {
                Text = "Source",
                Width = 500,
                AspectName = "SourcePath"
            });
            lvFiles.Columns.Add(new OLVColumn()
            {
                Text = "",
                FillsFreeSpace = true
            });
            lvFiles.PrimarySortColumn = (OLVColumn)lvFiles.Columns[0];
            lvFiles.PrimarySortOrder = SortOrder.Ascending;
            lvFiles.FormatRow += (s, e) =>
            {
                if (!((CustomFile)e.Model).Valid)
                    e.Item.ForeColor = UIColors.ErrorColor;
            };

            Logic = new CustomFilesLogic();
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
            await Async.Run(Logic.Initialize);
        }

        private void btnAddFolder_Click(object sender, EventArgs e)
        {
            var ofd = new FolderBrowserDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                AddFiles(Logic.GetFilesFromDir(ofd.SelectedPath));
            }
        }

        private void btnAddFiles_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Filter = "Pack files (*.bin)|*.bin|Zip files (*.zip)|*.zip|All files (*.*)|*.*",
                Multiselect = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                AddFiles(ofd.FileNames.SelectMany(Logic.GetFiles));
            }
        }

        private void AddFiles(IEnumerable<CustomFile> files)
        {
            Files.AddRange(files);

            lvFiles.BeginUpdate();
            lvFiles.AddObjects(Files);
            lvFiles.EndUpdate();
        }
    }
}
