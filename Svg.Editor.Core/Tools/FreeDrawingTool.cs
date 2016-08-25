﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Interfaces;
using Svg.Pathing;

namespace Svg.Core.Tools
{
    public class FreeDrawingTool : ToolBase
    {
        private const double MinMovedDistance = 0.0;
        private const double MinQuadMovedDistance = 120.0;

        //private static ILineOptionsInputService LineOptionsInputServiceProxy => Engine.Resolve<ILineOptionsInputService>();

        private double _movedDistance;
        private SvgPath _currentLine;
        private bool _multiplePointersRegistered;
        private bool _isActive;
        private SvgDrawingCanvas _canvas;
        private bool _validMove;
        private PointF _lastPointerPosition;

        private IEnumerable<SvgMarker> Markers { get; set; }

        //private static Uri CreateUriFromId(string markerEndId, string exception = "none")
        //{
        //    return markerEndId != exception ? new Uri($"#{markerEndId}", UriKind.Relative) : null;
        //}

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
                    _currentLine = _canvas.SelectedElements.OfType<SvgPath>().FirstOrDefault();
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

        //private SvgUnitCollection StrokeDashArray { get; set; } = new SvgUnitCollection
        //{
        //                        new SvgUnit(SvgUnitType.Pixel, 10),
        //                        new SvgUnit(SvgUnitType.Pixel, 10)
        //                    };

        public FreeDrawingTool(string properties) : base("Free draw", properties)
        {
            IconName = "ic_brush_white_48dp.png";
            ToolUsage = ToolUsage.Explicit;

            var markers = new List<SvgMarker>();
            var marker = new SvgMarker { ID = "arrowStart", Orient = new SvgOrient { IsAuto = true } };
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
            marker = new SvgMarker { ID = "arrowEnd", Orient = new SvgOrient { IsAuto = true } };
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
            marker = new SvgMarker { ID = "circle", Orient = new SvgOrient { IsAuto = true } };
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

            //Commands = new List<IToolCommand>
            //{
            //    new ChangeLineStyleCommand(ws, this, "Line style")
            //};

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
                _validMove = false;
                if (_currentLine != null)
                {
                    ws.SelectedElements.Remove(_currentLine);
                    _currentLine = null;
                }
                else
                {
                    _currentLine =
                        ws.GetElementsUnder<SvgPath>(ws.GetPointerRectangle(p.Pointer1Position),
                            SelectionType.Intersect).FirstOrDefault();
                    if (_currentLine != null) ws.SelectedElements.Add(_currentLine);
                }

                ws.FireToolCommandsChanged();
                ws.FireInvalidateCanvas();
            }

            if (p?.EventType == EventType.PointerDown)
            {
                _multiplePointersRegistered = p.PointerCount != 1;
            }

            if (_multiplePointersRegistered)
                return Task.FromResult(true);

            var e = @event as MoveEvent;
            if (e != null)
            {
                var startX = _lastPointerPosition?.X ?? e.Pointer1Down.X;
                var startY = _lastPointerPosition?.Y ?? e.Pointer1Down.Y;
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
                _movedDistance = Math.Sqrt(Math.Pow(rect.Width, 2) + Math.Pow(rect.Height, 2)) / ws.ZoomFactor;

                if (_movedDistance >= MinMovedDistance)
                {
                    var relativeStart = ws.ScreenToCanvas(e.Pointer1Down);
                    var relativePointerPosition = ws.ScreenToCanvas(e.Pointer1Position);

                    if (_currentLine == null)
                    {

                        _currentLine = new SvgPath
                        {
                            Stroke = new SvgColourServer(Engine.Factory.CreateColorFromArgb(255, 0, 0, 0)),
                            Fill = SvgPaintServer.None,
                            StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 5),
                            PathData = new SvgPathSegmentList(new List<SvgPathSegment> { new SvgMoveToSegment(relativeStart) }),
                            StrokeLineCap = SvgStrokeLineCap.Round,
                            StrokeLineJoin = SvgStrokeLineJoin.Round
                            //MarkerStart = CreateUriFromId(SelectedMarkerStartId),
                            //MarkerEnd = CreateUriFromId(SelectedMarkerEndId)
                        };

                        _validMove = true;

                        //if (SelectedLineStyle == "dashed")
                        //{
                        //    _currentLine.StrokeDashArray = StrokeDashArray.Clone();
                        //}

                        ws.Document.Children.Add(_currentLine);
                    }

                    if (_validMove)
                    {
                        // Calculate the control point
                        var lastSegment = _currentLine.PathData.Last;
                        var lastEndPoint = lastSegment.End;
                        //if (_movedDistance >= MIN_QUAD_MOVED_DISTANCE)
                        //{
                            // quadratic bezier
                            //var lastControlPoint = (lastSegment as SvgQuadraticCurveSegment)?.ControlPoint ??
                            //                       lastSegment.Start;
                            var nextControlPoint = lastSegment.End;
                            var nextEndPoint = (lastEndPoint + relativePointerPosition)/2;
                            _currentLine.PathData.Add(new SvgQuadraticCurveSegment(lastEndPoint, nextControlPoint, nextEndPoint));
                        // cubic bezier
                        //var firstControlPoint = (lastEndPoint + relativePointerPosition) / 2;
                        //var secondControlPoint = relativePointerPosition * 2 - firstControlPoint;
                        //_currentLine.PathData.Add(new SvgCubicCurveSegment(lastEndPoint, firstControlPoint, secondControlPoint, relativePointerPosition));
                        //}
                        //else
                        //{
                        //    // straight line
                        //    _currentLine.PathData.Add(new SvgLineSegment(lastEndPoint, relativePointerPosition));
                        //}
                    }

                    _lastPointerPosition = p.Pointer1Position;

                    ws.FireInvalidateCanvas();
                }
            }

            return Task.FromResult(true);
        }

        ///// <summary>
        ///// This command changes the line style of selected items, or the global line style, if no items are selected.
        ///// </summary>
        //private class ChangeLineStyleCommand : ToolCommand
        //{
        //    private readonly SvgDrawingCanvas _canvas;

        //    public ChangeLineStyleCommand(SvgDrawingCanvas canvas, LineTool tool, string name)
        //        : base(tool, name, o => { }, iconName: tool.LineStyleIconName, sortFunc: tc => 500)
        //    {
        //        _canvas = canvas;
        //    }

        //    private new LineTool Tool => (LineTool)base.Tool;

        //    public override async void Execute(object parameter)
        //    {
        //        var t = Tool;

        //        var selectedLines = _canvas.SelectedElements.OfType<SvgLine>().ToArray();

        //        var markerStartId = selectedLines.Any()
        //            ? selectedLines.All(x => selectedLines.First().MarkerStart == x.MarkerStart)
        //                ? selectedLines.First().MarkerStart?.OriginalString.Substring(1) ?? "none"
        //                : "none"
        //            : t.SelectedMarkerStartId;
        //        var lineStyle = selectedLines.Any()
        //            ? selectedLines.All(x => selectedLines.First().StrokeDashArray?.ToString() == x.StrokeDashArray?.ToString())
        //                ? string.IsNullOrEmpty(selectedLines.First().StrokeDashArray?.ToString()) ? "normal" : "dashed"
        //                : "normal"
        //            : t.SelectedLineStyle;
        //        var markerEndId = selectedLines.Any()
        //            ? selectedLines.All(x => selectedLines.First().MarkerEnd == x.MarkerEnd)
        //                ? selectedLines.First().MarkerEnd?.OriginalString.Substring(1) ?? "none"
        //                : "none"
        //            : t.SelectedMarkerEndId;

        //        int markerStartIndex;
        //        int lineStyleIndex;
        //        int markerEndIndex;

        //        markerStartIndex = Array.IndexOf(t.MarkerStartIds, markerStartId);
        //        lineStyleIndex = Array.IndexOf(t.LineStyles, lineStyle);
        //        markerEndIndex = Array.IndexOf(t.MarkerEndIds, markerEndId);

        //        var selectedOptions = await LineOptionsInputServiceProxy.GetUserInput("Choose line options",
        //            t.MarkerStartNames, markerStartIndex,
        //            t.LineStyleNames, lineStyleIndex,
        //            t.MarkerEndNames, markerEndIndex);

        //        var selectedMarkerStartId = t.MarkerStartIds[selectedOptions[0]];
        //        var selectedLineStyle = t.LineStyles[selectedOptions[1]];
        //        var selectedMarkerEndId = t.MarkerEndIds[selectedOptions[2]];

        //        if (selectedLines.Any())
        //        {
        //            // change the line style of all selected items
        //            foreach (var selectedLine in selectedLines)
        //            {
        //                selectedLine.MarkerStart = CreateUriFromId(selectedMarkerStartId);
        //                selectedLine.MarkerEnd = CreateUriFromId(selectedMarkerEndId);
        //                if (selectedLineStyle == "dashed")
        //                {
        //                    selectedLine.StrokeDashArray = Tool.StrokeDashArray.Clone();
        //                }
        //                else
        //                {
        //                    selectedLine.StrokeDashArray = null;
        //                }
        //            }
        //            _canvas.FireInvalidateCanvas();
        //            // don't change the global line style when items are selected
        //            return;
        //        }

        //        t.SelectedMarkerStartId = selectedMarkerStartId;
        //        t.SelectedMarkerEndId = selectedMarkerEndId;
        //        t.SelectedLineStyle = selectedLineStyle;
        //    }

        //    public override bool CanExecute(object parameter)
        //    {
        //        return Tool.IsActive;
        //    }
        //}

        //private enum MovementType { None, Start, End, StartEnd }
    }
}
