using Svg.Editor.Interfaces;
using Svg.Interfaces;
using Xamarin.Forms;

namespace Svg.Editor.Forms.Services
{
    public class FormsToolBarIconSizeProvider : IToolbarIconSizeProvider
    {
        public SizeF GetSize()
        {
            return Device.OnPlatform(SizeF.Create(30, 30), SizeF.Create(30, 30), SizeF.Create(120, 120));
        }
    }
}
