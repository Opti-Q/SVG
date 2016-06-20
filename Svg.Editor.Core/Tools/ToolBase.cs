using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public string Name { get; protected set; }

        public virtual bool IsActive { get; set; } = true;

        public IEnumerable<IToolCommand> Commands { get; protected set; } = Enumerable.Empty<IToolCommand>();

        public virtual Task Initialize(SvgDrawingCanvas ws)
        {
            return Task.FromResult(true);
        }

        public virtual Task OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            return Task.FromResult(true);
        }

        public virtual Task OnPreDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            return Task.FromResult(true);
        }

        public virtual Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            return Task.FromResult(true);
        }

        public virtual void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {

        }

        public virtual void Reset()
        {
        }

        public virtual void Dispose()
        {
        }
    }
}
