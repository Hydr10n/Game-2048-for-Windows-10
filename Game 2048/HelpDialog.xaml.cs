using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace Game_2048
{
    public sealed partial class HelpDialog : ContentDialog
    {
        public HelpDialog() => InitializeComponent();

        private void Hyperlink_Click(object sender, RoutedEventArgs e) => new WebView().Navigate((sender as Hyperlink).NavigateUri);
    }
}