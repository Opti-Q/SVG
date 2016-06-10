using System;
using System.Net;

namespace Svg
{
    public class WebRequestSvc : IWebRequest
    {
        public WebResponse GetResponse(Uri uri)
        {
            var httpRequest = WebRequest.Create(uri);
            return httpRequest.GetResponse();
        }
    }
}