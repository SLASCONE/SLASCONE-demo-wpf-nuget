using System;
using System.Diagnostics;
using System.Windows;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Licensing
{
    /// <summary>
    /// Interaction logic for ActivationFile.xaml
    /// </summary>
    public partial class ActivationFileWindow : Window
    {
        public ActivationFileWindow()
        {
            InitializeComponent();
        }

        private void OnClickClose(object sender, RoutedEventArgs e)
        {
	        this.Close();
        }

        private void OnClickLink(object sender, RoutedEventArgs e)
        {
	        if (DataContext is not ActivationFileViewModel vm)
                return;

	        Process.Start(new ProcessStartInfo(vm.ActivationFileDownloadUrl)
	        {
		        UseShellExecute = true
	        });
        }

        private void OnClickCopy(object sender, RoutedEventArgs e)
        {
	        if (DataContext is not ActivationFileViewModel vm)
		        return;

            // Copy request URL to clipboard
            Clipboard.SetText(vm.ActivationFileDownloadUrl);
		}
	}
}
