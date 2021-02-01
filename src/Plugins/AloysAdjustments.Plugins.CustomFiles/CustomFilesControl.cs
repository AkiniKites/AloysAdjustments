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

            lbFiles.DisplayMember = "Name";
            lbFiles.DrawMode = DrawMode.OwnerDrawVariable;
            lbFiles.ItemHeight = lbFiles.Font.Height + 2;
            lbFiles.DrawItem += (s, e) =>
            {
                if (e.Index < 0)
                    return;

                var l = (ListBox)s;

                e.DrawBackground();

                var valid = Files[e.Index].Valid;

                var foreColor = valid ? e.ForeColor : UIColors.ErrorColor;
                using (var b = new SolidBrush(foreColor))
                {
                    var text = l.GetItemText(l.Items[e.Index]);
                    e.Graphics.DrawString(text, e.Font, b, e.Bounds, StringFormat.GenericDefault);
                }
                e.DrawFocusRectangle();
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
            Files.Sort();

            lbFiles.Items.Clear();
            lbFiles.Items.AddRange(Files.ToArray());
        }
    }
}
