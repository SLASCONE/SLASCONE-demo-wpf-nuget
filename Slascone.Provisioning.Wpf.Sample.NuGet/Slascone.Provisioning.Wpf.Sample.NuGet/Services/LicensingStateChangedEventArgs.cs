namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services;

public struct LicensingStateChangedEventArgs
{
	public LicensingState LicensingState { get; set; }
	public string LicensingStateDescription { get; set; }
}