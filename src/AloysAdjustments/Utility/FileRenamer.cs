using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Utility
{
    public class FileRenamer : IDisposable
    {
        private string _path;
        private string _tempPath;

        public FileRenamer(string path)
        {
            _path = path;

            try
            {
                if (File.Exists(path))
                {
                    _tempPath = path + Guid.NewGuid();
                    File.Move(path, _tempPath);
                }
            }
            catch(IOException ex)
            {
                throw new Exception($"Failed to rename file: {path}", ex);
            }
        }

        public void Delete()
        {
            if (_tempPath != null && File.Exists(_tempPath))
                File.Delete(_tempPath);
        }

        public void Dispose()
        {
            if (_tempPath != null && File.Exists(_tempPath))
                File.Move(_tempPath, _path, true);
        }
    }
}
