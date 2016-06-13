using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Svg.Interfaces;
using FileNotFoundException = Java.IO.FileNotFoundException;

namespace Svg.Platform
{
    public class SvgAssetSource : ISvgSource
    {
        private readonly string _filePath;
        private readonly AssetManager _assetManager;

        public SvgAssetSource(string filePath, AssetManager assetManager)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            if (assetManager == null) throw new ArgumentNullException(nameof(assetManager));
            _filePath = filePath;
            _assetManager = assetManager;
        }

        public Stream GetStream()
        {
            return _assetManager.Open(_filePath);
        }

        public Stream GetFileRelativeTo(Uri relativePath)
        {
            var str = relativePath.OriginalString;
            if (str.StartsWith("#"))
                return null;

            var path = Path.GetDirectoryName(_filePath);

            // remove hash if there is any
            var hash = relativePath.OriginalString.Substring(relativePath.OriginalString.LastIndexOf('#'));
            
            if (!string.IsNullOrWhiteSpace(hash))
            {
                str = str.Contains(hash)
                    ? str.Substring(0, str.Length - hash.Length)
                    : str;
            }

            while (str.StartsWith("../"))
            {
                path = Path.GetDirectoryName(path);
                str = str.Substring(3);
            }

            var fullPath = path == null ? str : Path.Combine(path, str);
            try
            {
                return _assetManager.Open(fullPath);
            }
            catch (Java.IO.FileNotFoundException)
            {
                return null;
            }
        }
    }
}