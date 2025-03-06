using Microsoft.Extensions.Configuration;
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
    public class AuthenticationService
    {
		#region Attributes

		private IPublicClientApplication? _publicClientApp;
		private readonly IConfiguration _configuration;
		private readonly string _authoritySignUpSignIn;
		private readonly string _authorityResetPassword;
		private readonly string[] _apiScopes;

		#endregion

		#region Construction

		public AuthenticationService(IConfiguration configuration)
		{
			_configuration = configuration;

			var authorityBase = $"https://{_configuration["AzureAdB2C:Hostname"]}/tfp/{_configuration["AzureAdB2C:TenantName"]}.onmicrosoft.com/";
			_authoritySignUpSignIn = $"{authorityBase}{_configuration["AzureAdB2C:SignUpSignInPolicyId"]}";
			_authorityResetPassword = $"{authorityBase}{_configuration["AzureAdB2C:ResetPasswordPolicyId"]}";

			_apiScopes = _configuration.GetSection("AzureAdB2C:ApiScopes")
				.GetChildren()
				.Select(c => c.Value ?? string.Empty)
				.Where(s => !string.IsNullOrEmpty(s))
				.ToArray();
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
					authResult = await app.AcquireTokenInteractive(_apiScopes)
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

					/*
+		[00]	{[exp, {exp: 1741120586}]}	System.Collections.Generic.KeyValuePair<string, System.Security.Claims.Claim>
+		[01]	{[nbf, {nbf: 1741116986}]}	System.Collections.Generic.KeyValuePair<string, System.Security.Claims.Claim>
+		[02]	{[ver, {ver: 1.0}]}	System.Collections.Generic.KeyValuePair<string, System.Security.Claims.Claim>
+		[03]	{[iss, {iss: https://slascone.b2clogin.com/222c87b5-9c9c-48d2-9f10-55d5ab430cfd/v2.0/}]}	System.Collections.Generic.KeyValuePair<string, System.Security.Claims.Claim>
+		[04]	{[sub, {sub: 72e19384-1527-4a59-baa0-b5d5ea6c81ad}]}	System.Collections.Generic.KeyValuePair<string, System.Security.Claims.Claim>
+		[05]	{[aud, {aud: 4fee85d0-5396-4325-bd8d-4f3c835b83f7}]}	System.Collections.Generic.KeyValuePair<string, System.Security.Claims.Claim>
+		[06]	{[iat, {iat: 1741116986}]}	System.Collections.Generic.KeyValuePair<string, System.Security.Claims.Claim>
+		[07]	{[auth_time, {auth_time: 1741116985}]}	System.Collections.Generic.KeyValuePair<string, System.Security.Claims.Claim>
+		[08]	{[oid, {oid: 72e19384-1527-4a59-baa0-b5d5ea6c81ad}]}	System.Collections.Generic.KeyValuePair<string, System.Security.Claims.Claim>
+		[09]	{[name, {name: Michael Dürr - DEV}]}	System.Collections.Generic.KeyValuePair<string, System.Security.Claims.Claim>
+		[10]	{[given_name, {given_name: Michael}]}	System.Collections.Generic.KeyValuePair<string, System.Security.Claims.Claim>
+		[11]	{[family_name, {family_name: Dürr}]}	System.Collections.Generic.KeyValuePair<string, System.Security.Claims.Claim>
+		[12]	{[emails, {emails: michael.duerr@whiteduck.de}]}	System.Collections.Generic.KeyValuePair<string, System.Security.Claims.Claim>
+		[13]	{[tfp, {tfp: B2C_1_signinup}]}	System.Collections.Generic.KeyValuePair<string, System.Security.Claims.Claim>					
					 */

					var accounts = await app.GetAccountsAsync("B2C_1_signinup");
					authResult = await app.AcquireTokenSilent(_apiScopes, accounts.FirstOrDefault()).ExecuteAsync();
					BearerToken = authResult.AccessToken;

					IsSignedIn = true;
				}
				catch (MsalException msalException)
				{
					try
					{
						if (msalException.Message.Contains("AADB2C90118"))
						{
							authResult = await app.AcquireTokenInteractive(_apiScopes)
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
			=> _publicClientApp ??= PublicClientApplicationBuilder.Create(_configuration["AzureAdB2C:ClientId"])
				.WithB2CAuthority(_authoritySignUpSignIn)
				.WithRedirectUri(_configuration["AzureAdB2C:RedirectUri"])
				.WithWindowsEmbeddedBrowserSupport()
				.Build();

		#endregion

    }
}
