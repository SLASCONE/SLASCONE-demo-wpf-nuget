namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services;

public class AuthenticationServiceConfiguration
{
	// The name of your Azure AD B2C tenant. This is typically your company or organization's name.
	public string TenantName => "slascone";

	// The unique identifier of the application registered in Azure AD B2C. This GUID is used to identify your app within the B2C directory.
	public string ClientId => "4fee85d0-5396-4325-bd8d-4f3c835b83f7";

	// The hostname used to access your Azure AD B2C tenant's login services.
	public string Hostname => "slascone.b2clogin.com";

	// The policy ID for the sign-up and sign-in user flow. This value corresponds to the policy you configured in Azure AD B2C for user authentication.
	public string SignUpSignInPolicyId => "B2C_1_signinup";

	// The policy ID for the password reset user flow. This value corresponds to the policy you configured in Azure AD B2C for users to reset their passwords.
	public string ResetPasswordPolicyId => "B2C_1_pwdreset";

	// The URI to which Azure AD B2C will redirect users after they have authenticated or completed a user flow.
	public string RedirectUri => "https://slascone.b2clogin.com/tfp/oauth2/nativeclient";

	// A list of API scopes that the application will request access to. Scopes represent the permissions your app needs to perform certain actions on behalf of the user.
	public string[] ApiScopes => ["https://slascone.onmicrosoft.com/f52b8ff4-0a90-47dd-b35c-2c86487b3ed6/user_impersonation"];
}