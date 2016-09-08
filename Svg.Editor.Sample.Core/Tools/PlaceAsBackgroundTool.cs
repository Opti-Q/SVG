using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core;
using Svg.Core.Interfaces;
using Svg.Core.Tools;

namespace Svg.Droid.SampleEditor.Core.Tools
{
    public class PlaceAsBackgroundTool : UndoableToolBase
    {
        public PlaceAsBackgroundTool(string jsonProperties, IUndoRedoService undoRedoService) : base("Background", jsonProperties, undoRedoService)
        {
            IconName = "ic_insert_photo_white_48dp.png";
        }

        public override async Task Initialize(SvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Place image", async o =>
                {
                    var path = await new AndroidPickImageService().PickImagePath(Canvas.ScreenWidth);
                    if (path == null) return;
                    PlaceImage(path);
                }, iconName: "ic_insert_photo_white_48dp.png"),
                new ToolCommand(this, "Remove image", o =>
                {
                    var children = Canvas.Document.Children;
                    var background = children.FirstOrDefault(x => x.CustomAttributes.ContainsKey("iclbackground"));
                    if (background != null)
                    {
                        children.Remove(background);
                        background.Dispose();

                        Canvas.ConstraintLeft = float.MinValue;
                        Canvas.ConstraintTop = float.MinValue;
                        Canvas.ConstraintRight = float.MaxValue;
                        Canvas.ConstraintBottom = float.MaxValue;

                        Canvas.FireInvalidateCanvas();
                        Canvas.FireToolCommandsChanged();
                    }
                }, o => Canvas.Document.Children.Any(x => x.CustomAttributes.ContainsKey("iclbackground")), iconName: "ic_delete_white_48dp.png")
            };

            if (ImagePath != null)
            {
                PlaceImage(ImagePath);
            }
        }

        private void PlaceImage(string path)
        {
            try
            {
                var children = Canvas.Document.Children;
                // insert the background before the first visible element
                var index = children.IndexOf(children.FirstOrDefault(x => x is SvgVisualElement));
                // if there are no visual elements, we want to add it to the end of the list
                if (index == -1) index = children.Count;
                if (!path.StartsWith("/")) path = path.Insert(0, "/");
                var image = new SvgImage
                {
                    Href = new Uri($"file://{path}", UriKind.Absolute)
                };
                image.CustomAttributes.Add("iclbackground", "");
                image.CustomAttributes.Add("iclnosnapping", "");

                // remove already placed background
                var formerBackground = children.FirstOrDefault(x => x.CustomAttributes.ContainsKey("iclbackground"));
                if (formerBackground != null)
                {
                    children.Remove(formerBackground);
                    formerBackground.Dispose();
                }

                children.Insert(index, image);

                var size = image.GetImageSize();
                Canvas.ConstraintLeft = 0;
                Canvas.ConstraintTop = 0;
                Canvas.ConstraintRight = size.Width;
                Canvas.ConstraintBottom = size.Height;

                Canvas.FireInvalidateCanvas();
                Canvas.FireToolCommandsChanged();
            }
            catch (IOException)
            {
                Debugger.Break();
            }
        }

        public string ImagePath
        {
            get
            {
                object imagePath;
                Properties.TryGetValue("imagepath", out imagePath);
                return imagePath as string;
            }
            set { Properties["imagepath"] = value; }
        }
    }
}
