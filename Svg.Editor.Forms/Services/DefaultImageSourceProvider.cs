using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Svg.Editor.Interfaces;
using Xamarin.Forms;

namespace Svg.Editor.Forms.Services
{
    public class DefaultImageSourceProvider : IImageSourceProvider
    {
        private static readonly Lazy<string[]> Resources = new Lazy<string[]>(() => typeof(DefaultImageSourceProvider).GetTypeInfo().Assembly.GetManifestResourceNames());
        private static readonly ConcurrentDictionary<string, string> Cache = new ConcurrentDictionary<string, string>();

        public  virtual FileImageSource GetImage(string image)
        {
            if (image == null)
                return GetDefaultImage();

            var resource = Resources.Value.FirstOrDefault(r => r.EndsWith(image));
            // if this is a local resource file
            if (resource != null && resource.EndsWith(".svg"))
            {
                var cache = Engine.TryResolve<ISvgCachingService>();
                if (cache != null)
                {
                    if (Cache.ContainsKey(resource))
                        return Cache[resource];

                    var cached = cache.GetCachedPng(resource, new SaveAsPngOptions());
                    Cache.AddOrUpdate(resource, cached, (o, n) => n);
                    return cached;
                }
            }

            return image;
        }
        protected virtual string GetDefaultImage()
        {
            return null;
        }
    }
}