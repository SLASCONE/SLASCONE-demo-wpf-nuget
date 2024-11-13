using Slascone.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Slascone.Client;
using Slascone.Client.DeviceInfos;
using Slascone.Client.Xml;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services.SimulateNoInternet
{
    // ------------------------------------------------------------------------------
    // The only purpose of this decorator is to provide a decorated Provisioning
    // client, that simulates a temporarily lost internet connection.
    // You don't need this in a real application.
    // ------------------------------------------------------------------------------

    internal class SlasconeClientV2NoInternetDecorator : ISlasconeClientV2
    {
        #region Members

        private ISlasconeClientV2 _decoratedSlasconeClientV2;

        #endregion

        #region Construction

        public SlasconeClientV2NoInternetDecorator(ISlasconeClientV2 decoratedSlasconeClientV2)
        {
            _decoratedSlasconeClientV2 = decoratedSlasconeClientV2;
            Provisioning =
                new SlasconeProvisioningClientV2NoInternetDecorator(_decoratedSlasconeClientV2.Provisioning);
        }

		#endregion

		#region Implementation of ISlasconeClientV2

		// ------------------------------------------------------------------------------
        // Except for the Provisioning client mapper below, all ISlasconeClientV2 methods
        // are directly mapped to the decorated instance.
		// ------------------------------------------------------------------------------
        
		public ISlasconeClientV2 SetSignatureValidationMode(int signatureValidationMode)
        {
            _decoratedSlasconeClientV2.SetSignatureValidationMode(signatureValidationMode);
            return this;
        }

        public ISlasconeClientV2 SetAppDataFolder(string appDataFolder)
        {
            _decoratedSlasconeClientV2.SetAppDataFolder(appDataFolder);
            return this;
        }

        public ISlasconeClientV2 SetSignatureCertificate(byte[] rawData)
        {
            _decoratedSlasconeClientV2.SetSignatureCertificate(rawData);
            return this;
        }

        public ISlasconeClientV2 SetSignaturePublicKeyXml(string publicKeyXml)
        {
            _decoratedSlasconeClientV2.SetSignaturePublicKeyXml(publicKeyXml);
            return this;
        }

        public ISlasconeClientV2 SetSignaturePublicKey(PublicKey publicKey)
        {
            _decoratedSlasconeClientV2.SetSignaturePublicKey(publicKey);
            return this;
        }

        public ISlasconeClientV2 SetSignatureSymmetricKey(string symmetricEncryptionKey)
        {
            _decoratedSlasconeClientV2.SetSignatureSymmetricKey(symmetricEncryptionKey);
            return this;
        }

        public ISlasconeClientV2 SetProvisioningKey(string provisioningKey)
        {
            _decoratedSlasconeClientV2.SetProvisioningKey(provisioningKey);
            return this;
        }

        public ISlasconeClientV2 SetAdminKey(string adminKey)
        {
            _decoratedSlasconeClientV2.SetAdminKey(adminKey);
            return this;
        }

        public ISlasconeClientV2 SetBearer(string bearer)
        {
            _decoratedSlasconeClientV2.SetBearer(bearer);
            return this;
        }

        public ISlasconeClientV2 SetLastModifiedByHeader(string lastModifiedById)
        {
            _decoratedSlasconeClientV2.SetLastModifiedByHeader(lastModifiedById);
            return this;
        }

        public ISlasconeClientV2 SetHttpClientTimeout(TimeSpan timeout)
        {
            _decoratedSlasconeClientV2.SetHttpClientTimeout(timeout);
            return this;
        }

        public ISlasconeClientV2 SetCheckHttpsCertificate(bool check = true)
        {
            _decoratedSlasconeClientV2.SetCheckHttpsCertificate(check);
            return this;
        }

        public bool IsFileSignatureValid(string licenseFilePath)
        {
            return _decoratedSlasconeClientV2.IsFileSignatureValid(licenseFilePath);
        }

        public LicenseInfoDto ReadLicenseFile(string licenseFilePath)
        {
            return _decoratedSlasconeClientV2.ReadLicenseFile(licenseFilePath);
        }

        public ActivationDto ReadActivationFile(string activationFilePath)
        {
            return _decoratedSlasconeClientV2.ReadActivationFile(activationFilePath);
        }

        public ApiResponse<LicenseInfoDto> GetOfflineLicense()
        {
            return _decoratedSlasconeClientV2.GetOfflineLicense();
        }

        public ISlasconeClientV2 SetHttpRequestHeader(string headerName, string headerValue)
        {
			_decoratedSlasconeClientV2.SetHttpRequestHeader(headerName, headerValue);
			return this;
		}

        public ISlasconeClientV2 AddHttpRequestHeader(string headerName, string headerValue)
        {
			_decoratedSlasconeClientV2.AddHttpRequestHeader(headerName, headerValue);
			return this;
		}

        public bool TryGetHttpRequestHeader(string headerName, out IEnumerable<string> values)
        {
			return _decoratedSlasconeClientV2.TryGetHttpRequestHeader(headerName, out values);
		}

        public bool IsReleaseCompliant(LicenseInfoDto licenseInfo, string release)
        {
	        return _decoratedSlasconeClientV2.IsReleaseCompliant(licenseInfo, release);
        }

        public ISlasconeCustomerClientV2 Customer => _decoratedSlasconeClientV2.Customer;

        public ISlasconeDataExchangeClientV2 DataExchange => _decoratedSlasconeClientV2.DataExchange;

        public ISlasconeDataGatheringClientV2 DataGathering => _decoratedSlasconeClientV2.DataGathering;
        
        public ISlasconeTemplateClientV2 Template => _decoratedSlasconeClientV2.Template;

        public ISlasconeLicenseClientV2 License => _decoratedSlasconeClientV2.License;

        public ISlasconeLookupClientV2 Lookup => _decoratedSlasconeClientV2.Lookup;

        public ISlasconeProvisioningClientV2 Provisioning { get; }

        public IEnumerable<CertificateInfoDto> HttpsChainOfTrust => _decoratedSlasconeClientV2.HttpsChainOfTrust;

        public ISlasconeSessionClient Session => _decoratedSlasconeClientV2.Session;

        #endregion
    }
}
