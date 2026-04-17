# SLASCONE WPF Licensing & Analytics Sample

This WPF desktop sample shows how to integrate SLASCONE into a software product by using the [SLASCONE NuGet Client](https://www.nuget.org/packages/Slascone.Client/). It demonstrates real licensing workflows including online and offline activation, floating licenses, named-user licensing, analytics, software updates, and resilient handling of temporary offline or transient technical failures.

Although this sample is implemented as a WPF desktop application, the licensing patterns it demonstrates are not limited to desktop software. Workflows such as entitlement handling, usage tracking, floating sessions, offline fallback, and resilient error handling can also be adapted to web-based products and backend services with a license-administration UI.

Going beyond simple API connectivity, this sample is designed as a production-oriented integration template for applications with a real UI and end-user workflow.

For more information, see the [SLASCONE website](https://slascone.com/), the [Help Center](https://support.slascone.com/), and the [API Test Center](https://api365.slascone.com/swagger).

## Table of Contents

* [Licensing Models](#this-sample-combines-device-based-and-user-based-licensing)
* [Quick Start](#quick-start)
* [What This Sample Demonstrates](#what-this-sample-demonstrates)
* [Main User Workflows](#main-user-workflows)
  * [Online Activation](#online-activation)
  * [Offline Activation](#offline-activation)
  * [Floating Licensing](#floating-licensing)
  * [Offline Usage and Freeride](#offline-usage-and-freeride)
  * [Named User Licensing](#named-user-licensing)
  * [Feature Usage and Analytics](#feature-usage-and-analytics)
  * [Software Updates and Shipment](#software-updates-and-shipment)
* [Connecting to Your SLASCONE Environment](#connecting-to-your-slascone-environment)
* [Offline Storage and Local Persistence](#offline-storage-and-local-persistence)
* [Error Handling and Resilience](#error-handling-and-resilience)
* [Technical Notes](#technical-notes)
* [Project Structure](#project-structure)
* [Further Reading](#further-reading)


## This Sample Combines Device-Based and User-based Licensing

This sample intentionally includes both [device-based licensing](https://support.slascone.com/hc/en-us/articles/360016001677) and [named-user licensing](https://support.slascone.com/hc/en-us/articles/360017647817). It is designed to demonstrate multiple common licensing approaches in one place.

In a production system, you would usually choose one licensing model, not both. Keep the parts that match your product and user scenario, and remove or adapt the rest.

![Image](https://github.com/user-attachments/assets/ba2b1545-420d-499d-b8d1-77e1c9e19f88)


## Quick Start

```bash
# Build the project
dotnet build

# Run the WPF sample
dotnet run --project Slascone.Demo.Wpf.NuGet
```

The application starts with the sample UI and is configured to connect to a SLASCONE demo environment by default.

## What This Sample Demonstrates

This sample showcases how a desktop application can integrate SLASCONE in a realistic end-user workflow.

It demonstrates:

* online license activation
* offline activation based on license files
* regular license heartbeats
* temporary offline fallback using locally stored license data
* floating license session management
* named-user licensing
* feature usage tracking and analytics
* software version and shipment visibility
* response and file integrity validation
* resilient handling of transient technical failures

## Main User Workflows

### Online Activation

The application can activate a license online for the current client or device. It demonstrates how a desktop application can bind a license to a specific machine, handle the activation response, and continue into normal licensed operation.

![OnlineActivation2](https://github.com/user-attachments/assets/ee8f24be-fd53-4d22-98b3-6bdc1b3ca507)

### Offline Activation

For permanently [offline scenarios](https://support.slascone.com/hc/en-us/sections/10214124833693), the sample supports activation based on license files.

This workflow includes:

* validating the digital signature of license files
* reading and displaying license information from XML files
* generating an activation file that can be transferred via a proxy device
* supporting QR code or link-based transfer scenarios

This makes the sample suitable not only for intermittently connected systems, but also for environments with no direct internet access.

The sample supports [protection against multiple activations](https://support.slascone.com/hc/en-us/articles/4412248454161-PERMANENTLY-OFFLINE-SCENARIOS-LICENSE-FILES#multiple-activations), both 2-step activation and preactivation.

![Offline activation flow](https://github.com/user-attachments/assets/4d334242-2827-44b2-8ac0-4b5faaf159d2)

### Floating Licensing

The application automatically recognizes whether the active license is a [floating license](https://support.slascone.com/hc/en-us/articles/360016152858). In that case, it opens and manages a floating session as part of the desktop workflow.

This demonstrates:

* opening a floating session
* keeping the session alive during regular use
* closing the session cleanly
* handling transient failures during session opening without unnecessarily disrupting the user


### Offline Usage and Freeride

The sample supports temporary offline usage by storing the latest valid license state locally and falling back to that state when the licensing service is temporarily unreachable.

This includes:

* storing the latest valid license information
* verifying its digital signature before reuse
* enforcing features, limitations, and expiration locally
* allowing continued operation during the configured freeride period

This helps avoid unnecessary disruption while preserving proper long-term license enforcement.

### Named User Licensing

The sample also demonstrates a [named-user licensing](https://support.slascone.com/hc/en-us/articles/360017647817) scenario. The current implementation uses Azure AD B2C for authentication, but the same licensing principle can be adapted to other identity providers as well.

This is particularly useful for desktop software that needs to combine device-related licensing with user identity.

### Feature Usage and Analytics

The application sends analytics data to SLASCONE in the background and visualizes licensed features in the UI.

When a licensed feature is used, the sample can send a usage heartbeat. This demonstrates how an application can combine licensing and analytics in a way that remains transparent to the end user while still providing valuable product insights.

![Feature usage menu](https://github.com/user-attachments/assets/559622e3-75e7-420c-90c6-f6471f24f6ea)

### Software Updates and Shipment

The sample also demonstrates how [software version](https://support.slascone.com/hc/en-us/sections/28346838426269) information can be transmitted as part of the licensing workflow.

When a heartbeat or activation is performed, the application can send its current version number. The returned license information can then indicate whether a newer version is available. In the sample, this information is displayed in the About box.

![About box](https://github.com/user-attachments/assets/746d1550-9c87-4ad5-9c33-87707ac683f8)

## Connecting to Your SLASCONE Environment

By default, the application is configured to connect to a SLASCONE demo environment.

To connect it to your own SLASCONE environment, adjust the relevant configuration values in `LicensingService.cs`.

For meaningful testing and evaluation, your SLASCONE environment should have at least one active license.

> ⚠️ **Security Warning**: Keep provisioning keys and other secrets secure, and do not embed production secrets in publicly accessible repositories.

## Offline Storage and Local Persistence

For detailed guidance on what should be stored locally, why it matters, and how cached license state supports offline and freeride scenarios, see [What to Store Locally in Your Client](https://support.slascone.com/hc/en-us/articles/7702036319261).

The sample stores licensing-related data locally in order to support temporary offline operation and resilient behavior.

Depending on your implementation, locally stored data may include:

* cached license information
* digital signatures for validating stored license state
* locally relevant floating-session state

Make sure your application stores such data in a location appropriate for desktop software and with suitable access restrictions.

## Error Handling and Resilience

This sample demonstrates how an application can handle SLASCONE-related errors in a way that is resilient but still predictable for the user. For detailed information about SLASCONE API error codes, refer to the [SLASCONE error handling documentation](https://support.slascone.com/hc/en-us/articles/360016160398).

The current implementation applies this logic specifically to:

* license activation
* license heartbeat
* open session handling

### General Approach

When integrating SLASCONE into an application, it is important to distinguish between:

* successful responses that can be processed normally
* functional or business errors such as invalid activation attempts or exceeded floating limits
* transient technical failures such as `429`, `503`, or `504`
* offline or connectivity-related situations where local fallback may be appropriate

### Desktop-Oriented Behavior

The WPF sample demonstrates the following behavior:

* **Activation errors** are shown directly to the user rather than being retried automatically
* **Heartbeat failures** may fall back to the last locally stored license state if the error is technical and temporary
* **Open session failures** distinguish between business logic conflicts and transient technical issues
* **Retry logic** respects `Retry-After` where appropriate and avoids infinite loops

This makes the sample useful not only as a code example, but also as a reference for desktop-oriented UX behavior around licensing.

## Technical Notes

### SLASCONE NuGet Client

This sample is built on the [SLASCONE NuGet Client](https://www.nuget.org/packages/Slascone.Client) rather than direct API calls. The client provides a higher-level integration layer for common licensing workflows and supports features such as offline access, local persistence, and resilient request handling.

### Environment Requirements

* .NET SDK compatible with the sample project
* Internet connectivity for initial activation and online heartbeat operations
* A SLASCONE environment or the configured demo environment

### Dependencies

This application uses:

* the [SLASCONE NuGet Client](https://www.nuget.org/packages/Slascone.Client/)
* MSAL.NET for Azure AD B2C-based authentication in the named-user scenario
* platform-specific mechanisms for device identification

### Cross-Platform Device Identification

The sample includes platform-specific device identification logic for:

* Windows
* Linux
* macOS

### Azure AD B2C

To use Azure AD B2C in a compatible environment, you need to register an application in your Azure AD B2C tenant and configure the sample accordingly. The application uses MSAL.NET to authenticate users and obtain access tokens.

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

## Further Reading

* [API Test Center](https://api365.slascone.com/swagger)
* [What and How to Save in Your Client](https://support.slascone.com/hc/en-us/articles/7702036319261)
* [Digital Signature and Data Integrity](https://support.slascone.com/hc/en-us/articles/360016063637)
* [Error Handling](https://support.slascone.com/hc/en-us/articles/360016160398)
* [Named User Licenses](https://support.slascone.com/hc/en-us/articles/360017647817)
* [Product Analytics](https://support.slascone.com/hc/en-us/articles/360016055537)

