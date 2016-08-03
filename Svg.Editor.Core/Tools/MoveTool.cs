﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Interfaces;
using Svg.Transforms;

namespace Svg.Core.Tools
{
    public class MoveTool : ToolBase
    {
        private readonly Dictionary<object, PointF> _offsets = new Dictionary<object, PointF>();
        private readonly Dictionary<object, PointF> _translates = new Dictionary<object, PointF>();
        private bool _implicitlyActivated = false;

        public MoveTool() : base("Move")
        {
        }
        
        public override Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            var p = @event as PointerEvent;
            if (p != null)
            {
                if (p.EventType == EventType.PointerDown)
                {
                    // determine if active by searching thorough selection and determining whether pointer was put on selected element
                    // if there are selected elements and pointer was put down on one of them, activate tool, otherwhise deactivate
                    if (ws.SelectedElements.Count != 0 &&
                        ws.GetElementsUnderPointer<SvgVisualElement>(p.Pointer1Position).Any(eup => ws.SelectedElements.Contains(eup)))
                    {
                        // move tool is only active, if SelectionTool is the "ActiveTool"
                        // otherwise we'd move and pan at the same time, yielding confusing results... :)
                        if (ws.ActiveTool is SelectionTool)
                        {
                            ws.ActiveTool = this;
                            _implicitlyActivated = true;
                        }
                    }
                    else
                    {
                        this.IsActive = false;
                    }
                }
                // clear offsets 
                else if (p.EventType == EventType.Cancel || p.EventType == EventType.PointerUp)
                {
                    _offsets.Clear();
                    _translates.Clear();

                    if (_implicitlyActivated)
                    {
                        ws.ActiveTool = ws.Tools.OfType<SelectionTool>().Single();
                    }
                }
            }

            // skip moving if inactive
            if (!this.IsActive)
                return Task.FromResult(true);


            var e = @event as MoveEvent;

            if (e == null)
            {
                _offsets.Clear();
                _translates.Clear();
            }
            else
            {
                // if there is no selection, we just skip
                if (ws.SelectedElements.Any())
                {
                    var absoluteDeltaX = e.AbsoluteDelta.X / ws.ZoomFactor;
                    var absoluteDeltaY = e.AbsoluteDelta.Y / ws.ZoomFactor;

                    // add translation to every element
                    foreach (var element in ws.SelectedElements)
                    {
                        PointF previousDelta;
                        if (!_offsets.TryGetValue(element, out previousDelta))
                        {
                            previousDelta = PointF.Create(0f, 0f);
                        }
                        
                        var relativeDeltaX = absoluteDeltaX - previousDelta.X;
                        var relativeDeltaY = absoluteDeltaY - previousDelta.Y;

                        
                        previousDelta.X = absoluteDeltaX;
                        previousDelta.Y = absoluteDeltaY;
                        _offsets[element] = previousDelta;
                        
                        AddTranslate(element, relativeDeltaX, relativeDeltaY);
                    }   

                    ws.FireInvalidateCanvas();
                }
            }

            return Task.FromResult(true);
        }

        private void AddTranslate(SvgVisualElement element, float deltaX, float deltaY)
        {
            SvgTranslate trans = null;
            int index = -1;
            
            //if (element.Transforms.OfType<SvgTranslate>().Count() > 1)
            {
                for (int i = element.Transforms.Count - 1; i >= 0; i--)
                {
                    var translate = element.Transforms[i] as SvgTranslate;
                    if (translate != null)
                    {
                        trans = translate;
                        index = i;
                        break;
                    }
                }
            }

            // the movetool stores the last translation explicitly for each element
            // that way, if another tool manipulates the translation (e.g. the snapping tool)
            // the movetool is not interfered by that
            PointF previousTranslate;
            if (!_translates.TryGetValue(element, out previousTranslate))
            {
                if (trans != null)
                    previousTranslate = PointF.Create(trans.X, trans.Y);
                else
                    previousTranslate = PointF.Create(0f, 0f);
            }

            var transforms = element.Transforms;
            if (trans == null)
            {
                trans = new SvgTranslate(deltaX, deltaY);
                _translates[element] = PointF.Create(deltaX, deltaY);

                transforms.Add(trans);
            }
            else
            {
                var t = new SvgTranslate(previousTranslate.X + deltaX, previousTranslate.Y + deltaY);
                _translates[element] = PointF.Create(t.X, t.Y);

                transforms[index] = t; // we MUST explicitly set the transform so the "OnTransformChanged" event is fired!
            }
        }
    }
}
