using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Licensing;

public class ActivationFileViewModel
{
	#region Interface

	public string ApiUrl { get; set; }
	public string IsvId { get; set; }
	public string ProductId { get; set; }
	public string LicenseKey { get; set; }
	public string ClientId { get; set; }

	public string ActivationFileDownloadUrl
	{
		get
		{
			var urlBuilder = new System.Text.StringBuilder();
			urlBuilder.Append(ApiUrl != null ? ApiUrl.TrimEnd('/') : "")
				.Append($"/api/v2/isv/{IsvId}/provisioning/activations/offline?")
				.Append($"product_id={Uri.EscapeDataString(ProductId)}&")
				.Append($"license_key={Uri.EscapeDataString(LicenseKey)}&")
				.Append($"client_id={Uri.EscapeDataString(ClientId)}");
			return urlBuilder.ToString();
		}
	}

	#endregion
}