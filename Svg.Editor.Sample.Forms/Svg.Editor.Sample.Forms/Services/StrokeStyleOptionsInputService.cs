using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg.Editor.Tools;

namespace Svg.Editor.Sample.Forms.Services
{
    public class StrokeStyleOptionsInputService : IStrokeStyleOptionsInputService
    {
        public Task<StrokeStyleTool.StrokeStyleOptions> GetUserInput(string title, IEnumerable<string> strokeDashOptions, int strokeDashSelected, IEnumerable<string> strokeWidthOptions,
            int strokeWidthSelected)
        {
            return Task.FromResult(new StrokeStyleTool.StrokeStyleOptions() {StrokeDashIndex = 0, StrokeWidthIndex = 0});
        }
    }
}
