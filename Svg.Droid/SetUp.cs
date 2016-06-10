using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Javax.Crypto.Interfaces;
using Svg.Interfaces;

namespace Svg
{
    public class SetUp
    {
        private static IFileSystem _fileSystem = new FileSystem();

        public static void Initialize()
        {
            SvgSetup.Register<IFactory, Factory>(() => new Factory());

            SvgSetup.Register<IFileSystem, FileSystem>(() => _fileSystem);

            // register enumconverters
            // see http://stackoverflow.com/questions/1999803/how-to-implement-a-typeconverter-for-a-type-and-property-i-dont-own
        }
    }
}