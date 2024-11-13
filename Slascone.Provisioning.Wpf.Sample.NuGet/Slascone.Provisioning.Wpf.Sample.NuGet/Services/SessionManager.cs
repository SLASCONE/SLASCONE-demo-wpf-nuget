using Slascone.Client.Interfaces;
using Slascone.Client;
using System;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services
{
	internal class SessionManager : IDisposable
	{
		#region Attributes

		private readonly ISlasconeClientV2 _slasconeClient;
		private readonly LicenseInfoDto _licenseInfo;
		private readonly string _deviceId;

		private Guid _sessionId;
		private SessionStatusDto? _sessionStatus;
		private string _sessionDescription;
		private CancellationTokenSource _cancellationTokenSource;
		private Task? _sessionRenewalTask;

		#endregion

		#region Constructor

		public SessionManager(ISlasconeClientV2 slasconeClient, LicenseInfoDto licenseInfo, string deviceID)
		{
			_slasconeClient = slasconeClient;
			_licenseInfo = licenseInfo;
			_deviceId = deviceID;
			_cancellationTokenSource = new CancellationTokenSource();
		}

		#endregion

		// Define a delegate that will be called if the status changes
		public delegate void StatusChangedEventHandler(object sender, LicensingStateChangedEventArgs e);

		// Define an event based on the delegate
		public event StatusChangedEventHandler StatusChanged;

		#region Interface

		public async Task OpenSessionAsync()
		{
			// Open a session using the Slascone client
			_sessionId = Guid.NewGuid();

			await SendOpenSessionRequestAsync();

			if (null != _sessionStatus)
			{
				// Start the session renewal task
				_sessionRenewalTask = Task.Run(() => RenewSessionPeriodicallyAsync(_cancellationTokenSource.Token));
			}
		}

		public Guid SessionId
			=> _sessionId;

		public DateTime? SessionValidUntil 
			=> _sessionStatus?.Session_valid_until?.LocalDateTime;

		public string SessionDescription
			=> _sessionDescription;

		#endregion

		#region IDisposable

		public void Dispose()
		{
			Task.Run(CloseSessionAsync).GetAwaiter().GetResult();

			if (!(null == _sessionRenewalTask
			      || _sessionRenewalTask.IsCompleted
			      || _sessionRenewalTask.IsCanceled))
			{
				_cancellationTokenSource.Cancel();
				_sessionRenewalTask.Wait();
			}

			_cancellationTokenSource.Dispose();
			_sessionRenewalTask?.Dispose();
		}

		#endregion

		#region Private Methods

		private async Task RenewSessionPeriodicallyAsync(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				await Task.Delay(TimeSpan.FromMinutes(_licenseInfo.Session_period.Value), cancellationToken);

				if (!cancellationToken.IsCancellationRequested)
				{
					// Renew the session
					await SendOpenSessionRequestAsync(true);
				}
			}
		}

		private async Task CloseSessionAsync()
		{
			// Cancel the session renewal task
			_cancellationTokenSource.Cancel();

			// Close the session using the Slascone client
			var sessionRequest = BuildSessionRequest();
			var result = await _slasconeClient.Provisioning.CloseSessionAsync(sessionRequest);
		}

		private async Task SendOpenSessionRequestAsync(bool renew = false)
		{
			var sessionRequest = BuildSessionRequest();

			var response = await _slasconeClient.Provisioning.OpenSessionAsync(sessionRequest);

			switch (response.StatusCode)
			{
				case (int)HttpStatusCode.OK:
				{
					_sessionStatus = response.Result;

					var isSessionValid = response.Result.Is_session_valid;
					_sessionDescription = isSessionValid ? "Session is valid" : "Session is not valid";

					StatusChanged?.Invoke(this, new LicensingStateChangedEventArgs
					{
						LicensingState = isSessionValid
							? LicensingState.FullyValidated
							: LicensingState.SessionOpenFailed,
						LicensingStateDescription = _sessionDescription
					});
					break;
				}

				case (int)HttpStatusCode.Conflict:
					_sessionStatus = null;
					_sessionDescription = response.Error.Message;

					StatusChanged?.Invoke(this, new LicensingStateChangedEventArgs
					{
						LicensingState =
							1007 == response.Error.Id
								? LicensingState.FloatingLimitExceeded
								: LicensingState.SessionOpenFailed,
						LicensingStateDescription = _sessionDescription
					});
					break;

				default:
					_sessionStatus = null;
					_sessionDescription = renew ? "Renew session failed" : "Open session failed";

					StatusChanged?.Invoke(this, new LicensingStateChangedEventArgs
					{
						LicensingState = LicensingState.SessionOpenFailed,
						LicensingStateDescription = _sessionDescription
					});
					break;
			}
		}

		private SessionRequestDto BuildSessionRequest()
		{
			var sessionRequest = new SessionRequestDto
			{
				License_id = Guid.Parse(_licenseInfo.License_key),
				Session_id = _sessionId
			};

			if (ClientType.Devices == _licenseInfo.Client_type)
			{
				sessionRequest.Client_id = _deviceId;
			}
			else if (ClientType.Users == _licenseInfo.Client_type)
			{
				sessionRequest.User_id = WindowsIdentity.GetCurrent().User.Value;
			}

			return sessionRequest;
		}

		#endregion
	}
}
