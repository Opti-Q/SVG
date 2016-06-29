using System;
using System.Linq;
using System.Reflection;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.Toasts;
using Svg.Platform;
using Xamarin.Forms;

namespace SvgW3CTestSuite.Droid
{
    [Activity(Label = "SVG W3C TestSuite", MainLauncher = true, Theme = "@android:style/Theme.Holo.Light", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //string[] testCases = new[] {"coords-", "text-", "painting-", "paths-"};

            // get all SVG assets
            var svgFiles = Assets.List("svg")/*.Where(p => testCases.Any(p.StartsWith))*/;
            Func<string, string> getPngPath = (svgPath) =>
            {
                var fileName = System.IO.Path.GetFileNameWithoutExtension(svgPath)+".png";
                return System.IO.Path.Combine("png", fileName);
            };
            W3CTestFixture.SvgTestCases = svgFiles.Select(path => new object[]
                                                {
                                                    System.IO.Path.Combine("svg", path),
                                                    getPngPath(path)
                                                })
                                                .ToArray();
            W3CTestFixture.FileSourceProvider = (path) => new SvgAssetSource(path, Assets);

            // tests can be inside the main assembly
            //AddTest(Assembly.GetExecutingAssembly());
            // or in any reference assemblies
            // AddTest (typeof (Your.Library.TestClass).Assembly);

            // Once you called base.OnCreate(), you cannot add more assemblies.
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            // register Xamarin.Form.Toasts plugin so we can see the progress of our test
            DependencyService.Register<ToastNotificatorImplementation>();
            ToastNotificatorImplementation.Init(this);


            // This will load all tests within the current project
            var nunit = new NUnit.Runner.App();

            // If you want to add tests in another assembly
            //nunit.AddTestAssembly(typeof(MyTests).Assembly);

            // Do you want to automatically run tests when the app starts?
            nunit.AutoRun = true;

            LoadApplication(nunit);
        }
    }
}

