using System;
using System.Threading.Tasks;
using System.Windows;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Licensing
{
	/// <summary>
	/// Interaction logic for LimitationWindow.xaml
	/// </summary>
	public partial class LimitationWindow : Window
	{
		public LimitationWindow()
		{
			InitializeComponent();
		}
		
		private async void OnClickAddConsumption(object sender, RoutedEventArgs e)
		{
			try
			{
				// Add consumption
				if (DataContext is LimitationViewModel vm)
				{
					await vm.AddConsumptionAsync();
				}
			}
			catch (Exception _)
			{
			}
		}
	}
}
