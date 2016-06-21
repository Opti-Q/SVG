using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Svg.Interfaces;
using Svg.Interfaces.Xml;

namespace Svg
{
    public sealed class SvgXmlTextWriter : IXmlTextWriter
    {
        private XmlTextWriter _w;

        public SvgXmlTextWriter(Stream stream, Encoding encoding)
        {
            _w = new XmlTextWriter(stream, encoding);
        }

        public SvgXmlTextWriter(StringWriter writer)
        {
            _w = new XmlTextWriter(writer);
        }

        public Formatting Formatting { get { return _w.Formatting; } set { _w.Formatting = value; } }

        public void WriteAttributeString(string xmlns, string value)
        {
            _w.WriteAttributeString(xmlns, value);
        }

        public void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            _w.WriteDocType(name, pubid, sysid, subset);
        }

        public void WriteProcessingInstruction(string name, string text)
        {
            _w.WriteProcessingInstruction(name, text);
        }

        public void Flush()
        {
            _w.Flush();
        }

        public void WriteStartElement(string elementName)
        {
            _w.WriteStartElement(elementName);
        }

        public void WriteEndElement()
        {
            _w.WriteEndElement();
        }

        public void WriteString(string content)
        {
            _w.WriteString(content);
        }

        public void WriteRaw(string content)
        {
            _w.WriteRaw(content);
        }

        public void WriteStartDocument()
        {
            _w.WriteStartDocument(false);
        }

        public void Dispose()
        {
            _w.Dispose();
        }
    }
}