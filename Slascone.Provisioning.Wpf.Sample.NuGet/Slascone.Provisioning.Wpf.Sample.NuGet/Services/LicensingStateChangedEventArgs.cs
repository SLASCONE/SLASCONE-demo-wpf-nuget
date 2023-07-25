namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services;

internal struct LicensingStateChangedEventArgs
{
	public bool NeedsActivation { get; set; }
	public string LicensingState { get; set; }
}