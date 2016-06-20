using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmCross.Platform;
using MvvmCross.Plugins.Email;
using Svg.Core;
using Svg.Core.Tools;
using Svg.Interfaces;

namespace Svg.Droid.SampleEditor.Core.Tools
{
    public class SaveTool : ToolBase
    {
        private readonly bool _autoLoad;

        public SaveTool(bool autoLoad) : base("Save/Load")
        {
            _autoLoad = autoLoad;
        }

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            Commands = new[]
            {
                new ToolCommand(this, "Save", (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();

                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, "svg_file.svg");

                    if (fs.FileExists(storagePath))
                    {
                        fs.DeleteFile(storagePath);
                    }

                    ws.Document.Write(storagePath);

                    ws.FireToolCommandsChanged();

                }, (obj) => ws.Document != null),
                new ToolCommand(this, "Load", (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();

                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, "svg_file.svg");

                    if (fs.FileExists(storagePath))
                    {
                        try
                        {
                            ws.Document = Svg.SvgDocument.Open<SvgDocument>(storagePath);
                        }
                        catch (Exception)
                        {
                            fs.DeleteFile(storagePath);
                        }
                    }
                    ws.FireToolCommandsChanged();

                }, (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();

                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, "svg_file.svg");
                    return fs.FileExists(storagePath);
                }),

                new ToolCommand(this, "Share", (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();

                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, "svg_file.svg");

                    if (fs.FileExists(storagePath))
                    {
                        using (var stream = fs.OpenRead(storagePath))
                        {
                            var share = Mvx.Resolve<IMvxComposeEmailTaskEx>();
                            share.ComposeEmail(new [] {"alexander.marek@outlook.com"} , subject:$"SVG {DateTime.Now.ToString()}", attachments: new [] {new EmailAttachment {Content=stream, ContentType = "image/svg+xml", FileName = "svg_file.svg"} });
                        }
                    }

                }, (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();

                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, "svg_file.svg");
                    return fs.FileExists(storagePath);
                })

            };

            if (_autoLoad)
            {
                Commands.Single(t => t.Name == "Load").Execute(null);
            }

            return Task.FromResult(true);
        }
    }
}
