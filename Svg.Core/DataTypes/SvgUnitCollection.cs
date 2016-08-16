using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace Svg
{
    /// <summary>
    /// Represents a list of <see cref="SvgUnit"/>s.
    /// </summary>
    //[TypeConverter(typeof(SvgUnitCollectionConverter))]
    public class SvgUnitCollection : List<SvgUnit>
    {

        /// <summary>
        /// Gets an <see cref="SvgUnitCollection"/> that should inherit from its parent.
        /// </summary>
        public static readonly SvgUnitCollection Inherit = new SvgUnitCollection();

        /// <summary>
        /// An implicit operator that converts a SvgUnitCollection to its string representation.
        /// </summary>
        /// <param name="svgUnitCollection"></param>
        public static implicit operator string(SvgUnitCollection svgUnitCollection)
        {
            return svgUnitCollection.ToString();
        }

        /// <summary>
        /// An implicit operator that converts a string to a SvgUnitCollection.
        /// </summary>
        /// <param name="code"></param>
        public static implicit operator SvgUnitCollection(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            var svgUnitCollection = new SvgUnitCollection();
            var elements = code.Split(' ');
            svgUnitCollection.AddRange(elements.Select(x =>
            {
                float parsed;
                return float.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) ? new SvgUnit(parsed) : 0;
            }));
            return svgUnitCollection;
        }

        public override string ToString()
        {
            return string.Join<SvgUnit>(" ", this);
        }

        public static bool IsNullOrEmpty(SvgUnitCollection collection)
        {
            return collection == null || collection.Count < 1 ||
                (collection.Count == 1 && (collection[0] == SvgUnit.Empty || collection[0] == SvgUnit.None));
        }

        // TODO LX: is this correct?
        public IEnumerable<float> ConvertAll(Func<SvgUnit, float> func)
        {
            foreach (var unit in this)
                yield return func(unit);
        }

        public SvgUnitCollection Clone()
        {
            var values = this.Select(u => u.Clone());
            var c = new SvgUnitCollection();
            c.AddRange(values);
            return c;
        }
    }
    
}