using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Android.Text.Method;
using Svg.Interfaces;

namespace Svg
{
    public class SvgSortedList<TKey, TValue> : SortedList<TKey, TValue>, ISortedList<TKey, TValue>
    {
    }
}