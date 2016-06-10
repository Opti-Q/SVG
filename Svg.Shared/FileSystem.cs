using System.IO;
using Svg.Interfaces;

namespace Svg
{
    public class FileSystem : IFileSystem
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        public Stream OpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public Stream OpenWrite(string path)
        {
            return new FileStream(path, FileMode.Create, FileAccess.Write);
        }

        public string GetFullPath(string path)
        {
            return path;
        }
    }
}