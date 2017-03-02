using System;
using System.Globalization;
using Xamarin.Forms;

namespace Svg.Editor.Forms
{
    public class MenuActionsToToolbarItemsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //var menuActions = value as IEnumerable<IMenuAction>;
            //object iconColor;
            //if (!Application.Current.Resources.TryGetValue("ForegroundColorDynamic", out iconColor) || !(iconColor is Color))
            //    iconColor = Color.White;

            //return menuActions?
            //    .OrderBy(a => a.Sort)
            //    .Select
            //    (
            //        menuAction => (ToolbarItem) new IconToolbarItem
            //        {
            //            Text = menuAction.Title,
            //            AutomationId = menuAction.Title,
            //            Icon = menuAction.Icon,
            //            IconColor = (Color) iconColor,
            //            Command = new Command(async () => await menuAction.Execute(), menuAction.CanExecute)
            //        }
            //    )
            //    .ToList();
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
