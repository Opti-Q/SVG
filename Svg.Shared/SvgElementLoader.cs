using System;
using System.Runtime.InteropServices;
using Android.Content.Res;
using Svg.Interfaces;

namespace Svg
{
    public class SvgElementLoader : ISvgElementLoader
    {
        private readonly AssetManager _asset;

        public SvgElementLoader(AssetManager asset)
        {
            _asset = asset;
        }

        public SvgElement Load(Uri uri)
        {
            var fullUri = uri.OriginalString;
            if (!uri.IsAbsoluteUri)
            {
                fullUri = uri.OriginalString.Replace("../", null);
            }
            var hash = fullUri.Substring(fullUri.LastIndexOf('#'));
            SvgDocument doc;

            var path = fullUri.Contains(hash)
                ? fullUri.Replace(hash, null)
                : fullUri;

            if (string.IsNullOrEmpty(path))
                return null;

            using (var str = _asset.Open(path))
            {
                doc = SvgDocument.Open<SvgDocument>(str);
            }
            return doc.IdManager.GetElementById(hash);
        }
    }
}