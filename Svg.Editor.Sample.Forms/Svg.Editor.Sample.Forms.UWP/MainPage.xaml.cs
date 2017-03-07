using Svg.Editor.Forms;

namespace Svg.Editor.Sample.Forms.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            SvgPlatform.Init();
            SvgEditorForms.Init();

            LoadApplication(new Forms.App());
        }
    }
}
