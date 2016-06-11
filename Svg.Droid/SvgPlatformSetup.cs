using Android.App;
using Android.Content;
using Svg.Interfaces;

namespace Svg
{
    public class SvgPlatformSetup : SvgPlatformSetupBase
    {
        protected override void Initialize()
        {
            base.Initialize();

            Engine.Register<IFactory, Factory>(() => new Factory());
        }

        public static void Init(Context context)
        {
            new SvgPlatformSetup().Initialize();
            Engine.Register<ISvgElementLoader, SvgElementLoader>(() => new SvgElementLoader(context.Assets));
        }
    }
}