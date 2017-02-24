using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Svg
{
    public class WebRequestSvc : IWebRequest
    {
        public Stream GetResponse(Uri uri)
        {
            return Task.Run(async () =>
            {
                var httpRequest = WebRequest.Create(uri);
                return await httpRequest.GetRequestStreamAsync();
            }).Result;
        }
    }
}