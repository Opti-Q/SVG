using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Core.Tools;
using Svg.Interfaces;

namespace Svg.Core
{
    public class SvgDrawingCanvas : IDisposable, ICanInvalidateCanvas
    {
        private readonly ObservableCollection<SvgElement> _selectedElements = new ObservableCollection<SvgElement>();
        private readonly ObservableCollection<ITool> _tools;

        public event EventHandler CanvasInvalidated;

        public void InvalidateCanvas()
        {
            CanvasInvalidated?.Invoke(this, EventArgs.Empty);
        }

        public SvgDrawingCanvas()
        {
            Translate = Svg.Engine.Factory.CreatePointF(0f, 0f);
            ZoomFactor = 1f;

            _tools = new ObservableCollection<ITool>
            { 
            //        new ZoomTool(),
                    new GridTool(this),
            //        new SelectionTool(),
            //        new MoveSvgTool(),
            //        new PanTool(),
            //        new SnappingTool(),
            };
        }

        public ObservableCollection<SvgElement> SelectedElements => _selectedElements;

        public ObservableCollection<ITool> Tools => _tools;

        public SvgDocument Document { get; set; }

        public PointF Translate { get; set; }

        public float ZoomFactor { get; set; }

        /// <summary>
        /// Called by the platform specific input event detector whenever the user interacts with the model
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="view"></param>
        public void OnEvent(InputEvent ev)
        {
            foreach (var tool in Tools)
            {
                tool.OnTouch(ev, this);
            }
        }

        /// <summary>
        /// Called by platform specific implementation to allow tools to draw something onto the canvas
        /// </summary>
        /// <param name="renderer"></param>
        public void OnDraw(IRenderer renderer)
        {
            foreach (var tool in Tools)
            {
                tool.OnDraw(renderer, this);
            }
        }

        public void Dispose()
        {
            foreach(var tool in Tools)
                tool.Dispose();
        }

        public IEnumerable<IEnumerable<IToolCommand>> ToolCommands => Tools.Select(t => t.Commands);
    }
}
