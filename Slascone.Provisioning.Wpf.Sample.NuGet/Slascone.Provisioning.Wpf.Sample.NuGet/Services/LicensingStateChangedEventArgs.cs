namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services;

internal struct LicensingStateChangedEventArgs
{
	public LicensingState LicensingState { get; set; }
	public string LicensingStateDescription { get; set; }
}