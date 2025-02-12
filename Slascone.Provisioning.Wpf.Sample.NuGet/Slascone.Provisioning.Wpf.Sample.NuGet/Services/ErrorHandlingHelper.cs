using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Slascone.Client;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services
{
	/// <summary>
	/// Helper class to handle errors and standard retries while calling the SLASCONE API.
	/// </summary>
	internal static class ErrorHandlingHelper
	{
		/// <summary>
		/// Return values for the custom error handler to control the retry logic and further processing.
		/// </summary>
		/// <remarks>
		/// <list type="bullet">
		/// <item>
		/// <term>Continue</term>
		/// <description>Proceed with standard error handling.</description>
		/// </item>
		/// <item>
		/// <term>Retry</term>
		/// <description>Retry the operation.</description>
		/// </item>
		/// <item>
		/// <term>Abort</term>
		/// <description>Error handling is complete. Abort and do not perform standard retry logic or error handling.</description>
		/// </item>
		/// </list>
		/// </remarks>
		public enum ErrorHandlingControl
		{
			Continue = 0,
			Retry = 1,
			Abort = 2
		}

		/// <summary>
		/// Wait time between retries
		/// </summary>
		private static readonly TimeSpan RetryWaitTime = TimeSpan.FromSeconds(10);

		/// <summary>
		/// Do max 1 retry
		/// </summary>
		private const int MaxRetryCount = 1;

		/// <summary>
		/// Call a SLASCONE API endpoint with standard retry logic
		/// </summary>
		/// <typeparam name="TIn">Type of input argument</typeparam>
		/// <typeparam name="TOut">Type of result</typeparam>
		/// <param name="func">SLASCONE API endpoint call</param>
		/// <param name="argument">Input argument</param>
		/// <param name="callerMemberName">Caller member name for error message if necessary</param>
		/// <returns></returns>
		public static async Task<(TOut, string?)> Execute<TIn, TOut>(Func<TIn, Task<ApiResponse<TOut>>> func, TIn argument, [CallerMemberName] string callerMemberName = "")
			where TOut : class
		{
			return await Execute(func, argument, _ => ErrorHandlingControl.Continue, callerMemberName).ConfigureAwait(false);
		}

		/// <summary>
		/// Call a SLASCONE API endpoint with standard retry logic and a custom error handler
		/// </summary>
		/// <typeparam name="TIn">Type of input argument</typeparam>
		/// <typeparam name="TOut">Type of result</typeparam>
		/// <param name="func">SLASCONE API endpoint call</param>
		/// <param name="argument">Input argument</param>
		///	<param name="handler">Custom error handler</param>
		/// <param name="callerMemberName">Caller member name for error message if necessary</param>
		/// <returns></returns>
		public static Task<(TOut, string?)> Execute<TIn, TOut>(
			Func<TIn, Task<ApiResponse<TOut>>> func,
			TIn argument,
			Func<ApiResponse<TOut>, ErrorHandlingControl> handler,
			[CallerMemberName] string callerMemberName = "")
			where TOut : class
		{
			return Execute(func, () => argument, handler, callerMemberName);
		}

		/// <summary>
		/// Call a SLASCONE API endpoint with standard retry logic, a custom error handler, and a function to provide the input argument
		/// </summary>
		/// <remarks>The dynamic argument function is necessary to provide another input argument in case of a retry.</remarks>
		/// <typeparam name="TIn">Type of input argument</typeparam>
		/// <typeparam name="TOut">Type of result</typeparam>
		/// <param name="func">SLASCONE API endpoint call</param>
		/// <param name="argumentFunc">Input argument function</param>
		///	<param name="handler">Custom error handler</param>
		/// <param name="callerMemberName">Caller member name for error message if necessary</param>
		/// <returns></returns>
		public static async Task<(TOut, string?)> Execute<TIn, TOut>(
			Func<TIn, Task<ApiResponse<TOut>>> func, 
			Func<TIn> argumentFunc, 
			Func<ApiResponse<TOut>, ErrorHandlingControl> handler, 
			[CallerMemberName] string callerMemberName = "")
			where TOut : class
		{
			string errorMessage = null;

			try
			{
				ApiResponse<TOut> result = null;

				int retryCountdown = MaxRetryCount;

				while (0 <= retryCountdown)
				{
					var argument = argumentFunc();

					// Call the SLASCONE API endpoint
					result = await func.Invoke(argument).ConfigureAwait(false);

					if ((int)HttpStatusCode.OK == result.StatusCode)
					{
						// Success
						return (result.Result, null);
					}
					
					if ((int)HttpStatusCode.ServiceUnavailable == result.StatusCode
					         || (int)HttpStatusCode.GatewayTimeout == result.StatusCode)
					{
						// Transient error: Wait 30 seconds and try again
						--retryCountdown;
						if (0 <= retryCountdown)
						{
							await Task.Delay(RetryWaitTime).ConfigureAwait(false);
							continue;
						}
					}

					// Invoke custom error handling
					var errorHandlingControl = handler.Invoke(result);

					switch (errorHandlingControl)
					{
						case ErrorHandlingControl.Continue:
							// No more retries
							retryCountdown = -1;
							break;

						case ErrorHandlingControl.Retry:
							break;

						case ErrorHandlingControl.Abort:
							return (null, (string?)null);
					}
				}

				// Standard error handling: Return error message
				if ((int)HttpStatusCode.Conflict == result.StatusCode)
				{
					errorMessage = $"SLASCONE error {result.Error?.Id}: {result.Error?.Message}";
				}
				else
				{
					errorMessage = $"SLASCONE error {result.StatusCode}: {result.Message}";
				}
			}
			catch (Exception ex)
			{
				errorMessage = $"{callerMemberName} threw an exception:{Environment.NewLine}{ex.Message}";
			}

			return (null, errorMessage);
		}
	}
}
