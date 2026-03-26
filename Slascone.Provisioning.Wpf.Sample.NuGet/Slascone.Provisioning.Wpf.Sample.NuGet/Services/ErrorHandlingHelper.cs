using Slascone.Client;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
        /// Wait time between retries (in seconds)
        /// </summary>
        private static readonly int RetryWaitTime = 15;

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
				ApiResponse<TOut> response = null;

				int retryCountdown = MaxRetryCount;

				while (0 <= retryCountdown)
				{
					var argument = argumentFunc();

					// Call the SLASCONE API endpoint
					response = await func.Invoke(argument).ConfigureAwait(false);

					if ((int)HttpStatusCode.OK == response.StatusCode)
					{
						// Success
						return (response.Result, null);
					}

                    var isTransientStatusCode = IsTransientError(response.StatusCode);
                    var isTransientException = IsTransientError(response.ApiException);

                    if (isTransientStatusCode || isTransientException)
					{
                        // Transient error: Wait 30 seconds and try again
                        --retryCountdown;
						if (0 <= retryCountdown)
                        {
							await Task.Delay(TimeSpan.FromSeconds(GetRetryAfterPeriod(response.ApiException))).ConfigureAwait(false);
							continue;
						}
					}

					// Invoke custom error handling
					var errorHandlingControl = handler.Invoke(response);

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
				if ((int)HttpStatusCode.Conflict == response.StatusCode)
				{
					errorMessage = $"SLASCONE error {response.Error?.Id}: {response.Error?.Message}";
				}
				else if ((int)HttpStatusCode.Unauthorized == response.StatusCode
				         || (int)HttpStatusCode.Forbidden == response.StatusCode)
				{
					errorMessage = "Not authorized!";
				}
				else if (response.StatusCode <= 0 )
				{
					errorMessage = $"Error: {response.Message}";
				}

                else
                {
                    errorMessage = $"SLASCONE error {response.StatusCode}: {response.Message}";
                }
			}
            catch (Exception ex)
			{
				errorMessage = $"{callerMemberName} threw an exception:{Environment.NewLine}{ex.Message}";
			}

			return (null, errorMessage);
		}

        private static bool IsTransientError(ApiException exception)
        {
            if (exception?.InnerException != null && typeof(HttpRequestException) == exception.InnerException.GetType())
            {
                return true;
            }
            if (exception != null)
            {
                return IsTransientError(exception.StatusCode);
            }
            return false;
        }

        private static bool IsTransientError(int httpStatusCode)
        {
            return httpStatusCode == 408 || // Request Timeout
                   httpStatusCode == 429 || // Too Many Requests
                   httpStatusCode == 500 || // Internal Server Error
                   httpStatusCode == 502 || // Bad Gateway
                   httpStatusCode == 503 || // Service Unavailable
                   httpStatusCode == 504 || // Gateway Timeout
                   httpStatusCode == 507; // Insufficient Storage
        }

        private static int GetRetryAfterPeriod(ApiException apiException)
        {
            if (apiException?.Headers?.TryGetValue("Retry-After", out var retryAfterValues) ?? false)
            {
                var retryAfterValue = retryAfterValues.FirstOrDefault();
                if (int.TryParse(retryAfterValue, out var retryAfterSeconds))
                {
                    return Math.Clamp(retryAfterSeconds, 5, 120);
                }
            }

            return RetryWaitTime;
        }
    }
}
