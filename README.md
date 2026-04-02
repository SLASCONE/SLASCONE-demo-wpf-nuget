# SLASCONE WPF Licensing & Analytics Sample

This WPF desktop sample shows how to integrate SLASCONE into a software product by using the [SLASCONE NuGet Client](https://www.nuget.org/packages/Slascone.Client/). It covers license management, entitlement management, usage analytics, floating licenses, offline licensing, and named-user licensing.

Going beyond simple API connectivity, it is designed as a production-oriented integration template for real-world desktop licensing scenarios, including temporary offline operation, local license caching, digital signature validation, floating session handling, and resilient handling of transient technical failures such as 5xx responses.

It includes examples for the most important licensing, analytics, and resilience workflows:

* license activation and heartbeat
* floating session management
* license file validation for offline activation
* named-user licensing
* analytical and usage heartbeats
* offline fallback using locally stored license data
* response and file integrity validation
* error handling and retry logic
* software version and shipment visibility

For more information, see the [SLASCONE website](https://slascone.com/), the [Help Center](https://support.slascone.com/), and the [API Test Center](https://api365.slascone.com/swagger).

## Quick Start

```bash
# Build the project
dotnet build

# Run the WPF sample
dotnet run --project Slascone.Demo.Wpf.NuGet
```

The application starts with the sample UI and is configured for a SLASCONE demo environment by default.

## Table of Contents

* [Quick Start](#quick-start)
* [What This Sample Demonstrates](#what-this-sample-demonstrates)
* [Connecting to Your SLASCONE Environment](#connecting-to-your-slascone-environment)
* [Offline Licensing and Freeride Period](#offline-licensing-and-freeride-period)
* [Floating License Management](#floating-license-management)
* [Named User Licensing](#named-user-licensing)
* [Analytics and Feature Usage](#analytics-and-feature-usage)
* [Software Updates / Shipment](#software-updates--shipment)
* [Configuration and Storage](#configuration-and-storage)
* [Error Handling and Retry Logic](#error-handling-and-retry-logic)
* [Technical Details](#technical-details)
* [Project Structure](#project-structure)
* [SLASCONE NuGet Client](#slascone-nuget-client)
* [Further Reading](#further-reading)

## What This Sample Demonstrates

This sample application showcases the following key features of the SLASCONE licensing service.

### License Management & Entitlements

**License Activation (online)**  
* Activates a license for a specific device using its unique device ID  
* Demonstrates how to activate a license key for the first time on a specific machine  
* Handles activation responses and potential warnings or errors  

**License Heartbeat**  
* Sends periodic license verification requests to the SLASCONE server  
* Retrieves up-to-date license information including features, limitations, and expiration details  
* Stores the latest valid licensing state locally for offline use  

**License Activation (offline)**  
* Supports permanently offline activation scenarios based on license files  
* Validates the digital signature of license files to prevent tampering  
* Reads and displays comprehensive license information from XML files  

**Offline License Support**  
* Reads license information when temporarily disconnected from the internet  
* Uses locally stored license data from the last successful heartbeat  
* Ensures the software can continue to function during temporary network outages  

**License Unassignment**  
* Demonstrates how to unassign a license from a device  
* Allows a license to be transferred to a different machine  

### Floating License Management

**Session Management**  
* Opens licensing sessions for floating license scenarios  
* Supports concurrent user licensing models  
* Allows software to be installed on multiple machines while limiting concurrent use  

**Offline Session Handling**  
* Preserves usability during transient failures  
* Supports continued operation based on the sample's local session handling strategy  
* Distinguishes between logical licensing errors and transient technical failures  

**Session Closure**  
* Properly releases floating licenses back to the pool  
* Ensures efficient use of available licenses  
* Prevents license hoarding by inactive installations  

### Named User Licensing

**Named User Workflow**  
* Demonstrates a named-user licensing scenario  
* Shows how user identity can be linked to SLASCONE licensing  
* Illustrates a desktop-oriented flow that can be adapted to other identity providers  

### Analytics Capabilities

**Analytical Heartbeat**  
* Sends application analytics data to SLASCONE  
* Supports background collection for operational and product insights  

**Feature Usage Tracking**  
* Visualizes licensed features in the UI  
* Sends usage heartbeats when features are used  
* Provides insight into feature-level usage patterns  

### Security Features

**Device Identification**  
* Uses platform-specific device identification for Windows, Linux, and macOS  
* Secures licenses to a specific client or device  
* Helps prevent unauthorized license transfers  

**Digital Signature Validation**  
* Verifies the authenticity of license files and server responses  
* Prevents tampering with licensing data  
* Supports secure offline and online validation flows  

**Replay Protection**  
* Supports nonce-based challenge-response protection where applicable  

## Connecting to Your SLASCONE Environment

By default, the application is configured to connect to a SLASCONE demo environment.

To connect it to your own SLASCONE environment, adjust the relevant configuration values in `LicensingService.cs`.

For meaningful testing and evaluation, your SLASCONE environment should have at least one active license.

> ⚠️ **Security Warning**: Keep provisioning keys and other secrets secure, and do not embed production secrets in publicly accessible repositories.

## Offline Licensing and Freeride Period

The SLASCONE licensing system provides robust support for offline scenarios, especially temporary offline operation in desktop applications. For more background on temporary and permanent offline scenarios, see the [Offline & Connectivity](https://support.slascone.com/hc/en-us/sections/10214124833693) section in the SLASCONE Help Center.

This sample demonstrates both temporary offline behavior and permanently offline activation scenarios.

1. **License Caching**
   * During a successful license heartbeat, the application stores the license data locally
   * The cached license information includes features, limitations, and expiration details
   * The data is protected with digital signatures to prevent tampering

2. **Offline Validation**
   * When the application cannot connect to the SLASCONE server, it falls back to the cached license data
   * The application verifies the digital signature of the cached data before using it
   * All relevant license rules such as features, limitations, and expiration continue to be enforced based on the cached state

3. **Offline Activation**
   * For permanently offline scenarios, activation is based on license files
   * The process uses a license file and a generated activation file
   * The activation file can be created on a proxy device via a generated link or QR code

![Offline activation flow](https://github.com/user-attachments/assets/4d334242-2827-44b2-8ac0-4b5faaf159d2)

### Freeride Period

If a heartbeat fails, software access should usually not be restricted immediately. The freeride period gives users time to restore connectivity while keeping licensing under control. The current sample README explains this as a 7-day example configured in the SLASCONE portal.

This approach helps avoid unnecessary disruption while preserving proper license enforcement over time.

## Floating License Management

This application automatically recognizes whether a license is provisioned as named or floating. In the floating case, it opens a session and manages the session lifecycle accordingly.

For transient failures during session opening, the sample applies a resilience strategy that distinguishes technical issues from logical licensing errors.

## Named User Licensing

This sample also demonstrates named-user licensing. The current implementation uses Azure AD B2C for authentication, but the same licensing principle can be applied with other identity providers as well.

### Azure AD B2C

To use Azure AD B2C in a compatible environment, you need to register an application in your Azure AD B2C tenant and configure the sample accordingly. The application uses MSAL.NET to authenticate users and obtain access tokens.

## Analytics and Feature Usage

The application sends analytics data to SLASCONE in the background and uses it to provide insights into application usage. The current README notes that this happens without affecting the end-user experience.

### License Feature Visualization

If a valid license is present, the application shows licensed features as menu items in the features menu. When a licensed feature is selected, the application sends a usage heartbeat to SLASCONE.

![Feature usage menu](https://github.com/user-attachments/assets/559622e3-75e7-420c-90c6-f6471f24f6ea)

## Software Updates / Shipment

In the SLASCONE portal, you can manage software releases and related software shipments for your products. When a license heartbeat is generated or a license is activated, the application can transmit its current version number. The returned license information can then indicate whether a newer software version is available for the current license.

In the sample client, both the current version and information about any potentially available newer version are displayed in the About box.

![About box](https://github.com/user-attachments/assets/746d1550-9c87-4ad5-9c33-87707ac683f8)

## Configuration and Storage

For detailed guidance on what should be stored locally, why it matters, and how cached license state supports offline and freeride scenarios, see [What to Store Locally in Your Client](https://support.slascone.com/hc/en-us/articles/7702036319261).

### Application Data Folder

The sample stores licensing-related data locally in order to support temporary offline operation and resilient desktop behavior.

Depending on your implementation, locally stored data may include:
* cached license information
* digital signatures for validating stored license state
* locally relevant floating-session state

Make sure your application stores such data in a location appropriate for desktop software and with suitable access restrictions.

## Error Handling and Retry Logic

This sample demonstrates how to handle SLASCONE API errors and implement retry logic for the online licensing flow. For detailed information about SLASCONE API error codes, refer to the [SLASCONE error handling documentation](https://support.slascone.com/hc/en-us/articles/360016160398).

The current implementation applies this logic specifically to:
* license activation
* license heartbeat
* open session handling

### General Considerations

When integrating with SLASCONE, your application should distinguish between the following categories:

* **200 OK**  
  Process the returned result normally.

* **400 Bad Request**  
  The request was malformed or contained invalid data. Do not retry automatically.

* **401 Unauthorized / 403 Forbidden**  
  Credentials or permissions are invalid. Verify your provisioning key or authentication setup.

* **409 Conflict**  
  A logical or business error occurred. Inspect the SLASCONE-specific error ID and message to determine the correct application behavior.

* **429 Too Many Requests / 503 Service Unavailable / 504 Gateway Timeout**  
  A transient server-side issue occurred. Honor the `Retry-After` header when present and retry only a limited number of times.

### Endpoint-Specific Handling

**Activate license**  
Activation is a user-initiated action and should not be retried automatically. If activation fails, the error should be shown directly to the user.

**License heartbeat**  
The sample falls back to the last locally stored license result when a heartbeat fails due to a transient technical error.

**Open session**  
The sample distinguishes between logical errors such as exceeded floating limits and transient technical issues. For transient failures, it retries once, honors `Retry-After`, and then applies a fallback strategy that preserves usability.

### Retry Logic

The current README describes the following retry pattern:
* retry transient errors such as `429`, `503`, and `504`
* respect `Retry-After` if present
* clamp the delay to a reasonable range such as 5 to 120 seconds
* fall back to a default delay if the header is absent
* limit retries to avoid infinite loops

### Sample Implementation

This sample uses an `ErrorHandlingHelper` class to wrap SLASCONE API calls and separate generic transient-error handling from endpoint-specific business logic. The helper supports a control flow such as:
* continue with normal error handling
* retry with updated input
* abort and return control to the caller

## Technical Details

### Environment Requirements

* .NET SDK compatible with the sample project
* Internet connectivity for initial activation and online heartbeat operations
* A SLASCONE environment or the configured demo environment

### Dependencies Overview

This application uses:
* the [SLASCONE NuGet Client](https://www.nuget.org/packages/Slascone.Client/)
* MSAL.NET for Azure AD B2C-based authentication in the named-user scenario
* platform-specific mechanisms for device identification

### Cross-Platform Compatibility

The sample includes platform-specific device identification logic for:
* Windows
* Linux
* macOS

## Project Structure

```text
SLASCONE-demo-wpf-nuget/
├── README.md
├── Slascone.Demo.Wpf.NuGet/
│   ├── ... WPF application files
│   ├── LicensingService.cs
│   ├── UI views and view models
│   └── Assets/
```

Adjust the structure details as needed to match the repository exactly.

## SLASCONE NuGet Client

This sample is built on the [SLASCONE NuGet Client](https://www.nuget.org/packages/Slascone.Client) rather than direct API calls. The client provides a higher-level integration layer for common licensing workflows and supports features such as offline access, local persistence, and resilient request handling.

## Further Reading

* [API Test Center](https://api365.slascone.com/swagger)
* [What and How to Save in Your Client](https://support.slascone.com/hc/en-us/articles/7702036319261)
* [Digital Signature and Data Integrity](https://support.slascone.com/hc/en-us/articles/360016063637)
* [Error Handling](https://support.slascone.com/hc/en-us/articles/360016160398)
* [Named User Licenses](https://support.slascone.com/hc/en-us/articles/360017647817-NAMED-USER-LICENSES)
* [Product Analytics](https://support.slascone.com/hc/en-us/articles/360016055537)
* [Creating a Product](https://support.slascone.com/hc/en-us/articles/360016055257-CREATING-A-PRODUCT)
