using System;
using System.Linq;
using System.Collections.Generic;

namespace Svg
{
    /// <summary>
    /// Represents a list of <see cref="SvgUnits"/>.
    /// </summary>
    //[TypeConverter(typeof(SvgUnitCollectionConverter))]
    public class SvgUnitCollection : List<SvgUnit>
    {
        public override string ToString()
        {
            string ret = "";
            foreach (var unit in this)
            {
                ret += unit.ToString() + " ";
            }

            return ret;
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