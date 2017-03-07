using Svg.Editor.Interfaces;
using Svg.Interfaces;
using Xamarin.Forms;

namespace Svg.Editor.Forms.Services
{
    public class FormsToolBarIconSizeProvider : IToolbarIconSizeProvider
    {
        public SizeF GetSize()
        {
            return Device.OnPlatform(SizeF.Create(32, 32), SizeF.Create(32, 32), SizeF.Create(32, 32));
        }
    }
}
