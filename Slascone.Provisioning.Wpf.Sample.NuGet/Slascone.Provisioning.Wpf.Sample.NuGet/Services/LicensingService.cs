using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;
using Slascone.Client;
using Slascone.Client.Interfaces;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services
{
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
		//https://support.slascone.com/hc/en-us/articles/360016063637-DIGITAL-SIGNATURE-AND-DATA-INTEGRITY
		//0 = none, 1 = symmetric, 2 = assymetric
		//use 0 for initial prototyping, 2 for production
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

		#endregion

		#region Construction

		public LicensingService()
		{
			InitializeLicensing();
		}

		#endregion

		#region Interface

		public string LicensingState { get; private set; }

		public bool NeedsActivation { get; private set; }

		public bool IsValid
			=> _licenseInfo is { Is_license_valid: true };
		
		public bool IsAssigned
			=> _licenseInfo?.Token_key != null;

		public DateTimeOffset? ExpirationDateUtc
			=> _licenseInfo?.Expiration_date_utc;

		// Licensee information
		public CustomerAccountDto? Customer
			=> _licenseInfo?.Customer;

		// Included features
		public IEnumerable<ProvisioningFeatureDto> Features
			=> _licenseInfo?.Features ?? Array.Empty<ProvisioningFeatureDto>();

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

		public string SoftwareVersion
			=> !string.IsNullOrEmpty(_softwareVersion)
				? _softwareVersion
				: _softwareVersion = Assembly.GetAssembly(typeof(LicensingService)).GetName().Version.ToString();

		public async Task RefreshLicenseInformationAsync()
		{
			var heartbeatDto = new AddHeartbeatDto
			{
				Product_id = _product_id,
				Client_id = DeviceId,
				Software_version = SoftwareVersion,
				Operating_system = OperatingSystem
			};

			NeedsActivation = false;

			_licenseInfo =
				await Execute(_slasconeClientV2.Provisioning.AddHeartbeatAsync,
					heartbeatDto,
					result =>
					{
						if (409 == result.StatusCode && 2006 == result.Error.Id)
						{
							NeedsActivation = true;
							LicensingState = "License needs activation.";
							return true;
						}

						return false;
					});
			
			if (null != _licenseInfo)
			{
				LicensingState = _licenseInfo.Is_license_valid
					? "License is valid."
					: "License is not valid";
			}
			else if (!NeedsActivation)
			{
				LicensingState = "License information refresh failed.";
			}

			// Notify clients
			LicensingStateChanged?.Invoke(this,
				new LicensingStateChangedEventArgs
				{
					LicensingState = LicensingState,
					NeedsActivation = NeedsActivation
				});
		}

		public async Task ActivateLicenseAsync(string licenseKey, bool useDemoLicenseKey)
		{
			var activateClientDto = new ActivateClientDto
			{
				Product_id = _product_id,
				License_key = useDemoLicenseKey ? _license_key : licenseKey,
				Client_id = DeviceId,
				Client_description = "",
				Client_name = "From GitHub WPF Nuget Sample",
				Software_version = SoftwareVersion
			};

			_licenseInfo = await Execute(_slasconeClientV2.Provisioning.ActivateLicenseAsync, activateClientDto);

			if (null != _licenseInfo)
			{
				NeedsActivation = false;
				LicensingState = _licenseInfo.Is_license_valid
					? "License is valid."
					: "License is not valid";
			}
			else
			{
				NeedsActivation = true;
				LicensingState = "License activation failed.";
			}

			// Notify clients
			LicensingStateChanged?.Invoke(this,
				new LicensingStateChangedEventArgs
				{
					LicensingState = LicensingState,
					NeedsActivation = NeedsActivation
				});
		}

		public async Task UnassignLicenseAsync()
		{
			if (!IsAssigned)
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
				LicensingState = result.Result;
			}
			else
			{
				LicensingState = "License unassignment failed.";
			}

			// Notify clients
			LicensingStateChanged?.Invoke(this,
				new LicensingStateChangedEventArgs
				{
					LicensingState = LicensingState,
					NeedsActivation = NeedsActivation
				});
		}

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
					.SetHttpClientTimeout(TimeSpan.FromMilliseconds(10000));
			
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

		#endregion
	}
}
