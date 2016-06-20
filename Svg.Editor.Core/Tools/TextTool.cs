using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public interface ITextInputService
    {
        Task<string> GetUserInput(string title);
    }

    public class TextTool : ToolBase
    {
        private ITextInputService _textInputService;

        public TextTool() : base("Text")
        {
        }

        private ITextInputService TextInputService
        {
            get { return _textInputService ?? (_textInputService = Engine.Resolve<ITextInputService>()); }
        }

        public override async Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {

            var pe = @event as PointerEvent;
            if (pe.EventType == EventType.PointerUp)
            {
                var dX = pe.Pointer1Position.X - pe.Pointer1Down.X;
                var dY = pe.Pointer1Position.Y - pe.Pointer1Down.Y;

                // if Point-Down and Point-Up are merely the same
                if (dX < 20 && dY < 20)
                {
                    // if there is text below the pointer, edit it
                    var e = ws.GetElementsUnderPointer(pe.Pointer1Position).OfType<SvgText>().FirstOrDefault();

                    if (e != null)
                    {

                    }
                    // else add new text   
                    else
                    {
                        var txt = await TextInputService.GetUserInput("Add text");
                        // only add if user really entered text.
                        if (!string.IsNullOrWhiteSpace(txt))
                        {
                            
                        }
                    }
                }
            }
        }


    }
}
