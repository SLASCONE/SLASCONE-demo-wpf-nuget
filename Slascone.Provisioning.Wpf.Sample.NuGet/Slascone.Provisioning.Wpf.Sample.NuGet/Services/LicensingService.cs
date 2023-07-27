using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;
using Slascone.Client;
using Slascone.Client.Interfaces;
using Slascone.Client.Xml;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services
{
	public enum LicensingState
	{
		FullyValidated,
		OfflineValidated,
		TemporaryOfflineValidated,
		NeedsActivation,
		NeedsOfflineActivation,
		Invalid,
		Pending
	}

	internal class LicensingService
	{
		#region Main values - Fill according to your environment

		// Use this to connect to the Argus Demo 
		public const string ApiBaseUrl = "https://api.slascone.com";
		public static Guid IsvId = Guid.Parse("2af5fe02-6207-4214-946e-b00ac5309f53");
		public const string ProvisioningKey = "NfEpJ2DFfgczdYqOjvmlgP2O/4VlqmRHXNE9xDXbqZcOwXTbH3TFeBAKKbEzga7D7ashHxFtZOR142LYgKWdNocibDgN75/P58YNvUZafLdaie7eGwI/2gX/XuDPtqDW";

		private readonly string _license_key = "27180460-29df-4a5a-a0a1-78c85ab6cee0"; // Find your own license key at : https://my.slascone.com/licenses
		private readonly Guid _product_id = Guid.Parse("b18657cc-1f7c-43fa-e3a4-08da6fa41ad3"); // Find your own product id key at : https://my.slascone.com/products

		#endregion

		#region Encryption and Digital Signing

		// https://support.slascone.com/hc/en-us/articles/360016063637-DIGITAL-SIGNATURE-AND-DATA-INTEGRITY
		// 0 = none, 1 = symmetric, 2 = assymetric
		// use 0 for initial prototyping, 2 for production
		public const int SignatureValidationMode = 2;

		// CHANGE these values according to your environment at: https://my.slascone.com/administration/signature
		// You can work either with pem OR with xml
		public const string SignaturePubKeyPem = 
			@"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAwpigzm+cZIyw6x253YRD
mroGQyo0rO9qpOdbNAkE/FMSX+At5CQT/Cyr0eZTo2h+MO5gn5a6dwg2SYB/K1Yt
yuiKqnaEUfoPnG51KLrj8hi9LoZyIenfsQnxPz+r8XGCUPeS9MhBEVvT4ba0x9Ew
R+krU87VqfI3KNpFQVdLPaZxN4STTEZaet7nReeNtnnZFYaUt5XeNPB0b0rGfrps
y7drmZz81dlWoRcLrBRpkf6XrOTX4yFxe/3HJ8mpukuvdweUBFoQ0xOHmG9pNQ31
AHGtgLYGjbKcW4xYmpDGl0txfcipAr1zMj7X3oCO9lHcFRnXdzx+TTeJYxQX2XVb
hQIDAQAB
-----END PUBLIC KEY-----";

		public const string SignaturePublicKeyXml =
			@"<RSAKeyValue>
  <Modulus>wpigzm+cZIyw6x253YRDmroGQyo0rO9qpOdbNAkE/FMSX+At5CQT/Cyr0eZTo2h+MO5gn5a6dwg2SYB/K1YtyuiKqnaEUfoPnG51KLrj8hi9LoZyIenfsQnxPz+r8XGCUPeS9MhBEVvT4ba0x9EwR+krU87VqfI3KNpFQVdLPaZxN4STTEZaet7nReeNtnnZFYaUt5XeNPB0b0rGfrpsy7drmZz81dlWoRcLrBRpkf6XrOTX4yFxe/3HJ8mpukuvdweUBFoQ0xOHmG9pNQ31AHGtgLYGjbKcW4xYmpDGl0txfcipAr1zMj7X3oCO9lHcFRnXdzx+TTeJYxQX2XVbhQ==</Modulus>
  <Exponent>AQAB</Exponent>
</RSAKeyValue>";

		#endregion

		#region Fields

		private ISlasconeClientV2 _slasconeClientV2;
		private LicenseInfoDto _licenseInfo;
		private string _deviceId;
		private string _operatingSystem;
		private string _softwareVersion;

		private const string OfflineLicenseFileName = "LicenseFile.xml";
		private const string OfflineActivationFileName = "ActivationFile.xml";

		#endregion

		#region Construction

		public LicensingService()
		{
			InitializeLicensing();
		}

		#endregion

		#region Interface

		public LicensingState LicensingState { get; set; }

		public string LicensingStateDescription { get; private set; }

		/// <summary>
		/// License information: License key
		/// </summary>
		public string LicenseKey
			=> _licenseInfo?.License_key ?? string.Empty;

		/// <summary>
		/// License information: Token key
		/// </summary>
		public string TokenKey
			=> _licenseInfo?.Token_key?.ToString() ?? string.Empty;

		public string ProductName
			=> _licenseInfo?.Product_name ?? string.Empty;

		/// <summary>
		/// License information: Edition
		/// </summary>
		public string Edition
			=> _licenseInfo?.Template_name ?? string.Empty;

		/// <summary>
		/// License information: Expiration date
		/// </summary>
		public DateTimeOffset? ExpirationDateUtc
			=> _licenseInfo?.Expiration_date_utc;


		// License information: Created date
		public DateTimeOffset? CreatedDateUtc
			=> _licenseInfo?.Created_date_utc;

		// License information: Licensee
		public CustomerAccountDto? Customer
			=> _licenseInfo?.Customer;

		// License information: Included features
		public IEnumerable<ProvisioningFeatureDto> Features
			=> _licenseInfo?.Features ?? Array.Empty<ProvisioningFeatureDto>();

		// License information: Variables
		public IEnumerable<ProvisioningVariableDto> Variables
			=> _licenseInfo?.Variables ?? Array.Empty<ProvisioningVariableDto>();
		 
		// Computer information: Device ID
		public string DeviceId
			=> !string.IsNullOrEmpty(_deviceId)
				? _deviceId
				: _deviceId = Client.DeviceInfos.WindowsDeviceInfos.ComputerSystemProductId;

		// Computer Information: Operating System
		public string OperatingSystem =>
			!string.IsNullOrEmpty(_operatingSystem)
				? _operatingSystem
				: _operatingSystem = Client.DeviceInfos.WindowsDeviceInfos.OperatingSystem;

		// Software Information: Current Version
		public string SoftwareVersion
			=> !string.IsNullOrEmpty(_softwareVersion)
				? _softwareVersion
				: _softwareVersion = Assembly.GetAssembly(typeof(LicensingService)).GetName().Version.ToString();

		
		public async Task RefreshLicenseInformationAsync()
		{
			if (OfflineLicensing())
				return;

			var heartbeatDto = new AddHeartbeatDto
			{
				Product_id = _product_id,
				Client_id = DeviceId,
				Software_version = SoftwareVersion,
				Operating_system = OperatingSystem
			};

			SetLicensingState(LicensingState.Pending, "License validation pending ...");

			var licenseInfo =
				await Execute(_slasconeClientV2.Provisioning.AddHeartbeatAsync,
					heartbeatDto,
					result =>
					{
						if (409 == result.StatusCode && 2006 == result.Error.Id)
						{
							// License needs activation
							SetLicensingState(LicensingState.NeedsActivation, $"License heartbeat received an error: {result.Error.Message}");
							return true;
						}
						else if (400 == result.StatusCode)
						{
							return TemporaryOfflineFallback();
						}

						return false;
					});
			
			if (null != licenseInfo)
			{
				_licenseInfo = licenseInfo;
				SetLicensingState(
					_licenseInfo.Is_license_valid ? LicensingState.FullyValidated : LicensingState.Invalid,
					_licenseInfo.Is_license_valid 
						? BuildDescription(_licenseInfo)
						: "License is not valid");
			}
			else if (LicensingState.Pending == LicensingState)
			{
				SetLicensingState(LicensingState.Invalid, "License information refresh failed.");
			}
		}

		public async Task ActivateLicenseAsync(string licenseKey)
		{
			var activateClientDto = new ActivateClientDto
			{
				Product_id = _product_id,
				License_key = licenseKey,
				Client_id = DeviceId,
				Client_description = "",
				Client_name = "From GitHub WPF Nuget Sample",
				Software_version = SoftwareVersion
			};

			_licenseInfo = await Execute(_slasconeClientV2.Provisioning.ActivateLicenseAsync, activateClientDto);

			if (null != _licenseInfo)
			{
				SetLicensingState(
					_licenseInfo.Is_license_valid ? LicensingState.FullyValidated : LicensingState.Invalid,
					_licenseInfo.Is_license_valid 
						? BuildDescription(_licenseInfo)
						: "License is not valid");
			}
			else
			{
				SetLicensingState(LicensingState.NeedsActivation, "License activation failed.");
			}
		}

		public async Task UnassignLicenseAsync()
		{
			if (LicensingState.FullyValidated != LicensingState)
			{
				return;
			}

			var unassignDto = new UnassignDto
			{
				Token_key = _licenseInfo?.Token_key.Value ?? Guid.Empty
			};

			var result = await _slasconeClientV2.Provisioning.UnassignLicenseAsync(unassignDto);

			if (200 == result.StatusCode)
			{
				_licenseInfo = null;
				SetLicensingState(LicensingState.Invalid, result.Result);
			}
			else
			{
				SetLicensingState(LicensingState.FullyValidated, "License unassignment failed.");
			}
		}

		public async Task UploadLicenseFileAsync(string licenseFile)
		{
			File.Copy(licenseFile, Path.Combine(AppDataFolder, OfflineLicenseFileName), true);
			await RefreshLicenseInformationAsync();
		}

		public async Task UploadActivationFileAsync(string activationFile)
		{
			File.Copy(activationFile, Path.Combine(AppDataFolder, OfflineActivationFileName), true);
			await RefreshLicenseInformationAsync();
		}

		public void RemoveOfflineLicenseFiles()
		{
			var licenseFilePath = Path.Combine(AppDataFolder, OfflineLicenseFileName);
			var activationFilePath = Path.Combine(AppDataFolder, OfflineActivationFileName);

			// Delete files if existing
			if (File.Exists(licenseFilePath)) 
				File.Delete(licenseFilePath);
			if (File.Exists(activationFilePath))
				File.Delete(activationFilePath);
		}

		public void RemoveTemporaryOfflineLicenseFiles()
		{
			foreach (var file in new string[] { })
			{
				var filePath = Path.Combine(AppDataFolder, file);
				if (File.Exists(filePath))
					File.Delete(filePath);
			}
		}

		public string DemoLicenseKey
			=> _license_key;

		public ActivationFileRequest BuildActivationFileRequest()
			=> new()
			{
				ApiUrl = ApiBaseUrl,
				IsvId = IsvId,
				ProductId = _product_id,
				LicenseKey = _licenseInfo?.License_key ?? string.Empty,
				ClientId = DeviceId
			};

		// An event to notify the UI about the licensing state change
		public event EventHandler<LicensingStateChangedEventArgs> LicensingStateChanged;

		// An event to notify the UI about licensing errors
		public event EventHandler<LicensingErrorEventArgs> LicensingError;

		#endregion

		#region Implementation

		private void InitializeLicensing()
		{
			_slasconeClientV2 =
				SlasconeClientV2Factory.BuildClient(ApiBaseUrl, IsvId)
					.SetProvisioningKey(ProvisioningKey)
					.SetHttpClientTimeout(TimeSpan.FromMilliseconds(5000));

			
			_slasconeClientV2.SetAppDataFolder(AppDataFolder);

#if NET6_0_OR_GREATER

			// Importing a RSA key from a PEM encoded string is available in .NET 6.0 or later
			using (var rsa = RSA.Create())
			{
				rsa.ImportFromPem(SignaturePubKeyPem.ToCharArray());
				_slasconeClientV2
					.SetSignaturePublicKey(new PublicKey(rsa))
					.SetSignatureValidationMode(SignatureValidationMode);
			}
#else

			// If you are not using .NET 6.0 or later you have to load the public key from a xml string
			_slasconeClientV2.SetSignaturePublicKeyXml(SignaturePublicKeyXml);
			_slasconeClientV2.SetSignatureValidationMode(SignatureValidationMode);

#endif

			Task.Run(async () =>
			{
				// Get the current licensing state from the server sending a heartbeat
				await RefreshLicenseInformationAsync();
			});
		}

		private bool OfflineLicensing()
		{
			// Look for an offline license file
			LicenseInfoDto? licenseInfo = null;
			try
			{
				var licenseFilePath = Path.Combine(AppDataFolder, OfflineLicenseFileName);
				if (_slasconeClientV2.IsFileSignatureValid(licenseFilePath))
				{
					licenseInfo = _slasconeClientV2.ReadLicenseFile(licenseFilePath);

					// Check expiration date
					if (!licenseInfo.Expiration_date_utc.HasValue
					    || licenseInfo.Expiration_date_utc.Value < DateTime.UtcNow)
					{
						licenseInfo = null;
					}
				}
			}
			catch (Exception)
			{
				// Ignore
			}

			if (null == licenseInfo)
				return false;

			_licenseInfo = licenseInfo;

			// Look for an activation file
			ActivationDto? activation = null;
			try
			{
				var activationFilePath = Path.Combine(AppDataFolder, OfflineActivationFileName);
				if (_slasconeClientV2.IsFileSignatureValid(activationFilePath))
				{
					activation = _slasconeClientV2.ReadActivationFile(activationFilePath);

					// Check if license IDs and Client ID and Device ID match
					if (activation.License_key != licenseInfo.License_key
					    || activation.Client_id != DeviceId)
					{
						activation = null;
					}
				}
			}
			catch (Exception)
			{
				// Ignore
			}

			if (null != activation)
			{
				SetLicensingState(LicensingState.OfflineValidated,
					$"{BuildDescription(_licenseInfo)} (offline license)");
			}
			else
			{
				SetLicensingState(LicensingState.NeedsOfflineActivation,
					$"{BuildDescription(_licenseInfo)} (offline license, needs activation)");
			}

			return true;
		}

		private bool TemporaryOfflineFallback()
		{
			var response = _slasconeClientV2.GetOfflineLicense();

			if (response.StatusCode != 200) 
				return false;

			var licenseInfo = response.Result;

			// Check if Client ID and Device ID match
			if (licenseInfo.Client_id != DeviceId) 
				return false;

			// Check how old the stored license info is
			if (!licenseInfo.Created_date_utc.HasValue) 
				return false;
			
			var licenseInfoAge = (DateTime.Now - licenseInfo.Created_date_utc.Value).Days;

			// Check for freeride period
			if (0 > licenseInfoAge || !licenseInfo.Freeride.HasValue) 
				return false;
			
			if (licenseInfoAge <= licenseInfo.Freeride.Value)
			{
				_licenseInfo = licenseInfo;

				// Check expiration date
				if (licenseInfo.Expiration_date_utc.HasValue)
				{
					var expirationDate = licenseInfo.Expiration_date_utc.Value;

					if (expirationDate < DateTime.UtcNow)
					{
						_licenseInfo.Is_license_expired = true;
						_licenseInfo.Is_license_valid = false;
						SetLicensingState(LicensingState.Invalid,
							$"License expired on {expirationDate:d}.");
						return true;
					}
				}

				SetLicensingState(LicensingState.TemporaryOfflineValidated,
					$"{BuildDescription(_licenseInfo)} (temporary offline)");
			}
			else
			{
				SetLicensingState(LicensingState.Invalid,
					$"Freeride period of {licenseInfo.Freeride.Value} days exceeded.");
			}

			return true;

		}

		private void SetLicensingState(LicensingState licensingState, string description)
		{
			LicensingState = licensingState;
			LicensingStateDescription = description;

			// Notify clients
			LicensingStateChanged?.Invoke(this,
				new LicensingStateChangedEventArgs
				{
					LicensingState = licensingState,
					LicensingStateDescription = description
				});
		}

		private static string BuildDescription(LicenseInfoDto licenseInfo) 
			=> $"Product licensed for {licenseInfo.Customer.Company_name}. Expires on {licenseInfo.Expiration_date_utc:d}";

		private static string AppDataFolder { get; }
			= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
				Assembly.GetExecutingAssembly().GetName().Name);

		#endregion

		#region Error handling helper

		private async Task<TOut> Execute<TIn, TOut>(Func<TIn, Task<ApiResponse<TOut>>> func, TIn argument, [CallerMemberName] string callerMemberName = "")
			where TOut : class
		{
			return await Execute(func, argument, result => false, callerMemberName);
		}

		private async Task<TOut> Execute<TIn, TOut>(Func<TIn, Task<ApiResponse<TOut>>> func, TIn argument, Func<ApiResponse<TOut>, bool> interceptor, [CallerMemberName] string callerMemberName = "")
			where TOut : class
		{
			try
			{
				var result = await func.Invoke(argument).ConfigureAwait(false);

				if (200 == result.StatusCode)
				{
					return result.Result;
				}

				if (interceptor.Invoke(result))
					return null;

				if (409 == result.StatusCode)
				{
					LicensingError?.Invoke(this,
						new LicensingErrorEventArgs
						{
							Message = $"SLASCONE error {result.Error?.Id}: {result.Error?.Message}",
							MessageBoxImage = MessageBoxImage.Exclamation
						});
				}
				else
				{
					LicensingError?.Invoke(this,
						new LicensingErrorEventArgs
						{
							Message = $"SLASCONE error {result.StatusCode}: {result.Message}",
							MessageBoxImage = MessageBoxImage.Error
						});
				}
			}
			catch (Exception ex)
			{
				LicensingError?.Invoke(this,
					new LicensingErrorEventArgs
					{
						Message = $"{callerMemberName} threw an exception:{Environment.NewLine}{ex.Message}",
						MessageBoxImage = MessageBoxImage.Error
					});
			}

			return null;
		}

		#endregion
	}

	public struct ActivationFileRequest
	{
		public string ApiUrl { get; set; }
		public Guid IsvId { get; set; }
		public Guid ProductId { get; set; }
		public string LicenseKey { get; set; }
		public string ClientId { get; set; }
	}
}
