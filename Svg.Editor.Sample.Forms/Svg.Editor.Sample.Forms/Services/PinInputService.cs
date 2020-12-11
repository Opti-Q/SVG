using Acr.UserDialogs;
using Svg.Editor.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg.Editor.Sample.Forms.Services
{
	public class PinInputService : IPinInputService
    {
        public async Task<PinTool.PinSize> GetUserInput(IEnumerable<string> pinSizeOptions)
        {
            var defaultResult = PinTool.PinSize.Medium;

            var sizeResult = await UserDialogs.Instance.ActionSheetAsync("Select pin size", "Cancel", null, null, pinSizeOptions.ToArray());

            if (sizeResult == "Cancel")
            {
                return defaultResult;
            }

            PinTool.PinSize result;
            if (!System.Enum.TryParse(sizeResult, out result))
            {
                result = defaultResult;
            }

            return result;
        }
    }
}
