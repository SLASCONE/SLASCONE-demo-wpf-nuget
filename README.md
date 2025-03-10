# LICENSING & ANALYTICS FOR SOFTWARE AND IoT VENDORS
A demo client application (WPF) for SLASCONE software licensing, which uses the official NuGet package. Its main purpose is to demonstrate how you can enable online and/or offline licensing (at the same time), while providing a rudimentary/explanatory UI. Although this is a desktop application, the same principles apply for other application types as well. Both named and floating licenses can be used.

![Image](https://github.com/user-attachments/assets/ba2b1545-420d-499d-b8d1-77e1c9e19f88)

Depending on your application you might need:

- both online and offline mode (most desktop applications)
- online mode only (application servers/backends)
- offline mode only (any application type with no connectivity)

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

### License feature visualization

If a valid license is present, the application shows licensed features as menu items in the features menu.

![Image](https://github.com/user-attachments/assets/559622e3-75e7-420c-90c6-f6471f24f6ea)

## NAMED vs FLOATING

This application automatically recognizes the provisioning mode of the inserted license (named or floating). In case of a floating license, the application opens a session as described [here](https://support.slascone.com/hc/en-us/articles/360016001677-NAMED-DEVICE-LICENSES).

## User based licensing
This application also depicts user based licensing. 
Please refer to this [article](https://support.slascone.com/hc/en-us/articles/360017647817-NAMED-USER-LICENSES) to find more information about named user licenses.

### AZURE AD B2C
This application uses Azure AD B2C for user authentication. In order to use Azure AD B2C, you need to register an application in your Azure AD B2C tenant and configure the application to use Azure AD B2C for authentication. The application uses the MSAL.NET library to authenticate users with Azure AD B2C and obtain access tokens to access APIs.
