using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Svg
{
    public interface IWebRequest
    {
        WebResponse GetResponse(Uri uri);
    }
}
