using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Tools;
using Xamarin.Forms;

namespace Svg.Editor.Sample.Forms.Services
{
    public class MarkerOptionsInputService : IMarkerOptionsInputService
    {
        public async Task<int[]> GetUserInput(string title, IEnumerable<string> markerStartOptions, int markerStartSelected, IEnumerable<string> markerEndOptions,
            int markerEndSelected)
        {
            var result = await Application.Current.MainPage.DisplayActionSheet(title, "cancel", null, markerStartOptions.ToArray());

            if (result == null)
                return new [] {0};

            return new [] { markerStartOptions.ToList().IndexOf(result)};
        }
    }
}
