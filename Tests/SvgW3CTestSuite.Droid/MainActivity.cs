using System;
using System.Linq;
using System.Reflection;
using Android.App;
using Android.OS;
using Svg.Platform;
using Xamarin.Android.NUnitLite;

namespace SvgW3CTestSuite.Droid
{
    [Activity(Label = "SvgW3CTestSuite.Droid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : TestSuiteActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            // get all SVG assets
            var svgFiles = Assets.List("svg").Where(@s => s.StartsWith("coords-")).Take(3);
            Func<string, string> getPngPath = (svgPath) =>
            {
                var fileName = System.IO.Path.GetFileNameWithoutExtension(svgPath)+".png";
                return System.IO.Path.Combine("png", fileName);
            };
            TestsSample.SvgTestCases = svgFiles.Select(path => new object[]
                                                {
                                                    System.IO.Path.Combine("svg", path),
                                                    getPngPath(path)
                                                })
                                                .ToArray();
            TestsSample.FileSourceProvider = (path) => new SvgAssetSource(path, Assets);

            // tests can be inside the main assembly
            AddTest(Assembly.GetExecutingAssembly());
            // or in any reference assemblies
            // AddTest (typeof (Your.Library.TestClass).Assembly);

            // Once you called base.OnCreate(), you cannot add more assemblies.
            base.OnCreate(bundle);
        }
    }
}

