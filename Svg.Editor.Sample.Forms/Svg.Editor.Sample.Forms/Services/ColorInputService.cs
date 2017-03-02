using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Svg.Editor.Sample.Forms.Services
{
    public class ColorInputService : Svg.Editor.Tools.IColorInputService
    {
        public async Task<int> GetIndexFromUserInput(string title, string[] items, string[] colors)
        {
            var result = await Application.Current.MainPage.DisplayActionSheet(title, "cancel", null, items);

            if (result == null)
                return 0;

            var index = items.ToList().IndexOf(result);
            if (index < 0)
                return 0;


            return index;
        }
    }
}
