using Slascone.Client.Interfaces;
using System;
using System.Net.Http;
using Slascone.Client;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services.SimulateNoInternet
{
	// ------------------------------------------------------------------------------
	// This factory builds a decorated instance of ISlasconeClientV2 that simulates a
	// temporarily lost internet connection. 
	// You don't need this in a real application.
	// ------------------------------------------------------------------------------

	public class SlasconeClientV2NoInternetDecoratorFactory
	{
		public static ISlasconeClientV2 BuildClient(string baseUrl, Guid isv_id, IHttpClientFactory httpClientFactory)
		{
			var decoratedSlasconeClientV2 = SlasconeClientV2Factory.BuildClient(baseUrl, isv_id, httpClientFactory);
			return new SlasconeClientV2NoInternetDecorator(decoratedSlasconeClientV2);
		}

		public static ISlasconeClientV2 BuildClient(string baseUrl, Guid isv_id, string provisioningKey, IHttpClientFactory httpClientFactory)
		{
			var decoratedSlasconeClientV2 = SlasconeClientV2Factory.BuildClient(baseUrl, isv_id, provisioningKey, httpClientFactory);
			return new SlasconeClientV2NoInternetDecorator(decoratedSlasconeClientV2);
		}
	}
}