using System;
using System.Collections.Generic;
using Svg.Editor;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Droid.SampleEditor.Core.Tools
{
    public class AddRandomItemTool : ToolBase
    {
        private static Random _random = new Random();

        private string[] SvgPathStrings { get; } =
        {
            //"isolib/Welds/solid/weld1.svg",
            //"isolib/Valves/Valves/valve1.svg",
            //"isolib/Valves/Valves/valve2.svg",
            //"isolib/Valves/Valves/valve3.svg",
            //"isolib/Reducers/solid/reducer1.svg",
            //"isolib/Straights/solid and broken/solid1.svg",
            //"svg/painting-control-01-f.svg",
            //"svg/blind01.svg",
            //"svg/Blinds_6.svg",
            //"svg/Blinds_6_gezoomtes_minibild.svg",
            //"svg/Positions_13_kein_text_im_minibild_und_canvas.svg",
            //"svg/ic_format_color_fill_white_48px.svg",
            //"svg/Spec_change_2.svg",
            //"svg/painting-marker-05-f.svg",
            //"svg/painting-marker-01-f.svg",
            //"svg/rect.svg",
            //"svg/Bends_01.svg", causes StackOverflowException in SvgRectangle.Bounds
            //"isolib/Positions/Positions/Positions_03.svg",
            "svg/findingmarker.svg",
            //"svg/Penetrations.svg",
            //"svg/Positions.svg",
            //"svg/Reducers.svg",
            //"svg/Spec_change.svg",
            //"svg/Straights.svg",
            //"svg/Valves.svg",
            //"svg/Welds.svg"
        };

        public AddRandomItemTool(SvgDrawingCanvas canvas, Func<string, ISvgSource> sourceProvider = null) : base("Add random item")
        {
            SourceProvider = sourceProvider;
            ToolType = ToolType.Create;
            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Add random item", (obj) =>
                {
                    if (SourceProvider == null)
                        return;
                    //var provider = SourceProvider("isolib/Welds/solid/weld1.svg");
                    //var provider = SourceProvider("isolib/Valves/Valves/valve1.svg");
                    //var provider = SourceProvider("isolib/Valves/Valves/valve2.svg");
                    //var provider = SourceProvider("isolib/Valves/Valves/valve3.svg");
                    //var provider = SourceProvider("isolib/Valves/Valves/valve4.svg");
                    //var provider = SourceProvider("isolib/Reducers/solid/reducer1.svg");
                    //var provider = SourceProvider("isolib/Straights/solid and broken/solid1.svg");
                    //var provider = SourceProvider("svg/painting-control-01-f.svg");
                    //var provider = SourceProvider("svg/blind01.svg");
                    //var provider = SourceProvider("svg/Blinds_6.svg");
                    //var provider = SourceProvider("svg/Blinds_6_gezoomtes_minibild.svg");
                    //var provider = SourceProvider("svg/Positions_13_kein_text_im_minibild_und_canvas.svg");
                    //var provider = SourceProvider("svg/ic_format_color_fill_white_48px.svg");
                    //var provider = SourceProvider("svg/Spec_change_2.svg");
                    //var provider = SourceProvider("svg/positions_05.svg");
                    //var provider = SourceProvider("svg/Spec_change_2.svg");
                    //var provider = SourceProvider("svg/painting-marker-05-f.svg");
                    //var provider = SourceProvider("svg/painting-marker-01-f.svg");
                    //var provider = SourceProvider("svg/rect.svg");
                    var provider = SourceProvider(SvgPathStrings[_random.Next(0, SvgPathStrings.Length)]);
                    var otherDoc = SvgDocument.Open<SvgDocument>(provider);

                    Canvas.AddItemInScreenCenter(otherDoc);

                    //Canvas.AddItemInScreenCenter(new SvgImage { Width = 100, Height = 100, Href = new Uri("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUA" +
                    //                                              "AAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0D" +
                    //                                              "HxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg ==", UriKind.Absolute)});

                    //var fs = SvgEngine.Resolve<IFileSystem>();
                    //var svgCachingService = SvgEngine.Resolve<ISvgCachingService>();
                    //var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
                    //var selectedColor = colorTool.SelectedColor;
                    //var path = svgCachingService.GetCachedPngPath(colorTool.IconName, $"{selectedColor.R}_{selectedColor.G}_{selectedColor.B}", fs);
                    //try
                    //{
                    //    Canvas.AddItemInScreenCenter(new SvgImage
                    //    {
                    //        Width = 100,
                    //        Height = 100,
                    //        Href = new Uri($"file://{path}", UriKind.Absolute)
                    //    });
                    //}
                    //catch (IOException)
                    //{
                    //    Debugger.Break();
                    //}

                } , sortFunc:(x) => 1200)
            };
        }

        public Func<string, ISvgSource> SourceProvider { get; set; }
    }
}
