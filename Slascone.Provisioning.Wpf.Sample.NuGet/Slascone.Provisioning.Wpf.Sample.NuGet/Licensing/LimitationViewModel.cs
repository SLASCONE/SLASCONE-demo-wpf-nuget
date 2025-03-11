using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Slascone.Client;
using Slascone.Provisioning.Wpf.Sample.NuGet.Services;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Licensing
{
	internal class LimitationViewModel : INotifyPropertyChanged
	{
		#region Attributes

		private readonly LicensingService _licensingService;
		private readonly ProvisioningLimitationDto _limitation;
		private decimal? _goodwill;

		#endregion

		#region Construction

		public LimitationViewModel(LicensingService licensingService, ProvisioningLimitationDto limitation)
		{
			_licensingService = licensingService;
			_limitation = limitation;
		}

		#endregion

		#region Interface

		public string LimitationName
			=> _limitation.Name;

		public string LimitationDescription
			=> _limitation.Description;

		public string Value
			=> $"{_limitation.Value:D}";

		public string Remaining
			=> _limitation.Remaining < 0 && _goodwill.HasValue
				? $"{_limitation.Remaining:F1} (Goodwill: {_goodwill:F}%)"
				: $"{_limitation.Remaining:F1}";

		public string ResetMode
			=> $"{_limitation.Consumption_reset_mode}";

		public int? ResetPeriodDays
			=> _limitation.Consumption_reset_period_days;

		public bool CanAddConsumption
			=> ConsumptionResetPeriod.Disabled != _limitation.Consumption_reset_mode;

		public DateTimeOffset? LastResetDate { get; set; }
		public DateTimeOffset? NextResetDate { get; set; }

		public string ConsumptionHeartbeatResult { get; set; }

		public async Task AddConsumptionAsync()
		{
			ConsumptionHeartbeatResult = "Adding consumption heartbeat ...";
			OnPropertyChanged(nameof(ConsumptionHeartbeatResult));

			var(consumption, errorMessage) = await _licensingService.AddConsumptionHeartbeatAsync(_limitation.Id, 1.0m);

			if (null != consumption)
			{
				_limitation.Remaining = consumption?.Remaining ?? 0.0m;
				_goodwill = consumption.Goodwill;
				OnPropertyChanged(nameof(Remaining));
				_limitation.Value = Convert.ToInt32(consumption?.Limit ?? 0);
				OnPropertyChanged(nameof(Value));
				ConsumptionHeartbeatResult = "Consumption added successfully";
				OnPropertyChanged(nameof(ConsumptionHeartbeatResult));
				LastResetDate = consumption.Last_reset_date_utc;
				OnPropertyChanged(nameof(LastResetDate));
				NextResetDate = consumption.Next_reset_date_utc;
				OnPropertyChanged(nameof(NextResetDate));
			}
			else
			{
				ConsumptionHeartbeatResult = errorMessage;
				OnPropertyChanged(nameof(ConsumptionHeartbeatResult));
			}
		}

		#endregion

		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(field, value)) return false;
			field = value;
			OnPropertyChanged(propertyName);
			return true;
		}

		#endregion
	}
}
