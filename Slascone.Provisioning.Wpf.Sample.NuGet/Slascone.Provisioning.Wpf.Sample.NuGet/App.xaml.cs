using Microsoft.Extensions.DependencyInjection;
using Slascone.Provisioning.Wpf.Sample.NuGet.Licensing;
using Slascone.Provisioning.Wpf.Sample.NuGet.Services;
using System;
using System.Windows;

namespace Slascone.Provisioning.Wpf.Sample.NuGet
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

            // Service registration for dependency injection
            var services = new ServiceCollection();
			services
                .AddSingleton<SlasconeClientConfiguration>()
                .AddSingleton<AuthenticationServiceConfiguration>()
                .AddSingleton<AuthenticationService>()
                .AddSingleton<LicenseManagerViewModel>()
                .AddSingleton<LicensingService>();

            services.AddHttpClient("Slascone.Client");

            ServiceProvider = services.BuildServiceProvider();
		}
	}
}
