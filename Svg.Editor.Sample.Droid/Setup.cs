using Android.Content;
using MvvmCross.Droid.Platform;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Platform;
using Svg.Editor;
using Svg.Editor.Droid.Services;
using Svg.Editor.Services;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Droid.SampleEditor
{
    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
		{
			SvgEditor.Init();

			// Register SVG services
			SvgEngine.Register<IColorInputService>(() => new AndroidColorInputService());
			SvgEngine.Register<ITextInputService>(() => new AndroidTextInputService());
			SvgEngine.Register<IMarkerOptionsInputService>(() => new AndroidMarkerOptionsInputService());
			SvgEngine.Register<IStrokeStyleOptionsInputService>(() => new AndroidStrokeStyleOptionsInputService());
			SvgEngine.Register<IContextProvider>(() => new AndroidContextProvider());
		}

        protected override IMvxApplication CreateApp()
        {
            return new Core.App();
        }

        protected override IMvxTrace CreateDebugTrace()
        {
            return new DebugTrace();
        }
    }
}
