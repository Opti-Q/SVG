using Svg.Interfaces;

namespace Svg
{
    public class PlatformSetupSetUp : SvgPlatformSetupBase
    {
        protected override void Initialize()
        {
            base.Initialize();

            Engine.Register<IFactory, Factory>(() => new Factory());
        }
    }
}