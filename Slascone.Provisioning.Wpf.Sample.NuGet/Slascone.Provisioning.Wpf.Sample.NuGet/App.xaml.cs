using System;
using System.Windows;
using Slascone.Provisioning.Wpf.Sample.NuGet.Services;

namespace Slascone.Provisioning.Wpf.Sample.NuGet
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static AuthenticationService AuthenticationService { get; private set; }

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			AuthenticationService = new AuthenticationService(new AuthenticationServiceConfiguration());
		}
	}
}
