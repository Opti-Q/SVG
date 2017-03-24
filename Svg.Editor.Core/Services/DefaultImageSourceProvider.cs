using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Svg.Interfaces;

namespace Svg.Editor.Services
{
    public class DefaultImageSourceProvider : IImageSourceProvider
    {
        private static readonly Lazy<string[]> Resources = new Lazy<string[]>(() =>
            {
                var registry = SvgEngine.Resolve<IEmbeddedResourceRegistry>();
                return registry.EmbeddedResourceTypes.SelectMany(t => t.GetTypeInfo().Assembly.GetManifestResourceNames()).ToArray();
            }
        );
        private static readonly ConcurrentDictionary<string, string> Cache = new ConcurrentDictionary<string, string>();

        

        public  virtual string GetImage(string image, SizeF dimension = null)
        {
            if (image == null)
                return GetDefaultImage();

            var resource = Resources.Value.FirstOrDefault(r => r.EndsWith(image));
            // if this is a local resource file
            if (resource != null && resource.EndsWith(".svg"))
            {
                var cache = SvgEngine.TryResolve<ISvgCachingService>();
                if (cache != null)
                {
                    if (Cache.ContainsKey(resource))
                        return Cache[resource];

                    var cached = cache.GetCachedPng(resource, new SaveAsPngOptions() {ImageDimension = dimension});
                    Cache.AddOrUpdate(resource, cached, (o, n) => n);
                    return cached;
                }
            }

            return image;
        }

        public virtual string GetImage(string image, SaveAsPngOptions options)
        {
            if (image == null)
                return GetDefaultImage();

            var resource = Resources.Value.FirstOrDefault(r => r.EndsWith(image));
            // if this is a local resource file
            if (resource != null && resource.EndsWith(".svg"))
            {
                var cache = SvgEngine.TryResolve<ISvgCachingService>();
                if (cache != null)
                {
                    if (Cache.ContainsKey(resource))
                        return Cache[resource];

                    var cached = cache.GetCachedPng(resource, options);
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