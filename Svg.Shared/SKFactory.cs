using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Svg.Interfaces;
using Svg.Interfaces.Xml;

namespace Svg
{
    public class SKFactory : SKFactoryBase
    {
        public override ISortedList<TKey, TValue> CreateSortedList<TKey, TValue>()
        {
            return new SvgSortedList<TKey, TValue>();
        }

        public override IXmlTextWriter CreateXmlTextWriter(StringWriter writer)
        {
            return new SvgXmlTextWriter(writer);
        }

        public override IXmlTextWriter CreateXmlTextWriter(Stream stream, Encoding utf8)
        {
            var w = new SvgXmlTextWriter(stream, utf8);
            w.Formatting = Formatting.Indented;
            return w;
        }
        public override XmlReader CreateSvgTextReader(Stream stream, Dictionary<string, string> entities)
        {
            var reader = new SvgTextReader(stream, entities);
            reader.XmlResolver = new SvgDtdResolver();
            reader.WhitespaceHandling = WhitespaceHandling.None;
            return reader;
        }

        public override XmlReader CreateSvgTextReader(StringReader r, Dictionary<string, string> entities)
        {
            var reader = new SvgTextReader(r, entities);
            reader.XmlResolver = new SvgDtdResolver();
            reader.WhitespaceHandling = WhitespaceHandling.None;
            return reader;
        }

    }
}