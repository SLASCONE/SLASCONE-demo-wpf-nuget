# SLASCONE-demo-wpf-nuget
A demo client application (WPF) for SLASCONE software licensing, which uses the official NuGet package. Its main purpose is to demonstrate how you can enable online and/or offline licensing (at the same time), while providing a rudimentary/explanatory UI. Although this is a desktop application, the same principles apply for other types of applications too. 

![image](https://github.com/SLASCONE/SLASCONE-demo-wpf-nuget/assets/48522942/25f27033-aa99-4713-bf92-5b1b505587f1)



Depending on your scenario you might need:

- both online and offline mode
- online mode only
- offline mode only

## ONLINE 
### ACTIVATION (key based)
The online activation is a very straightforward process, requiring a license key.

![OnlineActivation2](https://github.com/SLASCONE/SLASCONE-demo-wpf-nuget/assets/48522942/c94b4890-8fe1-400b-a33b-b02e23b15f71)

### REFRESH/HEARTBEATS
After a sucessfull activation, the application sends a periodic heartbeat (license check) to ensure that the license parameters are up to date.

![LicensedState](https://github.com/SLASCONE/SLASCONE-demo-wpf-nuget/assets/48522942/2ed2d07c-e6a7-4fcd-8b44-32342da15f44)

### TEMPORARILY OFFLINE
Even if your client was activated online, you should always handle the case of being temporarily offline. This is especially relevant in desktop applications.

### UNASSIGN
The licensing lifecycle for a device ends with its unassigment/deactivation. It is recommended to provide an area in your software, in which the end user can unassign the used license code, so that this can be used on another device (typical hardware migration scenario).

## OFFLINE
Please refer to this [article](https://support.slascone.com/hc/en-us/articles/4412248454161), in order to find more information about permanently offline scenarios.

### ACTIVATION (file based)





