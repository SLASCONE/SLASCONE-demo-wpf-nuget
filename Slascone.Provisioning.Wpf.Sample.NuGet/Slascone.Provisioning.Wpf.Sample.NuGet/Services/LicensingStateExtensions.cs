namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services;

public static class LicensingStateExtensions
{
	public static bool IsOnlineState(this LicensingState state)
		=> state switch
		{
			LicensingState.FullyValidated => true,
			LicensingState.NeedsActivation => true,
			LicensingState.TemporaryOfflineValidated => true,
			LicensingState.FloatingLimitExceeded => true,
			LicensingState.NotSignedIn => true,
			LicensingState.SessionOpenFailed => true,
			LicensingState.Invalid => true,
			_ => false
		};

	public static bool IsOfflineState(this LicensingState state)
		=> state switch
		{
			LicensingState.OfflineValidated => true,
			LicensingState.NeedsOfflineActivation => true,
			LicensingState.LicenseFileMissing => true,
			LicensingState.LicenseFileInvalid => true,
			_ => false
		};
}