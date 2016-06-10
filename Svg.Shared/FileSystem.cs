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
    }
}