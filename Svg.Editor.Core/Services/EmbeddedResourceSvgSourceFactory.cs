using System;
using System.Reflection;
using Svg.Interfaces;
using Svg.Platform;

namespace Svg.Editor.Services
{
    public class EmbeddedResourceSvgSourceFactory : ISvgSourceFactory
    {
        private readonly Type _targetAssemblyType;

        public EmbeddedResourceSvgSourceFactory()
        {
            _targetAssemblyType = this.GetType();
        }

        public EmbeddedResourceSvgSourceFactory(Type targetAssemblyType)
        {
            if (targetAssemblyType == null) throw new ArgumentNullException(nameof(targetAssemblyType));
            _targetAssemblyType = targetAssemblyType;
        }

        public ISvgSource Create(string path)
        {
            return new EmbeddedResourceSource(path, _targetAssemblyType.GetTypeInfo().Assembly);
        }
    }
}
