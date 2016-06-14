using System.Collections.Generic;
using Svg.Core.Events;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public class TextTool : ITool
    {
        private List<IToolCommand> _commands;

        public void OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
        }

        public void OnTouch(UserInputEvent @event, SvgDrawingCanvas ws)
        {
        }

        public void Reset()
        {
        }

        public IEnumerable<IToolCommand> Commands => _commands;
        public bool IsActive { get; set; }
        public string Name => "Text";

        public void Dispose()
        {
        }
    }
}
