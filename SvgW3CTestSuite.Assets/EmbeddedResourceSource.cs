using System;
using System.IO;
using System.Reflection;
using Svg.Interfaces;

namespace SvgW3CTestSuite.Assets
{
    public class EmbeddedResourceSource : ISvgSource
    {
        private readonly string _name;
        private readonly Assembly _assembly;

        public EmbeddedResourceSource(string name, Assembly assembly)
        {
            _name = name;
            _assembly = assembly;
        }

        public Stream GetStream()
        {
            return _assembly.GetManifestResourceStream(_name);
        }

        public Stream GetFileRelativeTo(Uri relativePath)
        {
            throw new NotImplementedException();
        }
    }
}
