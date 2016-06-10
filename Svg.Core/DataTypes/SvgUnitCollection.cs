using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

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

        public void ConvertAll(Func<object, float> func)
        {
            throw new NotImplementedException();
        }
    }
    
}