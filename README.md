# SLASCONE-demo-wpf-nuget
A demo client application (WPF) for SLASCONE software licensing, which uses the official NuGet package. Its main purpose is to demonstrate how you can enable online and/or offline licensing (at the same time), while providing a rudimentary/explanatory UI. Although this is a desktop application, the same principles apply for other application types as well. Both named and floating licenses can be used.

![image](https://github.com/SLASCONE/SLASCONE-demo-wpf-nuget/assets/48522942/25f27033-aa99-4713-bf92-5b1b505587f1)

Depending on your application you might need:

- both online and offline mode (most desktop applications)
- online mode only (application servers/backends)
- offline mode only (any application type with no connectivity)

## ONLINE

Online is the recommended licensing mode, since it unleashes the full functionality of SLASCONE.

### ACTIVATION (key based)
The online activation is a very straightforward process, requiring a license key.

![OnlineActivation2](https://github.com/SLASCONE/SLASCONE-demo-wpf-nuget/assets/48522942/c94b4890-8fe1-400b-a33b-b02e23b15f71)

### REFRESH/HEARTBEATS
After a successful activation, the application sends a periodic heartbeat (license check) to ensure that the license parameters are up to date.

![LicensedState](https://github.com/SLASCONE/SLASCONE-demo-wpf-nuget/assets/48522942/2ed2d07c-e6a7-4fcd-8b44-32342da15f44)

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

![image](https://github.com/SLASCONE/SLASCONE-demo-wpf-nuget/assets/48522942/0af8c96f-4873-4c40-b970-aa5dd1409211)

By uploading the activation file, the activation is complete.

## NAMED vs FLOATING

This application automatically recognizes the provisioning mode of the inserted license (named or floating). In case of a floating license, the application opens a session as described [here](https://support.slascone.com/hc/en-us/articles/360016001677-NAMED-DEVICE-LICENSES).
