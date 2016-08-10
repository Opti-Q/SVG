using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Core.Utils;
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
        private bool _isActive;
        private SvgDrawingCanvas _canvas;
        private Brush BlueBrush => _brush ?? (_brush = Engine.Factory.CreateSolidBrush(Engine.Factory.CreateColorFromArgb(255, 80, 210, 210)));
        private Pen BluePen => _pen ?? (_pen = Engine.Factory.CreatePen(BlueBrush, 5));

        private IEnumerable<SvgMarker> Markers { get; set; }

        public string LineStyleIconName { get; set; } = "ic_line_style_white_48dp.png";

        public string[] MarkerIds
        {
            get
            {
                object markerIds;
                Properties.TryGetValue("markerids", out markerIds);
                if (markerIds == null) markerIds = Enumerable.Empty<string>();
                return (string[])markerIds;
            }
        }

        public string[] LineStyles
        {
            get
            {
                object lineStyles;
                Properties.TryGetValue("linestyles", out lineStyles);
                if (lineStyles == null) lineStyles = Enumerable.Empty<string>();
                return (string[])lineStyles;
            }
        }

        public override bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                if (_isActive)
                {
                    _currentLine = _canvas.SelectedElements.OfType<SvgLine>().FirstOrDefault();
                    _canvas.SelectedElements.Clear();
                    if (_currentLine == null) return;
                    _canvas.SelectedElements.Add(_currentLine);
                    _canvas.FireInvalidateCanvas();
                    return;
                }
                if (_currentLine == null) return;
                _canvas.SelectedElements.Remove(_currentLine);
                _currentLine = null;
                _canvas.FireInvalidateCanvas();
            }
        }

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            InitializeDefinitions(newDocument);
        }

        private void InitializeDefinitions(SvgDocument document)
        {
            var definitions = document.Children.OfType<SvgDefinitionList>().FirstOrDefault();
            if (definitions == null)
            {
                definitions = new SvgDefinitionList();
                document.Children.Add(definitions);
            }


            foreach (var marker in Markers)
            {
                if (document.GetElementById(marker.ID) != null) continue;

                definitions.Children.Add(marker);
            }
        }

        public string SelectedMarkerId { get; set; }

        public LineTool(string properties) : base("Line", properties)
        {
            IconName = "ic_mode_edit_white_48dp.png";
            ToolUsage = ToolUsage.Explicit;
            ToolType = ToolType.Create;

            var markers = new List<SvgMarker>();
            var marker = new SvgMarker { ID = "arrowMarker" };
            marker.Children.Add(new SvgPath
            {
                PathData = new SvgPathSegmentList(new SvgPathSegment[]
                {
                    new SvgLineSegment(PointF.Create(-8, -4), PointF.Create(-8, 4)),
                    new SvgLineSegment(PointF.Create(-8, 4), PointF.Create(0, 0)),
                    new SvgClosePathSegment()
                })
            });
            markers.Add(marker);
            marker = new SvgMarker { ID = "ellipseMarker" };
            marker.Children.Add(new SvgEllipse
            {
                RadiusX = 8,
                RadiusY = 8
            });
            markers.Add(marker);

            Markers = markers;
        }

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            _canvas = ws;

            IsActive = false;

            SelectedMarkerId = MarkerIds.FirstOrDefault();

            Commands = new List<IToolCommand>
            {
                new ChangeLineStyleCommand(ws, this, "Line style")
            };

            return Task.FromResult(true);
        }

        public override Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            if (!IsActive)
            {
                return Task.FromResult(true);
            }

            var p = @event as PointerEvent;
            if (p?.PointerCount == 1 && (p.EventType == EventType.PointerUp || p.EventType == EventType.Cancel))
            {
                if (_currentLine != null)
                {
                    ws.SelectedElements.Remove(_currentLine);
                    _currentLine = null;
                }
                else
                {
                    _currentLine =
                        ws.GetElementsUnder<SvgLine>(ws.GetPointerRectangle(p.Pointer1Position),
                            SelectionType.Intersect).FirstOrDefault();
                    if (_currentLine != null) ws.SelectedElements.Add(_currentLine);
                }

                ws.FireToolCommandsChanged();
                ws.FireInvalidateCanvas();
            }

            if (p?.EventType == EventType.PointerDown)
                _multiplePointersRegistered = p.PointerCount != 1;

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
                    var relativeStartX = CanvasCalculationUtil.GetCanvasDimension(ws.RelativeTranslate.X, e.Pointer1Down.X, z);
                    var relativeStartY = CanvasCalculationUtil.GetCanvasDimension(ws.RelativeTranslate.Y, e.Pointer1Down.Y, z);
                    var relativeEndX = CanvasCalculationUtil.GetCanvasDimension(ws.RelativeTranslate.X, e.Pointer1Position.X, z);
                    var relativeEndY = CanvasCalculationUtil.GetCanvasDimension(ws.RelativeTranslate.Y, e.Pointer1Position.Y, z);

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
                            MarkerEnd = new Uri($"#{SelectedMarkerId}", UriKind.Relative)
                        };

                        ws.Document.Children.Add(_currentLine);
                    }

                    var offsetX = 0.0f;
                    var offsetY = 0.0f;
                    foreach (var transform in _currentLine.Transforms)
                    {
                        offsetX += transform.Matrix.OffsetX;
                        offsetY += transform.Matrix.OffsetY;
                    }

                    _currentLine.EndX = new SvgUnit(SvgUnitType.Pixel, relativeEndX - offsetX);
                    _currentLine.EndY = new SvgUnit(SvgUnitType.Pixel, relativeEndY - offsetY);

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
                var offsetX = 0.0f;
                var offsetY = 0.0f;
                foreach (var transform in _currentLine.Transforms)
                {
                    offsetX += transform.Matrix.OffsetX;
                    offsetY += transform.Matrix.OffsetY;
                }
                renderer.DrawCircle(offsetX + _currentLine.EndX - radius, offsetY + _currentLine.EndY - radius, radius, BluePen);

                renderer.Graphics.Restore();
            }
        }

        /// <summary>
        /// This command changes the color of selected items, or the global selected color, if no items are selected.
        /// </summary>
        private class ChangeLineStyleCommand : ToolCommand
        {
            private readonly SvgDrawingCanvas _canvas;

            public ChangeLineStyleCommand(SvgDrawingCanvas canvas, LineTool tool, string name)
                : base(tool, name, o => { }, iconName: tool.LineStyleIconName, sortFunc: tc => 500)
            {
                _canvas = canvas;
            }

            public override async void Execute(object parameter)
            {
                var t = (LineTool)Tool;

                // TODO: bring up line style chooser

                if (_canvas.SelectedElements.Any())
                {
                    // change the color of all selected items
                    foreach (var selectedElement in _canvas.SelectedElements)
                    {
                        // TODO: change style for selected element
                    }
                    _canvas.FireInvalidateCanvas();
                    // don't change the global color when items are selected
                    return;
                }

                // TODO: change global line style
            }
        }
    }
}
