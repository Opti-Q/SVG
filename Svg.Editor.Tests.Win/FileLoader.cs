using NUnit.Framework;

namespace Svg.Editor.Tests
{
    public class FileLoader : IFileLoader
    {
        public SvgDocument Load(string fileName)
        {
            var path = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", fileName);
            using (var str = System.IO.File.OpenRead(path))
            {
                return SvgDocument.Open<SvgDocument>(str);
            }
        }
    }
}
