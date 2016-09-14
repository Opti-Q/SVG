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

        protected ToolBase(string name, IDictionary<string,object> properties) : this(name)
        {
            //Properties = JsonConvert.DeserializeObject<IDictionary<string, object>>(properties, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }) ?? new Dictionary<string, object>();
            Properties = properties ?? new Dictionary<string, object>();
        }

        protected SvgDrawingCanvas Canvas { get; private set; }
        public string Name { get; protected set; }
        public ToolUsage ToolUsage { get; protected set; }
        public ToolType ToolType { get; protected set; }
        public virtual bool IsActive { get; set; } = true;
        public IEnumerable<IToolCommand> Commands { get; protected set; } = Enumerable.Empty<IToolCommand>();
        /// <summary>
        /// Properties for the tool which can be configured in the designer. Key should be lower-case for consistency.
        /// </summary>
        public IDictionary<string, object> Properties { get; }

        public virtual int DrawOrder => 500;
        public virtual int PreDrawOrder => 500;
        public virtual int InputOrder => 500;

        public string IconName { get; set; }

        public virtual Task Initialize(SvgDrawingCanvas ws)
        {
            Canvas = ws;

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
