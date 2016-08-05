using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Utils;
using Svg.Interfaces;

namespace Svg.Core.Tools
{
    public interface ILineOptionsInputService
    {
        Task<int> GetUserInput(string title, IEnumerable<string> options);
    }

    public class LineTool : ToolBase
    {
        private const double MIN_MOVED_DISTANCE = 30.0;

        private double _movedDistance;
        private SvgLine _currentLine;

        public LineTool() : base("Line")
        {
            IconName = "ic_mode_edit_white_48dp.png";
            ToolUsage = ToolUsage.Explicit;
        }

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            IsActive = false;

            return Task.FromResult(true);
        }

        public override Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {

            var p = @event as PointerEvent;
            if (p != null)
            {
                if (p.EventType == EventType.PointerUp || p.EventType == EventType.Cancel)
                {
                    _currentLine = null;
                }
            }

            if (!IsActive)
                return Task.FromResult(true);

            var e = @event as MoveEvent;
            if (e != null)
            {
                var startX = e.Pointer1Down.X;
                var startY = e.Pointer1Down.Y;
                var endX = e.Pointer1Position.X;
                var endY = e.Pointer1Position.Y;

                if (startX > endX)
                {
                    var t = startX;
                    startX = endX;
                    endX = t;
                }
                if (startY > endY)
                {
                    var t = startY;
                    startY = endY;
                    endY = t;
                }
                var rect = RectangleF.Create(startX, startY, endX - startX, endY - startY);

                // drawing only counts if length is not too small
                _movedDistance = Math.Sqrt(Math.Pow(rect.Width, 2) + Math.Pow(rect.Height, 2));

                if (_movedDistance >= MIN_MOVED_DISTANCE)
                {

                    var z = ws.ZoomFactor;
                    var relativeStartX = CanvasCalculationUtil.GetRelativeDimension(ws.RelativeTranslate.X, e.Pointer1Down.X, z);
                    var relativeStartY = CanvasCalculationUtil.GetRelativeDimension(ws.RelativeTranslate.Y, e.Pointer1Down.Y, z);
                    var relativeEndX = CanvasCalculationUtil.GetRelativeDimension(ws.RelativeTranslate.X, e.Pointer1Position.X, z);
                    var relativeEndY = CanvasCalculationUtil.GetRelativeDimension(ws.RelativeTranslate.Y, e.Pointer1Position.Y, z);

                    if (_currentLine == null)
                    {

                        if (ws.Document.IdManager.GetElementById("marker") == null)
                        {
                            var definitions = ws.Document.Children.OfType<SvgDefinitionList>().FirstOrDefault();
                            if (definitions == null)
                            {
                                definitions = new SvgDefinitionList();
                                ws.Document.Children.Add(definitions);
                            }
                            var marker = new SvgMarker { ID = "marker" };
                            definitions.Children.Add(marker);
                            var markerRect = new SvgRectangle();
                            var bounds = RectangleF.Create(0, 0, 48, 48);
                            markerRect.SetRectangle(bounds);
                            marker.Children.Add(markerRect);
                        }

                        _currentLine = new SvgLine
                        {
                            Stroke = new SvgColourServer(Engine.Factory.CreateColorFromArgb(255, 0, 0, 0)),
                            Fill = new SvgColourServer(Engine.Factory.CreateColorFromArgb(255, 0, 0, 0)),
                            StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 5),
                            StartX = new SvgUnit(SvgUnitType.Pixel, relativeStartX),
                            StartY = new SvgUnit(SvgUnitType.Pixel, relativeStartY),
                            EndX = new SvgUnit(SvgUnitType.Pixel, relativeEndX),
                            EndY = new SvgUnit(SvgUnitType.Pixel, relativeEndY),
                            MarkerEnd = new Uri("#marker", UriKind.Relative)
                        };

                        ws.Document.Children.Add(_currentLine);
                    }

                    _currentLine.EndX = new SvgUnit(SvgUnitType.Pixel, relativeEndX);
                    _currentLine.EndY = new SvgUnit(SvgUnitType.Pixel, relativeEndY);

                    ws.FireInvalidateCanvas();
                }
            }

            return Task.FromResult(true);
        }
    }
}
