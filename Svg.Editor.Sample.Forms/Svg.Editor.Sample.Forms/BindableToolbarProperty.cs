using System.Collections.Generic;
using System.Collections.Specialized;
using Xamarin.Forms;

namespace Svg.Editor.Sample.Forms
{
    public static class BindableToolbarProperty
    {
        public static readonly BindableProperty BindableToolbarItemsProperty = BindableProperty.CreateAttached("BindableToolbarItems",
           typeof(List<ToolbarItem>),
           typeof(Page),
           new List<ToolbarItem>(),
           propertyChanged: ToolbarItemsChanged);

        private static void ToolbarItemsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var page = bindable as Page;

            if (page == null) return;

            if (oldValue != null)
            {
                page.ToolbarItems.Clear();
            }

            var items = newValue as IEnumerable<ToolbarItem>;
            if (items != null)
            {
                foreach (var item in items)
                    page.ToolbarItems.Add(item);
            }
        }

        public static List<ToolbarItem> GetBindableToolbarItems(BindableObject bindable)
        {
            return (List<ToolbarItem>) bindable.GetValue(BindableToolbarItemsProperty);
        }

        public static void SetBindableToolbarItems(BindableObject bindable, List<ToolbarItem> value)
        {
            bindable.SetValue(BindableToolbarItemsProperty, value);
        }
    }
}
