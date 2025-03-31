using System.IO;
using System.Text.Json;
using Slascone.Client;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services;

/// <summary>
/// Preserve the user choice of the client type.
/// </summary>
/// <remarks>
/// The LicensingServiceData class is used to store the user choice of the client type.
/// In a real-world application, the client mode would depend on the kind of application or
/// installation. But in this sample, the user can choose between the two client types.
/// The choice is stored in a JSON file in the application directory so it can be restored at
/// the next start.
/// Find more information about licensing models here:
/// https://support.slascone.com/hc/en-us/sections/360004693117-Licensing-Models
/// </remarks>
internal class LicensingServiceData
{
	#region Const

	private const string FileName = "LicensingServiceData.json";

	#endregion

	#region Interface

	public ClientType ClientType { get; set; } = ClientType.Devices;

	public void Save(string path)
	{
		var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
		var filePath = Path.Combine(path, FileName);

		File.WriteAllText(filePath, json);
	}

	public void Load(string path)
	{
		var filePath = Path.Combine(path, FileName);

		if (File.Exists(filePath))
		{
			var json = File.ReadAllText(filePath);
			var licensingData = JsonSerializer.Deserialize<LicensingServiceData>(json);

			if (licensingData != null)
			{
				ClientType = licensingData.ClientType;
			}
		}
	}

	#endregion
}