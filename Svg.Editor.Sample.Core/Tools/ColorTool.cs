using System.Collections.Generic;
using System.Threading.Tasks;
using SkiaSharp;
using Svg.Core;
using Svg.Core.Tools;
using Svg.Interfaces;
using Svg.Platform;

namespace Svg.Droid.SampleEditor.Core.Tools
{
    public class ColorTool : ToolBase
    {
        public ColorTool() : base("Color")
        {
        }

        public Color SelectedColor { get; set; } = Color.Create(255, 0, 0);

        public string ColorIconName { get; set; } = "ic_color_white_48dp.png";

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            // add tool commands
            Commands = new List<IToolCommand>
            {
                new ChangeColorCommand(ws, this, "Change Color")
            };

            // initialize with callbacks
            WatchDocument(ws.Document);

            return Task.FromResult(true);
        }

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            // add watch for element snapping
            WatchDocument(newDocument);
            UnWatchDocument(oldDocument);
        }

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

            //foreach (var child in document.Children.OfType<SvgVisualElement>())
            //{
            //    Subscribe(child);
            //}
        }

        private void UnWatchDocument(SvgDocument document)
        {
            if (document == null)
                return;

            document.ChildAdded -= OnChildAdded;

            //foreach (var child in document.Children.OfType<SvgVisualElement>())
            //{
            //    Unsubscribe(child);
            //}
        }

        //private void Subscribe(SvgElement child)
        //{
        //    if (!(child is SvgVisualElement))
        //        return;

        //    child.ChildAdded -= OnChildAdded;
        //    child.ChildAdded += OnChildAdded;
        //    child.ChildRemoved -= OnChildRemoved;
        //    child.ChildRemoved += OnChildRemoved;
        //    child.AttributeChanged -= OnAttributeChanged;
        //    child.AttributeChanged += OnAttributeChanged;
        //}

        //private void Unsubscribe(SvgElement child)
        //{
        //    if (!(child is SvgVisualElement))
        //        return;

        //    child.ChildAdded -= OnChildAdded;
        //    child.ChildRemoved -= OnChildRemoved;
        //    child.AttributeChanged -= OnAttributeChanged;
        //}

        private void OnChildAdded(object sender, ChildAddedEventArgs e)
        {
            //Subscribe(e.NewChild);

            e.NewChild.Color = new SvgColourServer(SelectedColor);
        }

        private void OnChildRemoved(object sender, ChildRemovedEventArgs e)
        {
            //Unsubscribe(e.RemovedChild);
        }

        private void OnAttributeChanged(object sender, AttributeEventArgs e)
        {
        }

        private class ChangeColorCommand : ToolCommand
        {
            private readonly SvgDrawingCanvas _canvas;

            public ChangeColorCommand(SvgDrawingCanvas canvas, ColorTool tool, string name)
                : base(tool, name, (o) => { }, iconName: tool.ColorIconName, sortFunc:(tc) => 1000)
            {
                _canvas = canvas;
            }

            public override void Execute(object parameter)
            {
                var t = (ColorTool)Tool;

                // TODO: change color
            }
        }
    }
}