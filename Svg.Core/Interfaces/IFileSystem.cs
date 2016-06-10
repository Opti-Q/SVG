using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public interface IFileSystem
    {
        bool FileExists(string path);

        bool FolderExists(string path);

        Stream OpenRead(string path);

        Stream OpenWrite(string path);

        string GetFullPath(string path);
    }
}
