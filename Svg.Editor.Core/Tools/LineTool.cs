using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Interfaces;
using Svg.Pathing;

namespace Svg.Core.Tools
{
    public interface ILineOptionsInputService
    {
        Task<int[]> GetUserInput(string title, IEnumerable<string> markerStartOptions, int markerStartSelected, IEnumerable<string> lineStyleOptions, int dashSelected, IEnumerable<string> markerEndOptions, int markerEndSelected);
    }

    public class LineTool : ToolBase
    {
        private const double MIN_MOVED_DISTANCE = 30.0;

        private static ILineOptionsInputService LineOptionsInputServiceProxy => Engine.Resolve<ILineOptionsInputService>();

        private double _movedDistance;
        private SvgLine _currentLine;
        private bool _multiplePointersRegistered;
        private Brush _brush;
        private Pen _pen;
        private bool _isActive;
        private SvgDrawingCanvas _canvas;
        private MovementType _movementType;
        private Brush BlueBrush => _brush ?? (_brush = Engine.Factory.CreateSolidBrush(Engine.Factory.CreateColorFromArgb(255, 80, 210, 210)));
        private Pen BluePen => _pen ?? (_pen = Engine.Factory.CreatePen(BlueBrush, 5));

        private IEnumerable<SvgMarker> Markers { get; set; }

        private static Uri CreateUriFromId(string markerEndId, string exception = "none")
        {
            return markerEndId != exception ? new Uri($"#{markerEndId}", UriKind.Relative) : null;
        }

        public string LineStyleIconName { get; set; } = "ic_line_style_white_48dp.png";

        public string[] MarkerStartIds
        {
            get
            {
                object markerIds;
                if (!Properties.TryGetValue("markerstartids", out markerIds))
                    markerIds = Enumerable.Empty<string>();
                return (string[])markerIds;
            }
        }

        public string[] MarkerStartNames
        {
            get
            {
                object markerNames;
                if (!Properties.TryGetValue("markerstartnames", out markerNames))
                    markerNames = Enumerable.Empty<string>();
                return (string[])markerNames;
            }
        }

        public string[] MarkerEndIds
        {
            get
            {
                object markerIds;
                if (!Properties.TryGetValue("markerendids", out markerIds))
                    markerIds = Enumerable.Empty<string>();
                return (string[])markerIds;
            }
        }

        public string[] MarkerEndNames
        {
            get
            {
                object markerNames;
                if (!Properties.TryGetValue("markerendnames", out markerNames))
                    markerNames = Enumerable.Empty<string>();
                return (string[])markerNames;
            }
        }

        public string[] LineStyles
        {
            get
            {
                object lineStyles;
                if (!Properties.TryGetValue("linestyles", out lineStyles))
                    lineStyles = Enumerable.Empty<string>();
                return (string[])lineStyles;
            }
        }

        public string[] LineStyleNames
        {
            get
            {
                object lineStyleNames;
                if (!Properties.TryGetValue("linestylenames", out lineStyleNames))
                    lineStyleNames = Enumerable.Empty<string>();
                return (string[])lineStyleNames;
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
                    // if tool was activated, reduce selection to a single line and set it as current line
                    _currentLine = _canvas.SelectedElements.OfType<SvgLine>().FirstOrDefault();
                    _canvas.SelectedElements.Clear();
                    if (_currentLine == null) return;
                    _canvas.SelectedElements.Add(_currentLine);
                    _canvas.FireInvalidateCanvas();
                    return;
                }
                // if tool was deactivated, reset current line
                if (_currentLine == null) return;
                _canvas.SelectedElements.Remove(_currentLine);
                _currentLine = null;
                _canvas.FireInvalidateCanvas();
            }
        }

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            if (oldDocument != null) UnWatchDocument(oldDocument);
            WatchDocument(newDocument);
            InitializeDefinitions(newDocument);
        }

        private void UnWatchDocument(SvgDocument svgDocument)
        {
            svgDocument.ChildRemoved -= SvgDocumentOnChildRemoved;
        }

        private void WatchDocument(SvgDocument svgDocument)
        {
            svgDocument.ChildRemoved -= SvgDocumentOnChildRemoved;
            svgDocument.ChildRemoved += SvgDocumentOnChildRemoved;
        }

        private void SvgDocumentOnChildRemoved(object sender, ChildRemovedEventArgs args)
        {
            if (IsActive && args.RemovedChild == _currentLine)
            {
                _currentLine = null;
                _canvas.FireInvalidateCanvas();
            }
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

        public string SelectedMarkerStartId { get; set; }

        public string SelectedMarkerEndId { get; set; }

        public string SelectedLineStyle { get; set; }

        private SvgUnitCollection StrokeDashArray { get; set; } = new SvgUnitCollection()
                            {
                                new SvgUnit(SvgUnitType.Pixel, 10),
                                new SvgUnit(SvgUnitType.Pixel, 10)
                            };

        public LineTool(string properties) : base("Line", properties)
        {
            IconName = "ic_mode_edit_white_48dp.png";
            ToolUsage = ToolUsage.Explicit;

            var markers = new List<SvgMarker>();
            var marker = new SvgMarker { ID = "arrowStart", Orient = new SvgOrient() { IsAuto = true } };
            marker.Children.Add(new SvgPath
            {
                PathData = new SvgPathSegmentList(new SvgPathSegment[]
                {
                    new SvgLineSegment(PointF.Create(0, -2.0f), PointF.Create(0, 2f)),
                    new SvgLineSegment(PointF.Create(0, 2.0f), PointF.Create(-4.0f, 0)),
                    new SvgClosePathSegment()
                })
            });
            markers.Add(marker);
            marker = new SvgMarker { ID = "arrowEnd", Orient = new SvgOrient() { IsAuto = true } };
            marker.Children.Add(new SvgPath
            {
                PathData = new SvgPathSegmentList(new SvgPathSegment[]
                {
                    new SvgLineSegment(PointF.Create(0, -2.0f), PointF.Create(0, 2.0f)),
                    new SvgLineSegment(PointF.Create(0, 2.0f), PointF.Create(4.0f, 0)),
                    new SvgClosePathSegment()
                })
            });
            markers.Add(marker);
            marker = new SvgMarker { ID = "circle", Orient = new SvgOrient() { IsAuto = true } };
            marker.Children.Add(new SvgEllipse
            {
                RadiusX = 1.5f,
                RadiusY = 1.5f
            });
            markers.Add(marker);

            Markers = markers;
        }

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            _canvas = ws;

            IsActive = false;

            SelectedMarkerStartId = MarkerStartIds.FirstOrDefault();
            SelectedMarkerEndId = MarkerEndIds.FirstOrDefault();
            SelectedLineStyle = LineStyles.FirstOrDefault();

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
            {
                _multiplePointersRegistered = p.PointerCount != 1;

                if (_currentLine != null)
                {
                    _movementType = Math.Abs(p.Pointer1Position.X - _currentLine.EndX) <= MIN_MOVED_DISTANCE &&
                                 Math.Abs(p.Pointer1Position.Y - _currentLine.EndY) <= MIN_MOVED_DISTANCE ? MovementType.End :
                                 Math.Abs(p.Pointer1Position.X - _currentLine.StartX) <= MIN_MOVED_DISTANCE &&
                                 Math.Abs(p.Pointer1Position.Y - _currentLine.StartY) <= MIN_MOVED_DISTANCE ? MovementType.Start :
                                 _currentLine.GetBoundingBox().Contains(ws.ScreenToCanvas(p.Pointer1Position)) ? MovementType.StartEnd : MovementType.None;
                }
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
                    var relativeStart = ws.ScreenToCanvas(e.Pointer1Down);
                    var relativeEnd = ws.ScreenToCanvas(e.Pointer1Position);

                    if (_currentLine == null)
                    {

                        _currentLine = new SvgLine
                        {
                            Stroke = new SvgColourServer(Engine.Factory.CreateColorFromArgb(255, 0, 0, 0)),
                            Fill = SvgPaintServer.None,
                            StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 3),
                            StartX = new SvgUnit(SvgUnitType.Pixel, relativeStart.X),
                            StartY = new SvgUnit(SvgUnitType.Pixel, relativeStart.Y),
                            EndX = new SvgUnit(SvgUnitType.Pixel, relativeEnd.X),
                            EndY = new SvgUnit(SvgUnitType.Pixel, relativeEnd.Y),
                            MarkerStart = CreateUriFromId(SelectedMarkerStartId),
                            MarkerEnd = CreateUriFromId(SelectedMarkerEndId)
                        };

                        _movementType = MovementType.End;

                        if (SelectedLineStyle == "dashed")
                        {
                            _currentLine.StrokeDashArray = StrokeDashArray.Clone();
                        }

                        ws.Document.Children.Add(_currentLine);
                    }

                    switch (_movementType)
                    {
                        case MovementType.End:
                            _currentLine.EndX = new SvgUnit(SvgUnitType.Pixel, relativeEnd.X);
                            _currentLine.EndY = new SvgUnit(SvgUnitType.Pixel, relativeEnd.Y);
                            break;
                        case MovementType.Start:
                            _currentLine.StartX = new SvgUnit(SvgUnitType.Pixel, relativeEnd.X);
                            _currentLine.StartY = new SvgUnit(SvgUnitType.Pixel, relativeEnd.Y);
                            break;
                        case MovementType.StartEnd:
                            // TODO: move both start and end points
                            break;
                    }

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
                renderer.DrawCircle(_currentLine.StartX - (radius >> 1), _currentLine.StartY - (radius >> 1), radius, BluePen);
                renderer.DrawCircle(_currentLine.EndX - (radius >> 1), _currentLine.EndY - (radius >> 1), radius, BluePen);

                renderer.Graphics.Restore();
            }
        }

        /// <summary>
        /// This command changes the line style of selected items, or the global line style, if no items are selected.
        /// </summary>
        private class ChangeLineStyleCommand : ToolCommand
        {
            private readonly SvgDrawingCanvas _canvas;

            public ChangeLineStyleCommand(SvgDrawingCanvas canvas, LineTool tool, string name)
                : base(tool, name, o => { }, iconName: tool.LineStyleIconName, sortFunc: tc => 500)
            {
                _canvas = canvas;
            }

            private new LineTool Tool => (LineTool)base.Tool;

            public override async void Execute(object parameter)
            {
                var t = Tool;

                var selectedLines = _canvas.SelectedElements.OfType<SvgLine>().ToArray();

                var markerStartId = selectedLines.Any()
                    ? selectedLines.All(x => selectedLines.First().MarkerStart == x.MarkerStart)
                        ? selectedLines.First().MarkerStart?.OriginalString.Substring(1) ?? "none"
                        : "none"
                    : t.SelectedMarkerStartId;
                var lineStyle = selectedLines.Any()
                    ? selectedLines.All(x => selectedLines.First().StrokeDashArray?.ToString() == x.StrokeDashArray?.ToString())
                        ? string.IsNullOrEmpty(selectedLines.First().StrokeDashArray?.ToString()) ? "normal" : "dashed"
                        : "normal"
                    : t.SelectedLineStyle;
                var markerEndId = selectedLines.Any()
                    ? selectedLines.All(x => selectedLines.First().MarkerEnd == x.MarkerEnd)
                        ? selectedLines.First().MarkerEnd?.OriginalString.Substring(1) ?? "none"
                        : "none"
                    : t.SelectedMarkerEndId;

                int markerStartIndex;
                int lineStyleIndex;
                int markerEndIndex;

                markerStartIndex = Array.IndexOf(t.MarkerStartIds, markerStartId);
                lineStyleIndex = Array.IndexOf(t.LineStyles, lineStyle);
                markerEndIndex = Array.IndexOf(t.MarkerEndIds, markerEndId);

                var selectedOptions = await LineOptionsInputServiceProxy.GetUserInput("Choose line options",
                    t.MarkerStartNames, markerStartIndex,
                    t.LineStyleNames, lineStyleIndex,
                    t.MarkerEndNames, markerEndIndex);

                var selectedMarkerStartId = t.MarkerStartIds[selectedOptions[0]];
                var selectedLineStyle = t.LineStyles[selectedOptions[1]];
                var selectedMarkerEndId = t.MarkerEndIds[selectedOptions[2]];

                if (selectedLines.Any())
                {
                    // change the line style of all selected items
                    foreach (var selectedLine in selectedLines)
                    {
                        selectedLine.MarkerStart = CreateUriFromId(selectedMarkerStartId);
                        selectedLine.MarkerEnd = CreateUriFromId(selectedMarkerEndId);
                        if (selectedLineStyle == "dashed")
                        {
                            selectedLine.StrokeDashArray = Tool.StrokeDashArray.Clone();
                        }
                        else
                        {
                            selectedLine.StrokeDashArray = null;
                        }
                    }
                    _canvas.FireInvalidateCanvas();
                    // don't change the global line style when items are selected
                    return;
                }

                t.SelectedMarkerStartId = selectedMarkerStartId;
                t.SelectedMarkerEndId = selectedMarkerEndId;
                t.SelectedLineStyle = selectedLineStyle;
            }

            public override bool CanExecute(object parameter)
            {
                return Tool.IsActive;
            }
        }

        private enum MovementType { None, Start, End, StartEnd }
    }
}
