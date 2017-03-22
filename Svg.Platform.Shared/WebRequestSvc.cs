using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Svg.Interfaces;

namespace Svg
{
    public class WebRequestSvc : IWebRequest
    {
        public Stream GetResponse(Uri uri)
        {
            if (string.Equals(uri.Scheme, "file", StringComparison.OrdinalIgnoreCase))
            {
                var filePath = uri.OriginalString.Substring(7).TrimStart('/');
                var fs = SvgEngine.Resolve<IFileSystem>();
                return File.OpenRead(Path.Combine(fs.GetDefaultStoragePath(), filePath));
            }

            return Task.Run(async () =>
            {
                var httpRequest = WebRequest.Create(uri);
                return await httpRequest.GetRequestStreamAsync();
            }).Result;
        }
    }
}