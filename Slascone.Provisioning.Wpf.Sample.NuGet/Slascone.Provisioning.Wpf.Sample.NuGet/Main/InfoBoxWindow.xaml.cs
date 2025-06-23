using System.Windows;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Main
{
    /// <summary>
    /// Interaction logic for InfoBoxWindow.xaml
    /// </summary>
    public partial class InfoBoxWindow : Window
    {
        public InfoBoxWindow()
        {
            InitializeComponent();

            DataContextChanged += InfoBoxWindow_DataContextChanged;
        }

        private void InfoBoxWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is not InfoBoxViewModel vm)
                return;

            vm.PropertyChanged += InfoBoxViewModel_PropertyChanged;

            informationText.Inlines.Clear();
            informationText.Inlines.AddRange(vm.InfoLines);
        }

        private void InfoBoxViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InfoBoxViewModel.InfoLines))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    informationText.Inlines.Clear();
                    informationText.Inlines.AddRange(((InfoBoxViewModel)DataContext).InfoLines);
                });
            }
        }

        private void OnClickClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}