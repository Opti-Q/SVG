using System;
using System.Linq;
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
        private readonly Func<string> _fileName;

        public SaveTool(bool autoLoad = true, Func<string> fileName = null ) : base("Save/Load")
        {
            _autoLoad = autoLoad;
            _fileName = fileName ?? (() => "svg_image.svg");
        }

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            Commands = new[]
            {
                new ToolCommand(this, "Save", (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();
                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, _fileName());

                    if (fs.FileExists(storagePath))
                    {
                        fs.DeleteFile(storagePath);
                    }

                    ws.Document.Write(storagePath);

                    ws.FireToolCommandsChanged();

                }, 
                (obj) => ws.Document != null),
                new ToolCommand(this, "Load", (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();
                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, _fileName());

                    if (fs.FileExists(storagePath))
                    {
                       ws.Document = Svg.SvgDocument.Open<SvgDocument>(storagePath);
                    }
                    ws.FireToolCommandsChanged();

                }, 
                (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();
                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, _fileName());
                    return fs.FileExists(storagePath);
                }),
                new ToolCommand(this, "Share", (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();

                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, _fileName());

                    if (fs.FileExists(storagePath))
                    {
                        using (var stream = fs.OpenRead(storagePath))
                        {
                            var share = Mvx.Resolve<IMvxComposeEmailTaskEx>();
                            share.ComposeEmail(new [] {"someone@somewhere.com"} , subject:$"SVG {DateTime.Now.ToString()}", attachments: new [] {new EmailAttachment {Content=stream, ContentType = "image/svg+xml", FileName = "svg_file.svg"} });
                        }
                    }

                }, 
                (obj) =>
                {
                    var fs = Svg.Engine.Resolve<IFileSystem>();
                    var path = fs.GetDownloadFolder();
                    var storagePath = fs.PathCombine(path, _fileName());
                    return fs.FileExists(storagePath);
                }),
                new ToolCommand(this, "Test SaveLoad", (obj) =>
                {
                    ws.Document = new SvgDocument();
                    this.Commands.Single(c => c.Name == "Save").Execute(null);
                    this.Commands.Single(c => c.Name == "Load").Execute(null);
                }),
                new ToolCommand(this, "Clear", (obj) =>
                {
                    ws.Document = new SvgDocument();
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
