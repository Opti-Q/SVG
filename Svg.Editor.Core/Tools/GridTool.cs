using System;
using System.Collections.Generic;
using Svg.Core.Events;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public class GridTool : ToolBase
    {
        private readonly ICanInvalidateCanvas _canvas;
        private static float StepSizeY = 40;
        private static double A;
        private static double B;
        private static double C;
        private static float StepSizeX;
        private const double Alpha = 27.3f;
        private const double Gamma = 90f;
        private static double Beta;
        private Pen _pen;

        private readonly List<IToolCommand> _commands = new List<IToolCommand>();
        private Pen _pen2;

        public GridTool(ICanInvalidateCanvas canvas)
            : base("Grid")
        {
            _canvas = canvas;
            // using triangle calculation to determine the x and y steps based on stepsize (y) and angle (alpha)
            // http://www.arndt-bruenner.de/mathe/scripts/Dreiecksberechnung.htm
            A = StepSizeY;
            Beta = 180f - (Alpha + Gamma);
            B = (A * SinDegree(Beta)) / SinDegree(Alpha);
            C = (A * SinDegree(Gamma)) / SinDegree(Alpha);
            StepSizeX = (float)B;
            
            // add tool commands
            _commands.Add(new ToolCommand(this, "Toggle Grid", (obj) =>
            {
                IsVisible = !IsVisible;
                _canvas.InvalidateCanvas();
            }, (obj) => true));
            Commands = _commands;
        }

        public bool IsVisible { get; set; } = true;

        private Pen Pen => _pen ?? (_pen = Svg.Engine.Factory.CreatePen(Svg.Engine.Factory.CreateSolidBrush(Svg.Engine.Factory.CreateColorFromArgb(255, 210, 210, 210)), 1));
        private Pen Pen2 => _pen2 ?? (_pen2 = Svg.Engine.Factory.CreatePen(Svg.Engine.Factory.CreateSolidBrush(Svg.Engine.Factory.CreateColorFromArgb(255, 255, 0, 0)), 2));

        public override void OnPreDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            //--------------------------------------------------
            // TODO DO THIS ONLY ONCE; NOT FOR EVERY ON DRAW
            //--------------------------------------------------

            if (!IsVisible)
                return;

            var canvasx = -ws.Translate.X;
            var canvasy = -ws.Translate.Y;
           
            //for (var i = -canvas.Width * MaxZoom; i <= canvas.Width * MaxZoom; i += StepSize - 2.5f)
            //    DrawTopDownIsoLine(canvas, i, canvasx, canvasy);      /* | */

            var relativeCanvasTranslationX = (canvasx) % StepSizeX;
            var relativeCanvasTranslationY = (canvasy) % StepSizeY;

            var height = renderer.Height/ws.ZoomFactor;
            var yPosition = (height - (height % StepSizeY) + (StepSizeY*2));
            var stepSize = (int) Math.Round(StepSizeY, 0);

            var x = canvasx - relativeCanvasTranslationX - (stepSize*2); // subtract 2x stepsize so gridlines always start from "out of sight" and lines do not start from a visible x-border
            var y = canvasy - relativeCanvasTranslationY;
            var lineLength = Math.Sqrt(Math.Pow(renderer.Width, 2) + Math.Pow(renderer.Height, 2)) / ws.ZoomFactor + (stepSize * 2); // multiply by 1.2f as we later also start drawing from a minus x coordinate (- 2*stepsize)

            for (var i = y - yPosition; i <= y + yPosition; i += stepSize)
            {
                DrawLineLeftToBottom(renderer, i, x, lineLength);    /* \ */
            }

            for (var i = y; i <= y + 2 * yPosition; i += stepSize)
            {
                DrawLineLeftToTop(renderer, i, x, lineLength);       /* / */
            }

            renderer.DrawCircle(canvasx, canvasy, 50, Pen); // point should remain in top left corner on screen
            renderer.DrawCircle(0, 0, 20, Pen2); // point on canvas - should move along
            renderer.DrawLine(1f, 1f, 200f, 1f, Pen2);
        }

        public override void OnUserInput(UserInputEvent userInputEvent, SvgDrawingCanvas ws)
        {
            // You know nothing Jon Snow
        }
        
        // line looks like this -> /
        private void DrawLineLeftToTop(IRenderer renderer, float y, float canvasX, double lineLength)
        {
            var startX = canvasX;
            var startY = y ;
            var stopX = ((float)(lineLength * Math.Cos(Alpha * (Math.PI / 180)))) + canvasX;
            var stopY = (y - (float)(lineLength * Math.Sin(Alpha * (Math.PI / 180)))) ;
            

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
            var endX = ((float)(lineLength * Math.Cos(Alpha * (Math.PI / 180)))) + canvasX;
            var endY = (y + (float)(lineLength * Math.Sin(Alpha * (Math.PI / 180))));

            renderer.DrawLine(
                startX,
                startY,
                endX,
                endY,
                Pen);
        }

        //// line looks like this -> |
        //private void DrawTopDownIsoLine(ICanvas canvas, float y)
        //{
        //    if(ZoomTool.ScaleFactor < 0.85f)
        //        return;

        //    canvas.DrawLine(
        //        (y),
        //        (-(canvas.Height * MaxZoom)),
        //        (y),
        //        (canvas.Height * MaxZoom),
        //        Paint);
        //}

        public override void Dispose()
        {
            _pen?.Dispose();
            _pen2?.Dispose();
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
    }
}