using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Core.Utils;
using Svg.Droid.SampleEditor.Core;
using Svg.Interfaces;
using Svg.Pathing;

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
        private bool _multiplePointersRegistered;
        private Brush _brush;
        private Pen _pen;
        private Brush BlueBrush => _brush ?? (_brush = Svg.Engine.Factory.CreateSolidBrush(Svg.Engine.Factory.CreateColorFromArgb(255, 80, 210, 210)));
        private Pen BluePen => _pen ?? (_pen = Svg.Engine.Factory.CreatePen(BlueBrush, 5));

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
            if (!IsActive)
                return Task.FromResult(true);

            var p = @event as PointerEvent;
            if (p?.PointerCount == 1 && (p.EventType == EventType.PointerUp || p.EventType == EventType.Cancel))
            {
                //ws.SelectedElements.Clear();
                //ws.SelectedElements.Add(_currentLine);
                //ws.ActiveTool = ws.Tools.OfType<SelectionTool>().Single();

                if (_currentLine != null)
                    _currentLine = null;
                else
                    _currentLine =
                        ws.GetElementsUnder<SvgLine>(ws.GetPointerRectangle(p.Pointer1Position),
                            SelectionType.Intersect).FirstOrDefault();

                ws.FireInvalidateCanvas();

                if (p.EventType == EventType.PointerDown)
                    _multiplePointersRegistered = p.PointerCount != 1;
            }

            if (_multiplePointersRegistered)
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

                        _currentLine = new SvgLine
                        {
                            Stroke = new SvgColourServer(Engine.Factory.CreateColorFromArgb(255, 0, 0, 0)),
                            Fill = new SvgColourServer(Engine.Factory.CreateColorFromArgb(255, 0, 0, 0)),
                            StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 3),
                            StartX = new SvgUnit(SvgUnitType.Pixel, relativeStartX),
                            StartY = new SvgUnit(SvgUnitType.Pixel, relativeStartY),
                            EndX = new SvgUnit(SvgUnitType.Pixel, relativeEndX),
                            EndY = new SvgUnit(SvgUnitType.Pixel, relativeEndY),
                            MarkerEnd = new Uri("#marker", UriKind.Relative)
                        };

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
                            var markerPath = new SvgPath
                            {
                                PathData = new SvgPathSegmentList(new SvgPathSegment[]
                                {
                                    new SvgLineSegment(PointF.Create(0, -4), PointF.Create(2, 4)),
                                    new SvgLineSegment(PointF.Create(0, 4), PointF.Create(8, 0)),
                                    new SvgLineSegment(PointF.Create(8, 0), PointF.Create(0, -4)),
                                    new SvgClosePathSegment()
                                })
                            };
                            marker.Children.Add(markerPath);
                        }

                        ws.Document.Children.Add(_currentLine);
                    }

                    _currentLine.EndX = new SvgUnit(SvgUnitType.Pixel, relativeEndX);
                    _currentLine.EndY = new SvgUnit(SvgUnitType.Pixel, relativeEndY);

                    ws.FireInvalidateCanvas();
                }
            }

            return Task.FromResult(true);
        }

        public override async Task OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            await base.OnDraw(renderer, ws);

            if (_currentLine != null)
            {
                renderer.Graphics.Save();

                const int radius = 16;
                renderer.DrawCircle(_currentLine.EndX - radius, _currentLine.EndY - radius, radius, BluePen);

                renderer.Graphics.Restore();
            }
        }
    }
}
