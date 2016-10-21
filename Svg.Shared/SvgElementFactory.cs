using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using ExCSS;
using Svg.Interfaces;

namespace Svg
{
    /// <summary>
    /// Provides the methods required in order to parse and create <see cref="SvgElement"/> instances from XML.
    /// </summary>
    public class SvgElementFactory : ISvgElementFactory
    {
        private static Dictionary<string, ElementInfo> availableElements;
        private static Parser cssParser = new Parser();

        /// <summary>
        /// Gets a list of available types that can be used when creating an <see cref="SvgElement"/>.
        /// </summary>
        public Dictionary<string, ElementInfo> AvailableElements
        {
            get
            {
                if (availableElements == null)
                {
                    var svgTypes = from t in typeof(SvgDocument).Assembly.GetExportedTypes()
                                   where t.GetCustomAttributes(typeof(SvgElementAttribute), true).Length > 0
                                   && t.IsSubclassOf(typeof(SvgElement))
                                   select new ElementInfo { ElementName = ((SvgElementAttribute)t.GetCustomAttributes(typeof(SvgElementAttribute), true)[0]).ElementName, ElementType = t };

                    availableElements = (from t in svgTypes
                                         where t.ElementName != "svg"
                                         group t by t.ElementName into types
                                         select types).ToDictionary(e => e.Key, e => e.SingleOrDefault());
                }

                return availableElements;
            }
        }

        /// <summary>
        /// Creates an <see cref="SvgDocument"/> from the current node in the specified <see cref="XmlTextReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="XmlTextReader"/> containing the node to parse into an <see cref="SvgDocument"/>.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> parameter cannot be <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The CreateDocument method can only be used to parse root &lt;svg&gt; elements.</exception>
        public T CreateDocument<T>(XmlReader reader) where T : SvgDocument, new()
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            if (reader.LocalName != "svg")
            {
                throw new InvalidOperationException("The CreateDocument method can only be used to parse root <svg> elements.");
            }

            return (T)CreateElement<T>(reader, true, null);
        }

        /// <summary>
        /// Creates an <see cref="SvgElement"/> from the current node in the specified <see cref="XmlTextReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="XmlTextReader"/> containing the node to parse into a subclass of <see cref="SvgElement"/>.</param>
        /// <param name="document">The <see cref="SvgDocument"/> that the created element belongs to.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> and <paramref name="document"/> parameters cannot be <c>null</c>.</exception>
        public SvgElement CreateElement(XmlReader reader, SvgDocument document)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            return CreateElement<SvgDocument>(reader, false, document);
        }

        private SvgElement CreateElement<T>(XmlReader reader, bool fragmentIsDocument, SvgDocument document) where T : SvgDocument, new()
        {
            SvgElement createdElement = null;
            string elementName = reader.LocalName;
            string elementNS = reader.NamespaceURI;

            Engine.Logger.Debug($"Begin CreateElement: {elementName}");

            if (elementNS == SvgAttributeAttribute.SvgNamespace || string.IsNullOrEmpty(elementNS))
            {
                if (elementName == "svg")
                {
                    createdElement = (fragmentIsDocument) ? new T() : new SvgFragment();
                }
                else
                {
                    ElementInfo validType = null;
                    if (AvailableElements.TryGetValue(elementName, out validType))
                    {
                        createdElement = (SvgElement) Activator.CreateInstance(validType.ElementType);
                    }
                    else
                    {
                        createdElement = new SvgUnknownElement(elementName);
                    }
                }

                if (createdElement != null)
                {
                    SetAttributes(createdElement, reader, document);
                }
            }
            else
            {
                // All non svg element (html, ...)
                createdElement = new NonSvgElement(elementName);
                SetAttributes(createdElement, reader, document);
            }

            Engine.Logger.Debug("End CreateElement");

            return createdElement;
        }

        private void SetAttributes(SvgElement element, XmlReader reader, SvgDocument document)
        {
            Engine.Logger.Debug("Begin SetAttributes");

            //string[] styles = null;
            //string[] style = null;
            //int i = 0;

            while (reader.MoveToNextAttribute())
            {
                var attributeName = reader.LocalName;

                if (!Regex.IsMatch(attributeName, @"^(:|[A-Z]|_|[a-z]|[\u00C0-\u00D6]|[\u00D8-\u00F6]|[\u00F8-\u02FF]|[\u0370-\u037D]|[\u037F-\u1FFF]|[\u200C-\u200D]|[\u2070-\u218F]|[\u2C00-\u2FEF]|[\u3001-\uD7FF]|[\uF900-\uFDCF]|[\uFDF0-\uFFFD])"))
                    continue;

                if (attributeName.Equals("style") && !(element is NonSvgElement)) 
                {
                    var inlineSheet = cssParser.Parse("#a{" + reader.Value + "}");
                    foreach (var rule in inlineSheet.StyleRules)
                    {
                        foreach (var decl in rule.Declarations)
                        {
                            if (!Regex.IsMatch(decl.Name, @"^(:|[A-Z]|_|[a-z]|[\u00C0-\u00D6]|[\u00D8-\u00F6]|[\u00F8-\u02FF]|[\u0370-\u037D]|[\u037F-\u1FFF]|[\u200C-\u200D]|[\u2070-\u218F]|[\u2C00-\u2FEF]|[\u3001-\uD7FF]|[\uF900-\uFDCF]|[\uFDF0-\uFFFD])"))
                                continue;
                            element.AddStyle(decl.Name, decl.Term.ToString(), SvgElement.StyleSpecificity_InlineStyle);
                        }
                    }
                }
                else if (IsStyleAttribute(attributeName))
                {
                    element.AddStyle(attributeName, reader.Value, SvgElement.StyleSpecificity_PresAttribute);
                }
                else
                {
                    SetPropertyValue(element, attributeName, reader.Value, document);
                }
            }

            Engine.Logger.Debug("End SetAttributes");
        }

        public bool IsStyleAttribute(string name)
        {
            switch (name)
            {
                case "alignment-baseline":
                case "baseline-shift":
                case "clip":
                case "clip-path":
                case "clip-rule":
                case "color":
                case "color-interpolation":
                case "color-interpolation-filters":
                case "color-profile":
                case "color-rendering":
                case "cursor":
                case "direction":
                case "display":
                case "dominant-baseline":
                case "enable-background":
                case "fill":
                case "fill-opacity":
                case "fill-rule":
                case "filter":
                case "flood-color":
                case "flood-opacity":
                case "font":
                case "font-family":
                case "font-size":
                case "font-size-adjust":
                case "font-stretch":
                case "font-style":
                case "font-variant":
                case "font-weight":
                case "glyph-orientation-horizontal":
                case "glyph-orientation-vertical":
                case "image-rendering":
                case "kerning":
                case "letter-spacing":
                case "lighting-color":
                case "marker":
                case "marker-end":
                case "marker-mid":
                case "marker-start":
                case "mask":
                case "opacity":
                case "overflow":
                case "pointer-events":
                case "shape-rendering":
                case "stop-color":
                case "stop-opacity":
                case "stroke":
                case "stroke-dasharray":
                case "stroke-dashoffset":
                case "stroke-linecap":
                case "stroke-linejoin":
                case "stroke-miterlimit":
                case "stroke-opacity":
                case "stroke-width":
                case "text-anchor":
                case "text-decoration":
                case "text-rendering":
                case "unicode-bidi":
                case "visibility":
                case "word-spacing":
                case "writing-mode":
                    return true;
            }
            return false;
        }

        private static Dictionary<Type, Dictionary<string, PropertyDescriptorCollection>> _propertyDescriptors = new Dictionary<Type, Dictionary<string, PropertyDescriptorCollection>>();
        private static object syncLock = new object();

        public void SetPropertyValue(SvgElement element, string attributeName, string attributeValue, SvgDocument document)
        {
            var elementType = element.GetType();

            PropertyDescriptorCollection properties;
            lock (syncLock)
            {
                if (_propertyDescriptors.Keys.Contains(elementType))
                {
                    if (_propertyDescriptors[elementType].Keys.Contains(attributeName))
                    {
                        properties = _propertyDescriptors[elementType][attributeName];
                    }
                    else
                    {
                        properties = TypeDescriptor.GetProperties(elementType, new[] { new SvgAttributeAttribute(attributeName) });
                        _propertyDescriptors[elementType].Add(attributeName, properties);
                    }
                }
                else
                {
                    properties = TypeDescriptor.GetProperties(elementType, new[] { new SvgAttributeAttribute(attributeName) });
                    _propertyDescriptors.Add(elementType, new Dictionary<string, PropertyDescriptorCollection>());

                    _propertyDescriptors[elementType].Add(attributeName, properties);
                } 
            }

            if (properties.Count > 0)
            {
                PropertyDescriptor descriptor = properties[0];

                try
                {
					if (attributeName == "opacity" && attributeValue == "undefined")
					{
						attributeValue = "1";
					}

                    var ctx = new SvgDocumentTypeDescriptorContext(document);

                    if (attributeName == "visibility")
                    {
                        bool visible = string.Equals(attributeValue, "visible",
                            StringComparison.InvariantCultureIgnoreCase);
                        descriptor.SetValue(element, visible);
                    }
                    else
                    {
                        descriptor.SetValue(element,
                            descriptor.Converter.ConvertFrom(ctx, CultureInfo.InvariantCulture, attributeValue));
                    }

                }
                catch
                {
                    Engine.Logger.Warn($"Attribute '{attributeName}' cannot be set - type '{descriptor.PropertyType.FullName}' cannot convert from string '{attributeValue}'.");
                }
            }
            else
            {
                //check for namespace declaration in svg element
                if (string.Equals(element.ElementName, "svg", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(attributeName, "xmlns", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(attributeName, "xlink", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(attributeName, "xmlns:xlink", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(attributeName, "version", StringComparison.OrdinalIgnoreCase))
                    {
                        //nothing to do
                    }
                    else
                    {
                        //attribute is not a svg attribute, store it in custom attributes
                        element.CustomAttributes[attributeName] = attributeValue;
                    }
                }
                else
                {
                    //attribute is not a svg attribute, store it in custom attributes
                    element.CustomAttributes[attributeName] = attributeValue;
                }
            }
        }

    }

    internal class SvgDocumentTypeDescriptorContext : ITypeDescriptorContext, ISvgDocumentProvider
    {
        private readonly SvgDocument _document;

        public SvgDocumentTypeDescriptorContext(SvgDocument document)
        {
            _document = document;
        }

        #region ITypeDescriptorContext Members

        IContainer ITypeDescriptorContext.Container
        {
            get { throw new NotImplementedException(); }
        }

        object ITypeDescriptorContext.Instance
        {
            get { return Document; }
        }

        void ITypeDescriptorContext.OnComponentChanged()
        {
            throw new NotImplementedException();
        }

        bool ITypeDescriptorContext.OnComponentChanging()
        {
            throw new NotImplementedException();
        }

        PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor
        {
            get { throw new NotImplementedException(); }
        }

        public SvgDocument Document
        {
            get { return _document; }
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }

        #endregion

        public static explicit operator SvgDocument(SvgDocumentTypeDescriptorContext other)
        {
            return other.Document;
        }
    }
}