using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Slascone.Client;
using Slascone.Client.Interfaces;
using Slascone.Client.Xml;
using Slascone.Provisioning.Wpf.Sample.NuGet.Services.SimulateNoInternet;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services
{
	internal class LicensingService : IDisposable
	{
		#region Main values - Fill according to your environment

		// Use this to connect to the Argus Demo 
		public const string ApiBaseUrl = "https://api.slascone.com"; // Find your own baseUrl at : https://my.slascone.com/administration/apikeys
        public static Guid IsvId = Guid.Parse("2af5fe02-6207-4214-946e-b00ac5309f53"); // Find your own Isv Id at : https://my.slascone.com/administration/apikeys

        public const string ProvisioningKey = "NfEpJ2DFfgczdYqOjvmlgP2O/4VlqmRHXNE9xDXbqZcOwXTbH3TFeBAKKbEzga7D7ashHxFtZOR142LYgKWdNocibDgN75/P58YNvUZafLdaie7eGwI/2gX/XuDPtqDW"; // Find your own product key(s) at : https://my.slascone.com/administration/apikeys

		private readonly Guid _product_id = Guid.Parse("b18657cc-1f7c-43fa-e3a4-08da6fa41ad3"); // Find your own product id key at : https://my.slascone.com/products

		private readonly string _license_key = "27180460-29df-4a5a-a0a1-78c85ab6cee0"; // Just for demo, do not change this

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

		private ISlasconeClientV2? _slasconeClientV2;
		private SessionManager? _sessionManager;
		private readonly AuthenticationService _authenticationService;

		private LicenseInfoDto _licenseInfo;
		private LicensingServiceData _licensingServiceData = new LicensingServiceData();
		
		private string _deviceId;
		private string _operatingSystem;
		private string _softwareVersion;

		private const string OfflineLicenseFileName = "LicenseFile.xml";
		private const string OfflineActivationFileName = "ActivationFile.xml";

		#endregion

		#region Construction

		public LicensingService(AuthenticationService authenticationService)
		{
			_authenticationService = authenticationService;
			_authenticationService.LoginStateChanged += AuthenticationServiceLoginStateChanged;

			_licensingServiceData.Load(AppDataFolder);
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
		/// License information: license type
		/// </summary>
		public string LicenseType
			=> _licenseInfo?.License_type?.Name ?? string.Empty;

		/// <summary>
		/// License information: Expiration date
		/// </summary>
		public DateTimeOffset? ExpirationDateUtc
			=> _licenseInfo?.Expiration_date_utc;

		/// <summary>
		/// License information: Freeride granted
		/// </summary>
		public string FreerideGranted
			=> _licenseInfo?.Freeride.HasValue ?? false
				? $"Freeride granted: {_licenseInfo?.Freeride.Value} days"
				: "No freeride granted";

		/// <summary>
		/// License information: Remaining freeride period
		/// </summary>
		public TimeSpan? RemainingFreeride
		{
			get
			{
				if (!_licenseInfo.Freeride.HasValue)
					return TimeSpan.Zero;

				var licenseInfoAge = (DateTime.Now - CreatedDateUtc.Value).Days;
				return TimeSpan.FromDays((_licenseInfo.Freeride.Value - licenseInfoAge));
			}
		}

		/// <summary>
		/// License information: End of freeride period if granted
		/// </summary>
		public DateTimeOffset? FreerideExpirationDate
			=> _licenseInfo.Freeride.HasValue
				? _licenseInfo?.Created_date_utc + TimeSpan.FromDays(_licenseInfo.Freeride.Value)
				: null;

		/// <summary>
		/// License information: Created date
		/// </summary>
		public DateTimeOffset? CreatedDateUtc
			=> _licenseInfo?.Created_date_utc;

		/// <summary>
		/// License information: Licensee
		/// </summary>
		public CustomerAccountDto? Customer
			=> _licenseInfo?.Customer;

		/// <summary>
		/// License information: Included features
		/// </summary>
		public IEnumerable<ProvisioningFeatureDto> Features
			=> _licenseInfo?.Features ?? Array.Empty<ProvisioningFeatureDto>();

		/// <summary>
		/// License information: Limitations
		/// </summary>
		public IEnumerable<ProvisioningLimitationDto> Limitations
			=> _licenseInfo?.Limitations ?? Array.Empty<ProvisioningLimitationDto>();

		/// <summary>
		/// License information: Variables
		/// </summary>
		public IEnumerable<ProvisioningVariableDto> Variables
			=> _licenseInfo?.Variables ?? Array.Empty<ProvisioningVariableDto>();

		/// <summary>
		/// License information: Constrained variables
		/// </summary>
		public IEnumerable<ProvisioningConstrainedVariableDto> ConstrainedVariables
			=> _licenseInfo?.Constrained_variables ?? Array.Empty<ProvisioningConstrainedVariableDto>();

		/// <summary>
		/// Computer information: Device ID
		/// </summary>
		public string DeviceId
			=> !string.IsNullOrEmpty(_deviceId)
				? _deviceId
				: _deviceId = Client.DeviceInfos.WindowsDeviceInfos.ComputerSystemProductId;

		/// <summary>
		/// Computer Information: Operating System
		/// </summary>
		public string OperatingSystem =>
			!string.IsNullOrEmpty(_operatingSystem)
				? _operatingSystem
				: _operatingSystem = Client.DeviceInfos.WindowsDeviceInfos.OperatingSystem;

		/// <summary>
		/// Software Information: Current Version
		/// </summary>
		public string SoftwareVersion
			=> !string.IsNullOrEmpty(_softwareVersion)
				? _softwareVersion
				: _softwareVersion = Assembly.GetAssembly(typeof(LicensingService)).GetName().Version.ToString();

		/// <summary>
		/// Provisioning mode (floating/named)
		/// </summary>
		public ProvisioningMode? ProvisioningMode
			=> _licenseInfo?.Provisioning_mode;

		/// <summary>
		/// Client type (user/device)
		/// </summary>
		public ClientType ClientType
			=> _licenseInfo?.Client_type ?? _licensingServiceData.ClientType;

		/// <summary>
		/// Session period
		/// </summary>
		public int? SessionPeriod
			=> _licenseInfo?.Session_period;

		public Guid? SessionId
			=> _sessionManager?.SessionId;

		public DateTimeOffset? SessionValidUntil
			=> _sessionManager?.SessionValidUntil;

		public DateTimeOffset? SessionCreated
			=> _sessionManager?.SessionCreated;

		public DateTimeOffset? SessionModified
			=> _sessionManager?.SessionModified;

		public string SessionDescription
			=> _sessionManager?.SessionDescription ?? string.Empty;

		/// <summary>
		/// Refresh license information looking for license files or by sending a license heartbeat
		/// </summary>
		/// <returns></returns>
		public async Task RefreshLicenseInformationAsync()
		{
			_licenseInfo = null;

			if (OfflineLicensing())
				return;

			SetLicensingState(LicensingState.Pending, "License validation pending ...");

			if (ClientType.Devices == ClientType)
			{
				await RefreshLicenseInfoClientTypeDevicesAsync();
			}
			else if (ClientType.Users == ClientType)
			{
				await RefreshLicenseInfoClientTypeUsersAsync();
			}
		}

		/// <summary>
		/// Activates a license
		/// </summary>
		/// <param name="licenseKey"></param>
		/// <returns></returns>
		public async Task ActivateLicenseAsync(string licenseKey, string? clientId = null)
		{
			var activateClientDto = new ActivateClientDto
			{
				Product_id = _product_id,
				License_key = licenseKey,
				Client_id = clientId ?? DeviceId,
				Client_description = "",
				Client_name = "From GitHub WPF Nuget Sample",
				Software_version = SoftwareVersion
			};

			string? errorMessage;
			(_licenseInfo, errorMessage) = 
				await ErrorHandlingHelper.Execute(SlasconeClientV2.Provisioning.ActivateLicenseAsync, activateClientDto).ConfigureAwait(false);

			if (null == _licenseInfo)
			{
				SetLicensingState(LicensingState.NeedsActivation, "License activation failed.");
				return;
			}
			
			SetLicensingState(
					_licenseInfo.Is_license_valid && _licenseInfo.Is_software_version_valid
						? LicensingState.FullyValidated 
						: LicensingState.Invalid,
					_licenseInfo.Is_license_valid
						? BuildDescription(_licenseInfo, LicensingState)
						: _licenseInfo.Is_license_expired
							? "License is expired"
							: !_licenseInfo.Is_license_active
								? "License is not active"
								: !_licenseInfo.Is_software_version_valid
									? "License is not valid for this software version"
									: !_licenseInfo.Is_software_version_valid
										? "License is not valid for this software version"
										: "License is not valid");

			await HandleProvisioningMode();
		}

		public async Task UnassignLicenseAsync()
		{
			if (!((bool)_licenseInfo?.Token_key.HasValue))
			{
				return;
			}

			var unassignDto = new UnassignDto
			{
				Token_key = _licenseInfo?.Token_key.Value ?? Guid.Empty
			};

			var result = await SlasconeClientV2.Provisioning.UnassignLicenseAsync(unassignDto).ConfigureAwait(false);

			if (200 == result.StatusCode)
			{
				_licenseInfo = null;
				SetLicensingState(LicensingState.NeedsActivation, result.Result);
			}
			else
			{
				SetLicensingState(LicensingState.FullyValidated, "License unassignment failed.");
			}
		}

		public async Task UploadLicenseFileAsync(string licenseFile)
		{
			File.Copy(licenseFile, Path.Combine(AppDataFolder, OfflineLicenseFileName), true);
			
			var activationFilePath = Path.Combine(AppDataFolder, OfflineActivationFileName);
			if (File.Exists(activationFilePath))
				File.Delete(activationFilePath);

			await RefreshLicenseInformationAsync();
		}

		public async Task UploadActivationFileAsync(string activationFile)
		{
			var activationFilePath = Path.Combine(AppDataFolder, OfflineActivationFileName);
			File.Copy(activationFile, activationFilePath, true);

			await RefreshLicenseInformationAsync();

			// Delete activation immediately if license is not valid
			if (LicensingState.OfflineValidated != LicensingState)
				File.Delete(activationFilePath);
		}

		public async Task SwitchToOnlineLicensingModeAsync()
		{
			RemoveOfflineLicenseFiles();

			_licensingServiceData.ClientType = Client.ClientType.Devices;
			_licensingServiceData.Save(AppDataFolder);

			await RefreshLicenseInformationAsync().ConfigureAwait(false);
		}

		public async Task SwitchToOfflineLicensingModeAsync()
		{
			RemoveTemporaryOfflineLicenseFiles();

			_licensingServiceData.ClientType = Client.ClientType.Devices;
			_licensingServiceData.Save(AppDataFolder);

			if (LicensingState.FullyValidated == LicensingState)
				await UnassignLicenseAsync();

			_licenseInfo = null;

			SetLicensingState(LicensingState.LicenseFileMissing, "No license file");
		}

		public async Task SwitchToClientTypeUserAsync()
		{
			RemoveTemporaryOfflineLicenseFiles();

			_licensingServiceData.ClientType = Client.ClientType.Users;
			_licensingServiceData.Save(AppDataFolder);

			if (LicensingState.FullyValidated == LicensingState)
				await UnassignLicenseAsync();

			_licenseInfo = null;

			await RefreshLicenseInformationAsync().ConfigureAwait(false);
		}

		public async Task SignOutUserAsync()
		{
			_licenseInfo = null;

			await _authenticationService.SignOutAsync();
		}

		public string DemoLicenseKey
			=> _license_key;

		public Uri BuildActivationFileRequest()
		{
			var urlBuilder = new StringBuilder();
			urlBuilder.Append(ApiBaseUrl != null ? ApiBaseUrl.TrimEnd('/') : "")
				.Append($"/api/v2/isv/{IsvId}/provisioning/activations/offline?")
				.Append($"product_id={Uri.EscapeDataString(_product_id.ToString())}&")
				.Append($"license_key={Uri.EscapeDataString(_licenseInfo?.License_key ?? string.Empty)}&")
                .Append($"file_name={Uri.EscapeDataString("LicenseActivation")}&")
                .Append($"client_id={Uri.EscapeDataString(DeviceId)}");

			return new Uri(urlBuilder.ToString());
		}

		// An event to notify the UI about the licensing state change
		public event EventHandler<LicensingStateChangedEventArgs> LicensingStateChanged;

		#endregion

		#region Implementation

		private ISlasconeClientV2 SlasconeClientV2
			=> _slasconeClientV2 ??= InitializeLicensingClient();

		private ISlasconeClientV2 InitializeLicensingClient()
		{
			// ----------------------------------------------------------------------------------------------------------
			// In a real application you would use the standard factory (SlasconeClientV2Factory) from the nuget package.
			// The NoInternetDecorator is only used here to simulate the offline mode.
			// ----------------------------------------------------------------------------------------------------------

			var slasconeClientV2 =
				SlasconeClientV2NoInternetDecoratorFactory.BuildClient(ApiBaseUrl, IsvId)
					.SetProvisioningKey(ProvisioningKey)
					.SetLastModifiedByHeader("Slascone.Provisioning.Wpf.Sample.NuGet")
					.SetHttpClientTimeout(TimeSpan.FromMilliseconds(30000));

			slasconeClientV2.SetAppDataFolder(AppDataFolder);

#if NET6_0_OR_GREATER

			// Importing a RSA key from a PEM encoded string is available in .NET 6.0 or later
			using (var rsa = RSA.Create())
			{
				rsa.ImportFromPem(SignaturePubKeyPem.ToCharArray());
				slasconeClientV2
					.SetSignaturePublicKey(new PublicKey(rsa))
					.SetSignatureValidationMode(SignatureValidationMode);
			}
#else

			// If you are not using .NET 6.0 or later you have to load the public key from a xml string
			slasconeClientV2.SetSignaturePublicKeyXml(SignaturePublicKeyXml);
			slasconeClientV2.SetSignatureValidationMode(SignatureValidationMode);

#endif

			var assembly = Assembly.GetAssembly(typeof(ISlasconeClientV2));
			var version = assembly.GetName().Version;

			return slasconeClientV2;
		}

		private async Task RefreshLicenseInfoClientTypeDevicesAsync()
		{
			if (OfflineLicensing())
				return;

			SetLicensingState(LicensingState.Pending, "License validation pending ...");

			LicenseInfoDto? licenseInfo = null;
			string? errorMessage = null;

			(licenseInfo, errorMessage) = await AddHeartbeatAsync(DeviceId, response =>
			{
				// License needs activation
				SetLicensingState(LicensingState.NeedsActivation,
					$"License heartbeat received an error: {response.Error.Message}");
				return ErrorHandlingHelper.ErrorHandlingControl.Abort;
			});

			if (null == licenseInfo)
			{
				if (LicensingState.Pending == LicensingState)
				{
					SetLicensingState(LicensingState.Invalid, $"License information refresh failed. {errorMessage ?? ""}");
				}

				return;
			}

			if (!licenseInfo.Is_license_valid || !licenseInfo.Is_software_version_valid)
			{
				var licenseInvalidDescription =
					licenseInfo.Is_license_expired
						? $"License is expired since {licenseInfo.Expiration_date_utc.GetValueOrDefault():d}"
						: !licenseInfo.Is_license_active
							? "License is not active"
							: !licenseInfo.Is_software_version_valid
								? "License is not valid for this software version"
								: "License is not valid";

				SetLicensingState(LicensingState.Invalid, licenseInvalidDescription);
				return;
			}

			_licenseInfo = licenseInfo;

			await HandleProvisioningMode();
		}

		private async Task RefreshLicenseInfoClientTypeUsersAsync()
		{
			SetLicensingState(LicensingState.Pending, "License validation pending ...");

			LicenseInfoDto? licenseInfo = null;
			string? errorMessage = null;

			if (!_authenticationService.IsSignedIn)
			{
				await _authenticationService.SignInAsync();

				// AuthenticationServiceLoginStateChanged event handler will trigger RefreshLicenseInformationAsync after successful login
				return;
			}

			var licenses = 
				await LookupLicenseAsync(_authenticationService.Email, _authenticationService.BearerToken);

			if (!(licenses?.Any() ?? false))
			{
				return;
			}

			(licenseInfo, errorMessage) = await AddHeartbeatAsync($"{DeviceId}/{_authenticationService.Email}",
				response =>
				{
					// License needs activation

					// Activate license retrieved from lookup
					var license = licenses.First();
					ActivateLicenseAsync(license.Id.ToString(), $"{DeviceId}/{_authenticationService.Email}").Wait();

					return ErrorHandlingHelper.ErrorHandlingControl.Continue;
				});


			if (null == licenseInfo)
			{
				if (LicensingState.Pending == LicensingState)
				{
					SetLicensingState(LicensingState.Invalid, $"License information refresh failed. {errorMessage ?? ""}");
				}

				return;
			}

			if (!licenseInfo.Is_license_valid || !licenseInfo.Is_software_version_valid)
			{
				var licenseInvalidDescription =
					licenseInfo.Is_license_expired
						? $"License is expired since {licenseInfo.Expiration_date_utc.GetValueOrDefault():d}"
						: !licenseInfo.Is_license_active
							? "License is not active"
							: !licenseInfo.Is_software_version_valid
								? "License is not valid for this software version"
								: "License is not valid";

				SetLicensingState(LicensingState.Invalid, licenseInvalidDescription);
				return;
			}

			_licenseInfo = licenseInfo;

			await HandleProvisioningMode();
		}

		private async Task<(LicenseInfoDto, string?)> AddHeartbeatAsync(string clientId, Func<ApiResponse<LicenseInfoDto>, ErrorHandlingHelper.ErrorHandlingControl> needsActivationStrategy)
		{
			return await ErrorHandlingHelper.Execute(SlasconeClientV2.Provisioning.AddHeartbeatAsync,
				() => new AddHeartbeatDto
				{
					Product_id = _product_id,
					Client_id = clientId,
					Token_key = GetTokenKeyFromTemporaryOfflineLicense(),
					Software_version = SoftwareVersion,
					Operating_system = OperatingSystem
				},
				response =>
				{
					if ((int)HttpStatusCode.Conflict == response.StatusCode)
					{
						if (2006 == response.Error.Id)
						{
							// License needs activation
							return needsActivationStrategy(response);
						}
						else if (2002 == response.Error.Id)
						{
							// Token is not assigned
							RemoveTemporaryOfflineLicenseFiles();
							return ErrorHandlingHelper.ErrorHandlingControl.Retry;
						}
					}
					else if ((int)HttpStatusCode.BadRequest == response.StatusCode
					         || (int)HttpStatusCode.ServiceUnavailable == response.StatusCode
					         || (int)HttpStatusCode.GatewayTimeout == response.StatusCode)
					{
						return TemporaryOfflineFallback()
							? ErrorHandlingHelper.ErrorHandlingControl.Abort
							: ErrorHandlingHelper.ErrorHandlingControl.Continue;
					}

					return ErrorHandlingHelper.ErrorHandlingControl.Continue;
				}).ConfigureAwait(false);
		}

		private async Task<ICollection<LicenseDto>?> LookupLicenseAsync(string userId, string bearerToken)
		{
			SlasconeClientV2.SetBearer($"Bearer {bearerToken}");

			var (licenses, errorMessage) =
				await ErrorHandlingHelper.Execute(SlasconeClientV2.Provisioning.GetLicensesByUserAsync,
					() => new GetLicensesByUserDto
					{
						Product_id = _product_id,
						User_id = userId,
						Active_licenses_only = true
					},
					response =>
					{
						if ((int)HttpStatusCode.Conflict == response.StatusCode)
						{
							SetLicensingState(LicensingState.Invalid, response.Message);
							return ErrorHandlingHelper.ErrorHandlingControl.Abort;
						}
						else if ((int)HttpStatusCode.BadRequest == response.StatusCode
						         || (int)HttpStatusCode.ServiceUnavailable == response.StatusCode
						         || (int)HttpStatusCode.GatewayTimeout == response.StatusCode)
						{
							return ErrorHandlingHelper.ErrorHandlingControl.Abort;
						}

						return ErrorHandlingHelper.ErrorHandlingControl.Continue;
					}).ConfigureAwait(false);

			if (null == licenses)
			{
				SetLicensingState(LicensingState.Invalid, errorMessage ?? "License lookup failed.");
				return null;
			}

			// Only respect licenses where the user is active
			licenses = licenses.Where(license => license.License_users.Any(u => u.User_id == userId && u.Is_active)).ToList();

			if (!licenses.Any())
			{
				SetLicensingState(LicensingState.Invalid, "User has no licenses");
				return null;
			}

			return licenses;
		}

		private async Task HandleProvisioningMode()
		{
			if (Client.ProvisioningMode.Named == _licenseInfo.Provisioning_mode
				&& ClientType.Devices == ClientType)
			{
				SetLicensingState(LicensingState.FullyValidated, BuildDescription(_licenseInfo, LicensingState.FullyValidated));
			}

			if (Client.ProvisioningMode.Floating == _licenseInfo.Provisioning_mode
			    || ClientType.Users == ClientType)
			{
				_sessionManager = new SessionManager(_slasconeClientV2, _licenseInfo, DeviceId, _authenticationService);

				_sessionManager.StatusChanged += (sender, args) =>
				{
					if (LicensingState.FullyValidated == args.LicensingState)
					{
						args.LicensingStateDescription = BuildDescription(_licenseInfo, LicensingState.FullyValidated);
					}

					Application.Current.Dispatcher.Invoke(() =>
					{
						SetLicensingState(args.LicensingState, args.LicensingStateDescription);
					});
				};

				await _sessionManager.OpenSessionAsync();
			}
		}

		/// <summary>
		/// Check offline licensing state
		/// </summary>
		/// <returns>true if offline licensing found, false otherwise.</returns>
		/// <remarks>Even if offline licensing is stated it doesn't mean that the licensing state is OK. It also can be 'invalid' or 'needs activation'.</remarks>
		private bool OfflineLicensing()
		{
			// Look for an offline license file
			// It can be invalid
            var isOfflineLicensing = CheckOfflineLicensingFile(out var licenseInfo, out var isOfflineLicenseFileValid);

            if (!isOfflineLicensing)
            {
				// No offline licensing found
                return false;
            }

			if (isOfflineLicensing && !isOfflineLicenseFileValid)
            {
				// Offline licensing found, but invalid
				// No further checks needed
                return true;
            }

			_licenseInfo = licenseInfo;

			// Look for activation, either in the license file or in a separate activation file
            if (CheckOfflineLicensingInlineActivation(licenseInfo))
            {
                return true;
            }

			CheckOfflineLicensingActivationFile(licenseInfo);

			return true;
		}

        private bool CheckOfflineLicensingFile(out LicenseInfoDto licenseInfo, out bool isOfflineLicenseFileValid)
        {
            // Look for an offline license file
            licenseInfo = null;
			isOfflineLicenseFileValid = false;
            try
            {
                var licenseFilePath = Path.Combine(AppDataFolder, OfflineLicenseFileName);

                // Check if license file exists
                if (!File.Exists(licenseFilePath))
                    return false;

                if (!SlasconeClientV2.IsFileSignatureValid(licenseFilePath))
                {
                    SetLicensingState(LicensingState.LicenseFileInvalid, "License file invalid: signature check failed!");
                    return true;
                }

                licenseInfo = SlasconeClientV2.ReadLicenseFile(licenseFilePath);

                // Check product id
                if (licenseInfo.Product_id != _product_id)
                {
                    licenseInfo = null;
                    SetLicensingState(LicensingState.LicenseFileInvalid, "License file invalid: product id doesn't match!");
                    return true;
                }

                // Check expiration date
                if (!licenseInfo.Expiration_date_utc.HasValue
                    || licenseInfo.Expiration_date_utc.Value < DateTime.UtcNow)
                {
                    licenseInfo = null;
                    SetLicensingState(LicensingState.LicenseFileInvalid, "License file invalid: license is expired!");
                    return true;
                }
            
                // Check if software release limitation is compliant
                if (!SlasconeClientV2.IsReleaseCompliant(licenseInfo, SoftwareVersion))
                {
	                SetLicensingState(LicensingState.LicenseFileInvalid, "License file invalid: not valid for this software version");
	                return true;
                }
            }
			catch (Exception ex)
            {
                licenseInfo = null;
                SetLicensingState(LicensingState.LicenseFileInvalid, $"License file invalid: could not read file: {ex.Message}");
                return true;
            }

            isOfflineLicenseFileValid = null != licenseInfo;
            return true;
        }

        private bool CheckOfflineLicensingInlineActivation(LicenseInfoDto licenseInfo)
        {
            if (null == licenseInfo.Client_id) 
                return false;

            // Check Client_id: must match the device id
            if (string.Equals(licenseInfo.Client_id, DeviceId, StringComparison.InvariantCultureIgnoreCase))
            {
                SetLicensingState(LicensingState.OfflineValidated, $"{BuildDescription(_licenseInfo, LicensingState.OfflineValidated)}{Environment.NewLine}(offline license, inline activation)");
                return true;
            }
            else
            {
                SetLicensingState(LicensingState.LicenseFileInvalid, "License file invalid: client id mismatch");
                return true;
            }
        }

        private void CheckOfflineLicensingActivationFile(LicenseInfoDto licenseInfo)
        {
            ActivationDto? activation = null;
            try
            {
                var activationFilePath = Path.Combine(AppDataFolder, OfflineActivationFileName);

                // Check if activation file exists and validate signature
                if (File.Exists(activationFilePath))
                {
                    if (!SlasconeClientV2.IsFileSignatureValid(activationFilePath))
                    {
                        SetLicensingState(LicensingState.NeedsOfflineActivation, "Activation file invalid: signature check failed!");
                        return;
                    }

                    activation = SlasconeClientV2.ReadActivationFile(activationFilePath);

                    // Check if license IDs and Client ID and Device ID match
                    if (activation.License_key != licenseInfo.License_key)
                    {
                        activation = null;
                        SetLicensingState(LicensingState.NeedsOfflineActivation, "Activation file invalid: license key doesn't match!");
                        return;
                    }

                    if (activation.Client_id != DeviceId)
                    {
                        activation = null;
                        SetLicensingState(LicensingState.NeedsOfflineActivation, "Activation file invalid: client id mismatch!");
                        return;
                    }

                    // Insert token key into license info
                    _licenseInfo.Token_key = activation.Token_key;
                }
            }
            catch (Exception)
            {
                activation = null;
                SetLicensingState(LicensingState.NeedsOfflineActivation, "Activation file invalid: could not read file!");
                return;
            }

            if (null != activation)
            {
                SetLicensingState(LicensingState.OfflineValidated, $"{BuildDescription(_licenseInfo, LicensingState)}{Environment.NewLine}(offline license, activated via activation file)");
            }
            else
            {
                SetLicensingState(LicensingState.NeedsOfflineActivation, $"{BuildDescription(_licenseInfo, LicensingState)}{Environment.NewLine}(offline license, needs activation)");
            }
        }

        private Guid? GetTokenKeyFromTemporaryOfflineLicense()
        {
			var response = SlasconeClientV2.GetOfflineLicense();

			// Status code 400 signals there is no offline license available
			if (400 == response.StatusCode)
				return null;

			// Status code 409 signals there is an offline license available, 
			// but it is not valid.
			if (409 == response.StatusCode)
			{
				return null;
			}

			var licenseInfo = response.Result;

			// Check product id
			if (licenseInfo.Product_id != _product_id)
			{
				return null;
			}

			// Check if Client ID and Device ID match
			if (licenseInfo.Client_id != DeviceId)
			{
				return null;
			}

			return licenseInfo.Token_key;
        }

        private bool TemporaryOfflineFallback()
		{
			var response = SlasconeClientV2.GetOfflineLicense();

			// Status code 400 signals there is no offline license available
			if (400 == response.StatusCode) 
				return false;

			// Status code 409 signals there is an offline license available, 
			// but it is not valid.
			if (409 == response.StatusCode)
			{
				SetLicensingState(LicensingState.Invalid, $"Temporary offline license invalid: {response.Error.Message}");
				return true;
			}

			var licenseInfo = response.Result;

			// Check product id
			if (licenseInfo.Product_id != _product_id)
			{
				licenseInfo = null;
				SetLicensingState(LicensingState.Invalid, "Temporary offline license invalid: product id doesn't match!");
				return true;
			}

			// Check if Client ID and Device ID match
			if (licenseInfo.Client_id != DeviceId)
			{
				SetLicensingState(LicensingState.Invalid, $"Temporary offline license invalid: client id and device id don't match");
				return true;
			}

			// Check if software release limitation is compliant
			if (!SlasconeClientV2.IsReleaseCompliant(licenseInfo, SoftwareVersion))
			{
				SetLicensingState(LicensingState.Invalid, $"Temporary offline license invalid: not valid for this software version");
				return true;
			}

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
						SetLicensingState(LicensingState.Invalid, $"License expired on {expirationDate:d}.");
						return true;
					}
				}

				SetLicensingState(LicensingState.TemporaryOfflineValidated,
					$"{BuildDescription(_licenseInfo, LicensingState.TemporaryOfflineValidated)} (temporary offline)");
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

		private static string BuildDescription(LicenseInfoDto licenseInfo, LicensingState licensingState)
		{
			var sb = new StringBuilder();
			sb.Append($"Product licensed for {licenseInfo.Customer.Company_name}. ");
			if (LicensingState.TemporaryOfflineValidated == licensingState)
			{
				var freerideExpires = licenseInfo.Created_date_utc.Value + TimeSpan.FromDays(licenseInfo.Freeride.Value);
				sb.Append($"Freeride periods expires on {freerideExpires:d}");
			}
			else
			{
				sb.Append($"Expires on {licenseInfo.Expiration_date_utc.GetValueOrDefault().ToLocalTime():d}");
			}

			return sb.ToString();
		}

		private static string AppDataFolder { get; }
			= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
				Assembly.GetExecutingAssembly().GetName().Name);

		private void RemoveOfflineLicenseFiles()
		{
			var licenseFilePath = Path.Combine(AppDataFolder, OfflineLicenseFileName);
			var activationFilePath = Path.Combine(AppDataFolder, OfflineActivationFileName);

			// Delete files if existing
			if (File.Exists(licenseFilePath))
				File.Delete(licenseFilePath);
			if (File.Exists(activationFilePath))
				File.Delete(activationFilePath);
		}

		private void RemoveTemporaryOfflineLicenseFiles()
		{
			foreach (var file in new string[] { "license.json", "license_signature.txt", "signature.txt" })
			{
				var filePath = Path.Combine(AppDataFolder, file);
				if (File.Exists(filePath))
					File.Delete(filePath);
			}
		}

		// Event handler for App.AuthenticationService.LoginStateChanged
		private void AuthenticationServiceLoginStateChanged(object sender, EventArgs e)
		{
			if (_authenticationService.IsSignedIn)
			{
				Task.Run(async () => await RefreshLicenseInformationAsync());
			}
			else
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					SetLicensingState(LicensingState.NotSignedIn, _authenticationService.ErrorMessage);
				});
			}
		}

		#endregion

		#region Implementation of IDisposable

		public void Dispose()
		{
			if (null != _sessionManager)
			{
				_sessionManager.Dispose();
			}
		}

		#endregion
	}

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
}
