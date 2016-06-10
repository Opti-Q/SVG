using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces.Xml
{
    public interface IXmlTextWriter : IDisposable
    {
        void WriteAttributeString(string xmlns, string value);
        void WriteDocType(string name, string pubid, string sysid, string subset);
        void WriteProcessingInstruction(string name, string text);
        void Flush();
        void WriteStartElement(string elementName);
        void WriteEndElement();
        void WriteString(string content);
        void WriteRaw(string content);
    }
}
