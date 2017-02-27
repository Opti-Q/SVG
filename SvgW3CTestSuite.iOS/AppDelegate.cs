using System.Linq;

using Foundation;
using Plugin.Toasts;
using SvgW3CTestSuite.Assets;
using SvgW3CTestSuite.Droid;
using UIKit;
using Xamarin.Forms;

namespace SvgW3CTestSuite.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        // class-level declarations
        UIWindow window;
        private static NUnit.Runner.App nunit = null;

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // Once you called base.OnCreate(), you cannot add more assemblies.
            global::Xamarin.Forms.Forms.Init();

            // register Xamarin.Form.Toasts plugin so we can see the progress of our test
            DependencyService.Register<ToastNotificatorImplementation>();
            ToastNotificatorImplementation.Init();

            // create a new window instance based on the screen size
            window = new UIWindow(UIScreen.MainScreen.Bounds);
            //runner = new TouchRunner(window);

            //// register every tests included in the main application/assembly
            //runner.Add(System.Reflection.Assembly.GetExecutingAssembly());

            //window.RootViewController = new UINavigationController(runner.GetViewController());

            if (nunit == null)
            {
                // get all SVG assets
                var svgFiles = AssetHelper.GetAllSvgFiles().Take(400)/*.Where(s => !s.StartsWith("struct-image"))*/;
                
                W3CTestFixture.SvgTestCases = svgFiles.Select(path => new object[]
                                                        {
                                                            path,
                                                            AssetHelper.GetPngForSvg(path)
                                                        })
                                                        .ToArray();
                W3CTestFixture.FileSourceProvider = (path) => AssetHelper.GetSource(path);

                // This will load all tests within the current project
                nunit = new NUnit.Runner.App();

                // If you want to add tests in another assembly
                //nunit.AddTestAssembly(typeof(MyTests).Assembly);

                // Do you want to automatically run tests when the app starts?
                nunit.AutoRun = false;

                LoadApplication(nunit);
            }

            // make the window visible
            window.MakeKeyAndVisible();

            return true;
        }
    }
}