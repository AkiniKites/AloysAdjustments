using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HZDOutfitEditor.Utility
{
    public class FileRenamer : IDisposable
    {
        private string _path;
        private string _tempPath;

        public FileRenamer(string path)
        {
            _path = path;

            if (File.Exists(path))
            {
                _tempPath = path + Guid.NewGuid();
                File.Move(path, _tempPath);
            }
        }

        public void Dispose()
        {
            if (_tempPath != null && File.Exists(_tempPath))
                File.Move(_tempPath, _path);
        }
    }
}
