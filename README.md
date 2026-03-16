# LICENSING & ANALYTICS FOR SOFTWARE AND IoT VENDORS

### Table of Contents
- [Overview](#overview)
- [Connecting to your SLASCONE environment](#connecting-to-your-slascone-environment)
- [Online](#online)
- [Offline](#offline)
- [Named vs Floating](#named-vs-floating)
- [Named user licensing](#named-user-licensing)
- [Analytics](#analytics)
- [Software updates/shipment](#software-updatesshipment)
- [Error handling and retry logic](#error-handling-and-retry-logic)

## Overview

A demo client application (WPF) for SLASCONE software licensing, which uses the official NuGet package. Its main purpose is to demonstrate how you can enable online and/or offline device licensing (at the same time), while providing a rudimentary/explanatory UI. Although this is a desktop application, the same principles apply for other application types as well. Both named and floating licenses can be used.

![Image](https://github.com/user-attachments/assets/ba2b1545-420d-499d-b8d1-77e1c9e19f88)

Depending on your application you might need:

- both online and offline mode (most desktop applications)
- online mode only (application servers/backends)
- offline mode only (any application type with no connectivity)

This demo also implements a named user licensing scenario. While it uses Azure AD B2C for authentication, the same principles apply for any identity provider.

## CONNECTING TO YOUR SLASCONE ENVIRONMENT

The application connects to the SLASCONE official demo environment. In order to connect to your SLASCONE environment, adjust the values of the file `LicensingService.cs`.

## ONLINE

Online is the recommended licensing mode, since it unleashes the full functionality of SLASCONE.

### ACTIVATION (key based)
The online activation is a very straightforward process, requiring a license key.

![OnlineActivation2](https://github.com/user-attachments/assets/ee8f24be-fd53-4d22-98b3-6bdc1b3ca507)

### REFRESH/HEARTBEATS
After a successful activation, the application sends a periodic heartbeat (license check) to ensure that the license parameters are up to date.

![LicensedState](https://github.com/user-attachments/assets/90b82234-74fd-4892-b43b-44b0711ea787)

A heartbeat might fail due to primarily two reasons:
- No connectivity
- The license is not valid anymore (e.g., deactivated, expired).

#### TEMPORARILY OFFLINE - FREERIDE
If a heartbeat fails, you normally do not want to restrict software access immediately. Instead, you typically want to notify the user and ensure that the problem can be remedied (e.g., by going online), within a reasonable amount of time.

[Freeride](https://support.slascone.com/hc/en-us/articles/7702036319261#freeride) comes into play for such scenarios. In this example freeride is set to 7 days, but the value can be changed in the SLASCONE web portal.

### UNASSIGN
The licensing lifecycle for a device ends with its unassigning/deactivation. It is recommended to provide an area in your software, in which the end user can unassign the used license code, so that this can be used on another device (typical hardware migration scenario).

## OFFLINE
Please refer to this [article](https://support.slascone.com/hc/en-us/articles/4412248454161), in order to find more information about permanently offline scenarios.

### ACTIVATION (file based)

Activation in offline scenarios is a 2-step process requiring two license (xml) files:

- the license file
- the activation file

After uploading the license file, an activation file has to be generated and uploaded too. Since the client is offline this generation has to be done on a proxy device using:
- the generated link
- or the QR code

![Image](https://github.com/user-attachments/assets/4d334242-2827-44b2-8ac0-4b5faaf159d2)

By uploading the activation file, the activation is complete.

## NAMED vs FLOATING

This application automatically recognizes the provisioning mode of the inserted license (named or floating). In case of a floating license, the application opens a session as described [here](https://support.slascone.com/hc/en-us/articles/360016001677-NAMED-DEVICE-LICENSES).

## NAMED USER LICENSING
This application also depicts named user licensing. 
Please refer to this [article](https://support.slascone.com/hc/en-us/articles/360017647817-NAMED-USER-LICENSES) to find more information about named user licenses.

### AZURE AD B2C
This application uses Azure AD B2C for user authentication. In order to use Azure AD B2C (private SLASCONE deployments only), you need to register an application in your Azure AD B2C tenant and configure the application to use Azure AD B2C for authentication. The application uses the MSAL.NET library to authenticate users with Azure AD B2C and obtain access tokens to access APIs.

## Analytics

The application sends analytics data to SLASCONE. 
The data is used to provide insights into the usage of the application. 
The data is sent in the background and does not affect the user experience. 
The data is sent in a secure way and is only used for analytics purposes.

### License feature visualization

If a valid license is present, the application shows licensed features as menu items in the features menu.

![Image](https://github.com/user-attachments/assets/559622e3-75e7-420c-90c6-f6471f24f6ea)

On a click on a licensed feature, the application sends an usage heartbeat to SLASCONE. 

## SOFTWARE UPDATES/SHIPMENT

In the SLASCONE portal, you can manage software releases for your products. You can store software shipments for these releases. 
When a license heartbeat is generated or a license is activated, your application can transmit the current version number. 
The returned license information will then indicate whether there is a current software version matching the license. 
Learn more about managing software releases in the SLASCONE portal in this [article](https://support.slascone.com/hc/en-us/articles/360016055257-CREATING-A-PRODUCT).

In the demo client, both the current version and information about any potentially available newer version are displayed in the About Box:

![aboutbox](https://github.com/user-attachments/assets/746d1550-9c87-4ad5-9c33-87707ac683f8)

## Error handling and retry logic

The article [ERROR HANDLING](https://support.slascone.com/hc/en-us/articles/360016160398-ERROR-HANDLING) in the SLASCONE documentation provides general guidelines on how to implement error handling and retry logic for SLASCONE API calls. This demo client implements this logic for the online licensing mode, specifically for license activation, the license heartbeat and the open session process.

### General considerations

The SLASCONE API uses standard HTTP status codes combined with application-specific error codes to communicate the outcome of each request. When integrating with the API, your application should handle the following categories:

- **200 OK** — The request succeeded. Process the returned result normally.
- **400 Bad Request** — The request was malformed or contained invalid data. Do not retry automatically; review the request parameters.
- **401 Unauthorized / 403 Forbidden** — The API key, bearer token, or permissions are invalid. Verify your provisioning key or authentication credentials.
- **409 Conflict** — A logical/business error occurred. The response body contains a SLASCONE-specific error ID and message (e.g., error 2006 "license needs activation", error 2002 "token not assigned", error 1007 "floating limit exceeded"). Your application should inspect the error ID to determine the appropriate action.
- **503 Service Unavailable / 504 Gateway Timeout** — A transient server-side error. The response may include a `Retry-After` header indicating how many seconds to wait before retrying. Your application should honor this header (clamped to a reasonable range, e.g., 5–120 seconds) and retry the request a limited number of times (e.g., max 1 retry).

**Retry logic:** For transient errors (503/504), implement an automatic retry with a delay. If the response includes a `Retry-After` header, use its value; otherwise fall back to a sensible default (e.g., 10 seconds). Limit the number of retries to avoid infinite loops.

**Fallback logic:** If retries are unsuccessful, implement a fallback policy. Whenever possible, prioritize graceful degradation over outright denial to avoid disrupting the end-user experience. The appropriate strategy depends on the endpoint and your use case.

See also the article about [API fundamentals](https://support.slascone.com/hc/en-us/articles/360016153358-API-FUNDAMENTALS) for more information.

### Activate license error handling flow

Activation is a user-initiated action and should not be retried automatically. The application calls `ActivateLicenseAsync`, which directly invokes the SLASCONE API without a retry loop. If the activation fails, the error is displayed to the user.

### License heartbeat error handling flow

The `SlasconeClient` from the NuGet package stores the results of successful license heartbeats in a local file.
The error handling flow for license heartbeats then falls back to the last locally stored result if a license heartbeat fails due to a technical and/or transient error.

See also: [TEMPORARILY OFFLINE - FREERIDE](#TEMPORARILY-OFFLINE-FREERIDE)

### Open session error handling flow

The application differerntiates between logical errors (e.g., license not valid, floating limit exceeded) and technical errors (e.g., network problems). 
For logical errors, the application displays the specific SLASCONE error message to the user. For transient errors, the application implements a retry 
policy with a maximum of 1 retry, honoring the `Retry-After` header. If all retries are exhausted or for any other error status, the application treats 
the event as successful (200) to allow continued operation, while marking the session as conditionally valid.

### Error handling sample implementation

This demo client uses the helper class `ErrorHandlingHelper` to wrap SLASCONE API calls with the error handling and retry logic described above.

The helper's `Execute` method takes two inputs:
1. A **delegate** that performs the actual SLASCONE API call.
2. A **custom error handler** that inspects the error response and returns one of three `ErrorHandlingControl` values:
   - `Continue` — Exit the retry loop and proceed with standard error handling (generates a human-readable error message based on the status code).
   - `Retry` — Re-enter the retry loop with a fresh input argument (e.g., after removing stale local data).
   - `Abort` — Stop immediately and return `(null, null)` — the caller is expected to have already set the licensing state.

The retry loop works as follows:
1. Call the SLASCONE API endpoint.
2. If the response is **200 OK**, return the result.
3. If the response is **503 or 504**, read the `Retry-After` header (clamped to 5–120 seconds, default 10 seconds), wait, and retry (max 1 retry).
4. If all retries are exhausted or for any other error status, invoke the custom error handler.
5. Depending on the handler's return value, either retry, abort, or exit the loop with a standard error message.

If an unhandled exception occurs at any point, it is caught and returned as an error message.

This pattern decouples transient-error retry logic from endpoint-specific business logic, making it reusable across different SLASCONE API calls (heartbeats, sessions, license lookups, etc.).
