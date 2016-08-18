using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Interfaces;
using Svg.Transforms;

namespace Svg.Core.Tools
{
    public class GridTool : ToolBase
    {
        private const double Gamma = 90f;

        private float StepSizeY
        {
            get
            {
                object stepSizeY;
                if (!Properties.TryGetValue("stepsizey", out stepSizeY))
                    stepSizeY = 20.0f;
                return Convert.ToSingle(stepSizeY);
            }
        }

        private float StepSizeX { get; }

        private double Alpha
        {
            get
            {
                object alpha;
                if (!Properties.TryGetValue("alpha", out alpha))
                    alpha = 30.0f;
                return Convert.ToSingle(alpha);
            }
        }

        private Pen _pen;

        private Pen _pen2;
        private Brush _brush;
        private Brush _brush2;

        private bool _isSnappingInProgress;
        private bool _areElementsMoved;
        private PointF _generalTranslation;


        public GridTool(string properties)
            : base("Grid", properties)
        {
            // using triangle calculation to determine the x and y steps based on stepsize (y) and angle (alpha)
            // http://www.arndt-bruenner.de/mathe/scripts/Dreiecksberechnung.htm
            /*
                                                  XXX+
                                               XXX   |
                                             XX      |
                                          XXX  B = ? |
                                        XXX          |
                                      XXX            |
                           c = ?   XXX               |
                                 XXX                 |
                               XXX                   |  a = 20
                             XXX                     |
                           XXX                       |
                         XXX                         |
                       XXX                           |
                     XXX    A = 27.3          G = 90 |
                    XX-------------------------------+
                              b = ?
             * */
            var a = StepSizeY / 2;
            var beta = 180f - (Alpha + Gamma);
            var b = a * SinDegree(beta) / SinDegree(Alpha);
            //var c = a * SinDegree(Gamma) / SinDegree(Alpha);
            StepSizeX = (float)b * 2;
        }

        public string IconGridOn { get; set; } = "ic_grid_on_white_48dp.png";
        public string IconGridOff { get; set; } = "ic_grid_off_white_48dp.png";

        public bool IsSnappingEnabled
        {
            get
            {
                object isSnappingEnabled;
                if (!Properties.TryGetValue("issnappingenabled", out isSnappingEnabled))
                    isSnappingEnabled = true;
                return (bool)isSnappingEnabled;
            }
            set { Properties["issnappingenabled"] = value; }
        }

        public bool IsVisible { get; set; } = true;
        private Brush Brush => _brush ?? (_brush = Engine.Factory.CreateSolidBrush(Engine.Factory.CreateColorFromArgb(255, 210, 210, 210)));
        private Brush Brush2 => _brush2 ?? (_brush2 = Engine.Factory.CreateSolidBrush(Engine.Factory.CreateColorFromArgb(180, 0, 0, 0)));
        private Pen Pen => _pen ?? (_pen = Engine.Factory.CreatePen(Brush, 1));
        private Pen Pen2 => _pen2 ?? (_pen2 = Engine.Factory.CreatePen(Brush2, 1));

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            // add tool commands
            Commands = new List<IToolCommand>
            {
                new ToggleGridCommand(ws, this, "Toggle Grid"),
                new ToggleSnappingCommand(ws, this, "Toggle Snapping")
            };

            // initialize with callbacks
            WatchDocument(ws.Document);

            return Task.FromResult(true);
        }

        public override Task OnPreDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            if (!IsVisible)
                return Task.FromResult(true);

            // draw gridlines
            DrawGridLines(renderer, ws);

            // draw debug stuff
            //var canvasx = -ws.RelativeTranslate.X;
            //var canvasy = -ws.RelativeTranslate.Y;
            //renderer.DrawCircle(canvasx, canvasy, 50, Pen); // point should remain in top left corner on screen
            //renderer.DrawCircle(0, 0, 20, Pen2); // point on canvas - should move along
            const float originLength = 50f;
            renderer.DrawLine(0f, -originLength, 0f, originLength, Pen);
            renderer.DrawLine(-originLength, 0f, originLength, 0f, Pen);

            return Task.FromResult(true);
        }

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            // add watch for element snapping
            WatchDocument(newDocument);
            UnWatchDocument(oldDocument);
        }

        public override Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            var me = @event as MoveEvent;
            if (me != null)
            {
                _areElementsMoved = true;
                _generalTranslation = null;
            }
            else
            {
                _generalTranslation = null;
                _areElementsMoved = false;
            }
            return Task.FromResult(true);
        }

        #region GridLines

        private void DrawGridLines(IRenderer renderer, SvgDrawingCanvas ws)
        {
            var screenTopLeft = ws.ScreenToCanvas(0, 0);

            var relativeCanvasTranslationX = screenTopLeft.X % StepSizeX;
            var relativeCanvasTranslationY = screenTopLeft.Y % StepSizeY;

            var height = renderer.Height / ws.ZoomFactor;
            var yPosition = height - height % StepSizeY + StepSizeY * 2;
            var stepSize = (int)Math.Round(StepSizeY, 0);

            var x = screenTopLeft.X - relativeCanvasTranslationX - stepSize * 2;
            // subtract 2x stepsize so gridlines always start from "out of sight" and lines do not start from a visible x-border
            var y = screenTopLeft.Y - relativeCanvasTranslationY;
            var lineLength = Math.Sqrt(Math.Pow(renderer.Width, 2) + Math.Pow(renderer.Height, 2)) / ws.ZoomFactor + stepSize * 4;

            for (var i = y - yPosition; i <= y + yPosition; i += stepSize)
            {
                DrawLineLeftToBottom(renderer, i, x, lineLength); /* \ */
            }

            for (var i = y; i <= y + 2 * yPosition; i += stepSize)
            {
                DrawLineLeftToTop(renderer, i, x, lineLength); /* / */
            }
        }

        // line looks like this -> /
        private void DrawLineLeftToTop(IRenderer renderer, float y, float canvasX, double lineLength)
        {
            var startX = canvasX;
            var startY = y;
            var stopX = (float)(lineLength * Math.Cos(Alpha * (Math.PI / 180))) + canvasX;
            var stopY = y - (float)(lineLength * Math.Sin(Alpha * (Math.PI / 180)));


            renderer.DrawLine(
                startX,
                startY,
                stopX,
                stopY,
                Pen);
        }

        // line looks like this -> \
        private void DrawLineLeftToBottom(IRenderer renderer, float y, float canvasX, double lineLength)
        {
            var startX = canvasX;
            var startY = y;
            var endX = (float)(lineLength * Math.Cos(Alpha * (Math.PI / 180))) + canvasX;
            var endY = y + (float)(lineLength * Math.Sin(Alpha * (Math.PI / 180)));

            renderer.DrawLine(
                startX,
                startY,
                endX,
                endY,
                Pen);
        }

        #endregion

        #region Snapping

        /// <summary>
        /// Subscribes to all visual elements "Add/RemoveChild" handlers and their "transformCollection changed" event
        /// </summary>
        /// <param name="document"></param>
        private void WatchDocument(SvgDocument document)
        {
            if (document == null)
                return;

            document.ChildAdded -= OnChildAdded;
            document.ChildAdded += OnChildAdded;

            foreach (var child in document.Children.OfType<SvgVisualElement>())
            {
                Subscribe(child);
            }
        }

        private void UnWatchDocument(SvgDocument document)
        {
            if (document == null)
                return;

            document.ChildAdded -= OnChildAdded;

            foreach (var child in document.Children.OfType<SvgVisualElement>())
            {
                Unsubscribe(child);
            }
        }

        private void Subscribe(SvgElement child)
        {
            if (!(child is SvgVisualElement))
                return;

            child.ChildAdded -= OnChildAdded;
            child.ChildAdded += OnChildAdded;
            child.ChildRemoved -= OnChildRemoved;
            child.ChildRemoved += OnChildRemoved;
            child.AttributeChanged -= OnAttributeChanged;
            child.AttributeChanged += OnAttributeChanged;
        }

        private void Unsubscribe(SvgElement child)
        {
            if (!(child is SvgVisualElement))
                return;

            child.ChildAdded -= OnChildAdded;
            child.ChildRemoved -= OnChildRemoved;
            child.AttributeChanged -= OnAttributeChanged;
        }

        private void OnChildAdded(object sender, ChildAddedEventArgs e)
        {
            Subscribe(e.NewChild);
            SnapElementToGrid(e.NewChild);
        }

        private void OnChildRemoved(object sender, ChildRemovedEventArgs e)
        {
            Unsubscribe(e.RemovedChild);
        }

        private void OnAttributeChanged(object sender, AttributeEventArgs e)
        {
            // if snapping is currently in progress, just skip (otherwise we might cause stackoverflowexception!
            if (_isSnappingInProgress)
                return;

            if (string.Equals(e.Attribute, "transform"))
            {
                var element = (SvgElement)sender;

                // if transform was changed and rotation has been added, skip snapping
                var oldRotation = (e.OldValue as SvgTransformCollection)?.GetMatrix()?.RotationDegrees;
                var newRotation = (e.Value as SvgTransformCollection)?.GetMatrix()?.RotationDegrees;
                if (oldRotation != newRotation)
                    return;

                // otherwise we need to reevaluate the translate of that particular element
                SnapElementToGrid(element);

                return;
            }

            var line = sender as SvgLine;
            if (line != null && Regex.IsMatch(e.Attribute, @"^[xy][12]$"))
            {
                _isSnappingInProgress = true;
                SnapLineToGrid(line);
                _isSnappingInProgress = false;
            }
        }

        private void SnapElementToGrid(SvgElement element)
        {
            if (!IsSnappingEnabled)
                return;

            var ve = element as SvgVisualElement;
            if (ve == null)
                return;

            try
            {
                _isSnappingInProgress = true;

                // snap to grid:
                // get absolute point
                var b = ve.GetBoundingBox();

                float absoluteDeltaX, absoluteDeltaY;

                // for the first moved element, store the translation and translate all other elements by the same translation
                // so if multiple elements are moved, their position relative to each other stays the same
                if (_areElementsMoved && _generalTranslation != null)
                {
                    absoluteDeltaX = _generalTranslation.X;
                    absoluteDeltaY = _generalTranslation.Y;
                }
                else
                {
                    SnapPointToGrid(b.X, b.Y, out absoluteDeltaX, out absoluteDeltaY);

                    if (_generalTranslation == null)
                    {
                        _generalTranslation = PointF.Create(absoluteDeltaX, absoluteDeltaY);
                    }
                }

                var mx = ve.CreateTranslation(absoluteDeltaX, absoluteDeltaY);
                ve.SetTransformationMatrix(mx);

                var line = element as SvgLine;
                if (line != null)
                {
                    var points = new[] { PointF.Create(0, 0) };
                    line.Transforms.GetMatrix().TransformPoints(points);
                    var transformedX = points[0].X;
                    var transformedY = points[0].Y;
                    line.StartX += transformedX;
                    line.StartY += transformedY;
                    line.EndX += transformedX;
                    line.EndY += transformedY;
                    line.SetTransformationMatrix(Matrix.Create());
                }
            }
            finally
            {
                _isSnappingInProgress = false;
            }
        }

        private void SnapLineToGrid(SvgLine line)
        {
            float absoluteDeltaX;
            float absoluteDeltaY;

            SnapPointToGrid(line.StartX, line.StartY, out absoluteDeltaX, out absoluteDeltaY);

            line.StartX += absoluteDeltaX;
            line.StartY += absoluteDeltaY;

            if (Math.Abs(absoluteDeltaX) < StepSizeX && Math.Abs(absoluteDeltaY) < StepSizeY)
                SnapPointToGrid(line.EndX, line.EndY, out absoluteDeltaX, out absoluteDeltaY);

            line.EndX += absoluteDeltaX;
            line.EndY += absoluteDeltaY;
        }

        private void SnapPointToGrid(float x, float y, out float absoluteDeltaX, out float absoluteDeltaY)
        {
            // determine next intersection of gridlines
            // so we determine which point P1, P2 is the nearest one
            // afterwards, see if the intermediary points (Px, Pz) are even closer 
            // and in that case transition to those.

            // so when we have the ankle points (P1, P2), determine if an intermediate point (Px, Pz) is even nearer
            /*
                                                    Px
                                                                                              Px = P1 + Vx
                                                 XXX+XX
                                              XXXX  | XXXX                                    Px = P1 + (P1.x + b)
                                            XXX     |    XXXX                                           (P1.y - a)
                                          XX   B = ?|       XXX
                                       XXX          |         XXX
                                     XXX            |            XXX
                     c = ?         XXX              |               XXX
                                XXX                 |                 XXX
                              XXX                   |  a = 20            XXX
                            XXX                     |                      XXX
                          XXX                       |                         XXX
                        XXX                         |                           XX
                      XXX                    G = 90 |                            XXX
                    XXX    A = 27.3                 |                              XXX
                P1 XX---------------------------------------------------------------+    P2
                   XXX           b = ?              |                             XX
                      XX                            |                           XXX
                       XXX                          |                         XXX
                          XXX                       |                       XXX
                            XXX                     |                     XXX
                              XXX                   |                  XXXX
                                XXX                 |                XXX
                                  XXXX              |              XXX
                                     XX             |            XXX
                                      XXX           |          XXX
                                        XXX         |       XXXX
                                          XXX       |     XXX
                                            XXX     |   XXX
                                              XXX  ++ XXX
                                                 XXX XX

                                                   Pz

             * 
             * */

            var diffX = x % StepSizeX;
            var diffY = y % StepSizeY;

            var deltaX = 0f;
            //if (diffX > StepSizeX / 2)
            //    deltaX = StepSizeX;

            var deltaY = 0f;
            if (diffY > StepSizeY / 2)
                deltaY = StepSizeY;

            // see if intermediary point is even nearer but also take Y coordinate into consideration!!
            if (diffX > StepSizeX / 2)
            {
                // transition to intermediary point
                deltaX = StepSizeX / 2;

                if (diffY >= StepSizeY / 2)
                    deltaY = StepSizeY / 2;
                else
                    deltaY = -StepSizeY / 2;
            }
            else if (diffX < -(StepSizeX / 2))
            {
                deltaX = -(StepSizeX / 2);

                if (diffY >= StepSizeY / 2)
                    deltaY = StepSizeY / 2;
                else
                    deltaY = -StepSizeY / 2;
            }

            absoluteDeltaX = deltaX - diffX;
            absoluteDeltaY = deltaY - diffY;
        }

        #endregion

        public override void Dispose()
        {
            _pen?.Dispose();
            _pen2?.Dispose();
            _brush?.Dispose();
            _brush2?.Dispose();
        }

        private static double SinDegree(double value)
        {
            return RadianToDegree(Math.Sin(DegreeToRadian(value)));
        }
        private static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        private static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        private class ToggleGridCommand : ToolCommand
        {
            private readonly SvgDrawingCanvas _canvas;

            public ToggleGridCommand(SvgDrawingCanvas canvas, GridTool tool, string name)
                : base(tool, name, (o) => { }, iconName: tool.IconGridOff, sortFunc: (tc) => 2000)
            {
                _canvas = canvas;
            }

            public override void Execute(object parameter)
            {
                var t = (GridTool)Tool;
                t.IsVisible = !t.IsVisible;
                IconName = t.IsVisible ? t.IconGridOff : t.IconGridOn;
                _canvas.FireInvalidateCanvas();
                _canvas.FireToolCommandsChanged();
            }
        }

        private class ToggleSnappingCommand : ToolCommand
        {
            private readonly SvgDrawingCanvas _canvas;

            public ToggleSnappingCommand(SvgDrawingCanvas canvas, GridTool tool, string name)
                : base(tool, name, (o) => { }, iconName: tool.IconGridOff, sortFunc: (tc) => 2000)
            {
                _canvas = canvas;
            }

            public override void Execute(object parameter)
            {
                var t = (GridTool)Tool;
                t.IsSnappingEnabled = !t.IsSnappingEnabled;
                IconName = t.IsSnappingEnabled ? t.IconGridOff : t.IconGridOn;
                _canvas.FireToolCommandsChanged();
            }
        }
    }
}