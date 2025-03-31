using Microsoft.Identity.Client;
using System.Windows.Interop;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Identity.Client.Desktop;
using System.Collections.Generic;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services
{
	/// <summary>
	/// Authentication service to handle sign-in and sign-out.
	/// </summary>
	/// <remarks>
	/// The AuthenticationService class is used to handle the sign-in and sign-out process
	/// with Azure Active Directory B2C as identity provider. It uses the Microsoft Identity Client.
	/// </remarks>
	public class AuthenticationService
    {
		#region Attributes

		private IPublicClientApplication? _publicClientApp;
		private readonly AuthenticationServiceConfiguration _configuration;
		private readonly string _authoritySignUpSignIn;
		private readonly string _authorityResetPassword;

		#endregion

		#region Construction

		public AuthenticationService(AuthenticationServiceConfiguration configuration)
		{
			_configuration = configuration;

			var authorityBase = $"https://{_configuration.Hostname}/tfp/{_configuration.TenantName}.onmicrosoft.com/";
			_authoritySignUpSignIn = $"{authorityBase}{_configuration.SignUpSignInPolicyId}";
			_authorityResetPassword = $"{authorityBase}{_configuration.ResetPasswordPolicyId}";
		}

		#endregion

		#region Interface

		public bool IsSignedIn { get; private set; }
		public string Name { get; private set; }
		public string Email { get; private set; }
		public string BearerToken { get; private set; }
		public string ErrorMessage { get; private set; }

		// Define the event delegate
		public delegate void LoginStateChangedEventHandler(object sender, EventArgs e);

		// Define the event
		public event LoginStateChangedEventHandler LoginStateChanged;

		public async Task SignInAsync()
		{
			// Execute in main thread
			await Application.Current.Dispatcher.InvokeAsync(async () =>
			{
				AuthenticationResult authResult = null;
				var app = PublicClientApp;
				try
				{
					authResult = await app.AcquireTokenInteractive(_configuration.ApiScopes)
						.WithParentActivityOrWindow(new WindowInteropHelper(App.Current.MainWindow).Handle)
						.ExecuteAsync();

					var claims = authResult.ClaimsPrincipal.Claims.ToDictionary(c => c.Type);
					if (claims.TryGetValue("name", out var name))
					{
						Name = name.Value;
					}
					if (claims.TryGetValue("emails", out var mails))
					{
						Email = mails.Value;
					}

					var accounts = await app.GetAccountsAsync("B2C_1_signinup");
					authResult = await app.AcquireTokenSilent(_configuration.ApiScopes, accounts.FirstOrDefault()).ExecuteAsync();
					BearerToken = authResult.AccessToken;

					IsSignedIn = true;
				}
				catch (MsalException msalException)
				{
					try
					{
						if (msalException.Message.Contains("AADB2C90118"))
						{
							authResult = await app.AcquireTokenInteractive(_configuration.ApiScopes)
								.WithParentActivityOrWindow(new WindowInteropHelper(App.Current.MainWindow).Handle)
								.WithPrompt(Prompt.SelectAccount)
								.WithB2CAuthority(_authorityResetPassword)
								.ExecuteAsync();
						}
						else
						{
							IsSignedIn = false;
							ErrorMessage = $"Error Acquiring Token: {msalException.Message}";
						}
					}
					catch (Exception e)
					{
						IsSignedIn = false;
						ErrorMessage = $"Error Acquiring Token: {e.Message}";
					}
				}
				catch (Exception ex)
				{
					IsSignedIn = false;
					ErrorMessage = $"Error Acquiring Token: {ex.Message}";
				}

				// Trigger login state changed event
				LoginStateChanged?.Invoke(this, new EventArgs());
			});
		}

		public async Task SignOutAsync()
		{
			// SingOut will remove tokens from the token cache from ALL accounts, irrespective of user flow
			IEnumerable<IAccount> accounts = await PublicClientApp.GetAccountsAsync();
			foreach (var account in accounts)
			{
				try
				{
					await PublicClientApp.RemoveAsync(account);
				}
				catch (Exception _)
				{
				}
			}

			_publicClientApp = null;
			IsSignedIn = false;
			Name = string.Empty;
			Email = string.Empty;
			BearerToken = string.Empty;
			LoginStateChanged?.Invoke(this, new EventArgs());
		}

		#endregion

		#region Implementation

		private IPublicClientApplication PublicClientApp
			=> _publicClientApp ??= PublicClientApplicationBuilder.Create(_configuration.ClientId)
				.WithB2CAuthority(_authoritySignUpSignIn)
				.WithRedirectUri(_configuration.RedirectUri)
				.WithWindowsEmbeddedBrowserSupport()
				.Build();

		#endregion

    }
}
