using System.Collections.Generic;
using System.Linq;
using Svg.Core.Events;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public abstract class ToolBase : ITool
    {
        protected ToolBase(string name)
        {
            Name = name;
        }

        public virtual void OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
        }

        public virtual void OnPreDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
        }

        public virtual void OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            
        }
        
        public virtual void Reset()
        {
        }

        public IEnumerable<IToolCommand> Commands { get; protected set; } = Enumerable.Empty<IToolCommand>();
        public virtual bool IsActive { get; set; }
        public string Name { get; protected set; }

        public virtual void Dispose()
        {
        }
    }
}
