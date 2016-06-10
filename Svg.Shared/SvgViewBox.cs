using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Svg.Interfaces;

namespace Svg
{
    internal class SvgViewBoxConverter : TypeConverter
    {
        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"/> to use as the current culture.</param>
        /// <param name="value">The <see cref="T:System.Object"/> to convert.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> that represents the converted value.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                string[] coords = ((string)value).Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (coords.Length != 4)
                {
                    throw new SvgException("The 'viewBox' attribute must be in the format 'minX, minY, width, height'.");
                }

                return new SvgViewBox(float.Parse(coords[0], NumberStyles.Float, CultureInfo.InvariantCulture),
                    float.Parse(coords[1], NumberStyles.Float, CultureInfo.InvariantCulture),
                    float.Parse(coords[2], NumberStyles.Float, CultureInfo.InvariantCulture),
                    float.Parse(coords[3], NumberStyles.Float, CultureInfo.InvariantCulture));
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var viewBox = (SvgViewBox)value;

                return string.Format("{0}, {1}, {2}, {3}",
                    viewBox.MinX.ToString(CultureInfo.InvariantCulture), viewBox.MinY.ToString(CultureInfo.InvariantCulture),
                    viewBox.Width.ToString(CultureInfo.InvariantCulture), viewBox.Height.ToString(CultureInfo.InvariantCulture));
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}