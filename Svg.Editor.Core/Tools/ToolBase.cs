using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Gestures;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public abstract class ToolBase : ITool
    {
        protected ToolBase(string name)
        {
            Name = name;
        }

        protected ToolBase(string name, IDictionary<string, object> properties) : this(name)
        {
            //Properties = JsonConvert.DeserializeObject<IDictionary<string, object>>(properties, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }) ?? new Dictionary<string, object>();
            Properties = properties ?? new Dictionary<string, object>();
        }

        protected SvgDrawingCanvas Canvas { get; private set; }

        public const string NoSnappingCustomAttributeKey = "iclnosnapping";
        public const string BackgroundCustomAttributeKey = "iclbackground";

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
        public virtual int GestureOrder => 500;

        /// <summary>
        /// States if the <see cref="OnDrag"/> method should receive <see cref="DragGesture.Enter"/> gestures.
        /// </summary>
        protected bool HandleDragEnter { get; set; }
        /// <summary>
        /// States if the <see cref="OnDrag"/> method should receive <see cref="DragGesture.Exit"/> gestures.
        /// </summary>
        protected bool HandleDragExit { get; set; }

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

        public virtual async Task OnGesture(UserGesture gesture)
        {
            switch (gesture.Type)
            {
                case GestureType.Tap:
                    await OnTap((TapGesture) gesture);
                    break;
                case GestureType.DoubleTap:
                    await OnDoubleTap((DoubleTapGesture) gesture);
                    break;
                case GestureType.LongPress:
                    await OnLongPress((LongPressGesture) gesture);
                    break;
                case GestureType.Drag:
                    var drag = (DragGesture) gesture;
                    if (drag.State == DragState.Enter && !HandleDragEnter) return;
                    if (drag.State == DragState.Exit && !HandleDragExit) return;
                    await OnDrag((DragGesture) gesture);
                    break;
            }
        }

        protected virtual Task OnTap(TapGesture tap)
        {
            return Task.FromResult(true);
        }

        protected virtual Task OnDoubleTap(DoubleTapGesture doubleTap)
        {
            return Task.FromResult(true);
        }

        protected virtual Task OnLongPress(LongPressGesture longPress)
        {
            return Task.FromResult(true);
        }

        protected virtual Task OnDrag(DragGesture drag)
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
