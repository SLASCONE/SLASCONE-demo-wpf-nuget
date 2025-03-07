namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services;

public enum LicensingState
{
	// Received valid License Info from a license heartbeat and opened valid session if floating license
	FullyValidated,

	// Valid license file and activation file exists
	OfflineValidated,

	// Valid License Info from a saved heartbeat response
	TemporaryOfflineValidated,

	// Heartbeat sent error "Unknown client"; needs license activation for device
	NeedsActivation,

	// Open session failed
	SessionOpenFailed,

	// Received valid License Info from a license heartbeat (floating license) but could not open a session
	FloatingLimitExceeded,
		
	// Valid license file exists, but activation file is missing
	NeedsOfflineActivation,
		
	// License validation failed (online mode)
	Invalid,

	// No user signed in
	NotSignedIn,

	// No license file exists (offline mode)
	LicenseFileMissing,

	// License file or activation file is invalid (offline mode)
	LicenseFileInvalid,
		
	// Online license validation is pending
	Pending
}