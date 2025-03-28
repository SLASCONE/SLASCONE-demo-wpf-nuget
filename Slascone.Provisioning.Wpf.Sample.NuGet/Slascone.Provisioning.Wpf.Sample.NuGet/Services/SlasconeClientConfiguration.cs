using System;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services
{
	public abstract class SlasconeClientConfiguration
	{
		// ===== Main values - Fill according to your environment =====

		// Use this to connect to the Argus Demo 
		// Find your own baseUrl at : https://my.slascone.com/administration/apikeys
		public virtual string ApiBaseUrl => "https://api.slascone.com"; 

		// Find your own Isv Id at : https://my.slascone.com/administration/apikeys
		public virtual Guid IsvId => Guid.Parse("2af5fe02-6207-4214-946e-b00ac5309f53"); 

		// Find your own product key(s) at : https://my.slascone.com/administration/apikeys
		public virtual string ProvisioningKey
			=> "NfEpJ2DFfgczdYqOjvmlgP2O/4VlqmRHXNE9xDXbqZcOwXTbH3TFeBAKKbEzga7D7ashHxFtZOR142LYgKWdNocibDgN75/P58YNvUZafLdaie7eGwI/2gX/XuDPtqDW";

		// Find your own product id key at : https://my.slascone.com/products
		public virtual Guid ProductId => Guid.Parse("b18657cc-1f7c-43fa-e3a4-08da6fa41ad3");

		// ===== Encryption and Digital Signing =====

		// https://support.slascone.com/hc/en-us/articles/360016063637-DIGITAL-SIGNATURE-AND-DATA-INTEGRITY
		// 0 = none, 1 = symmetric, 2 = asymmetric
		// use 0 for initial prototyping, 2 for production
		public virtual int SignatureValidationMode => 2;


		// CHANGE these values according to your environment at: https://my.slascone.com/administration/signature
		// You can work either with pem OR with xml
		public virtual string SignaturePublicKeyPem 
			=> @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAwpigzm+cZIyw6x253YRD
mroGQyo0rO9qpOdbNAkE/FMSX+At5CQT/Cyr0eZTo2h+MO5gn5a6dwg2SYB/K1Yt
yuiKqnaEUfoPnG51KLrj8hi9LoZyIenfsQnxPz+r8XGCUPeS9MhBEVvT4ba0x9Ew
R+krU87VqfI3KNpFQVdLPaZxN4STTEZaet7nReeNtnnZFYaUt5XeNPB0b0rGfrps
y7drmZz81dlWoRcLrBRpkf6XrOTX4yFxe/3HJ8mpukuvdweUBFoQ0xOHmG9pNQ31
AHGtgLYGjbKcW4xYmpDGl0txfcipAr1zMj7X3oCO9lHcFRnXdzx+TTeJYxQX2XVb
hQIDAQAB
-----END PUBLIC KEY-----";

		public virtual string SignaturePublicKeyXml
			=> @"<RSAKeyValue>
  <Modulus>wpigzm+cZIyw6x253YRDmroGQyo0rO9qpOdbNAkE/FMSX+At5CQT/Cyr0eZTo2h+MO5gn5a6dwg2SYB/K1YtyuiKqnaEUfoPnG51KLrj8hi9LoZyIenfsQnxPz+r8XGCUPeS9MhBEVvT4ba0x9EwR+krU87VqfI3KNpFQVdLPaZxN4STTEZaet7nReeNtnnZFYaUt5XeNPB0b0rGfrpsy7drmZz81dlWoRcLrBRpkf6XrOTX4yFxe/3HJ8mpukuvdweUBFoQ0xOHmG9pNQ31AHGtgLYGjbKcW4xYmpDGl0txfcipAr1zMj7X3oCO9lHcFRnXdzx+TTeJYxQX2XVbhQ==</Modulus>
  <Exponent>AQAB</Exponent>
</RSAKeyValue>";
	}

	public class LocalhostClientConfiguration : SlasconeClientConfiguration
	{
		public override string ApiBaseUrl => "https://localhost:44333";
		public override Guid IsvId => Guid.Parse("ffc4524e-3616-4eb2-aa16-d031e71441f3");
		public override string ProvisioningKey => "NfEpJ2DFfgemj+X9DGz2KNOIyJ0KfxjTpGZUUh9JmURS0+TN//DWm7t8HGEjXn6Ov8YUl2RAVPBusLExPkJkzPRVwDFRxIAe+vJ7REYTm4bU5PbT3wpoxdrw4M8h+L45";
		public override string SignaturePublicKeyPem 
			=> @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvo/ZKivRbI5xbgV/YZYn
4KmlYxSJZ7YHFKjb0ZggRE35SHtFaVH3c/ec3J0WkSJk0KyPXs24Q5w/hWFHeG7Z
nde/kZePQ1FaDNqmI75GMAkwe0r9GXuwjmzlZkvG1wO8ZFdgDzI1iT+PkwC7CZQS
0tnZu3zBpvbhIet1mcviG6hwLZKBI+9fDWXDr7Dz51r2f3HURkcZRKYY9ljYgpBJ
I+IGnvgn6aKfArfcPXrrY19QV9e/DMxu8jvxserXcqOzJ+z88O3a8RW2iv6BJ8Vh
oQTCCZNcaX9dDD6XA3DH0HduYIa2KY8ktr1pF0G8WG9+XkmHT+TnzPLNdQqcKJm9
cQIDAQAB
-----END PUBLIC KEY-----";
	}
}
