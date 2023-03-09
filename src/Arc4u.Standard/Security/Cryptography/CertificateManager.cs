using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Arc4u.Security.Cryptography;
public class CertificateManager : ICertificateManager
{
    private const string CertificateNameKey = "CertificateName";
    private const string CertificateFindTypeKey = "FindType";
    private const string CertificateStoreLocationKey = "StoreLocation";
    private const string CertificateStoreNameKey = "StoreName";

    public X509Certificate2 FindCertificate(
        string find,
        X509FindType findType = X509FindType.FindBySubjectName,
        StoreLocation location = StoreLocation.LocalMachine,
        StoreName name = StoreName.My)
    {
        var certificateStore = new X509Store(name, location);

        try
        {
            certificateStore.Open(OpenFlags.ReadOnly);

            var certificates = certificateStore.Certificates.Find(findType, find, false);

            return certificates.Count > 0 ? certificates[0] : throw new KeyNotFoundException("No certificate found for the given criteria.");
        }
        finally
        {
            certificateStore.Close();
        }
    }

    public X509Certificate2 FindCertificate(IKeyValueSettings settings)
    {
        var certificateName = settings.Values.ContainsKey(CertificateNameKey) ? settings.Values[CertificateNameKey] : string.Empty;
        var findType = settings.Values.ContainsKey(CertificateFindTypeKey) ? settings.Values[CertificateFindTypeKey] : string.Empty;
        var storeLocation = settings.Values.ContainsKey(CertificateStoreLocationKey) ? settings.Values[CertificateStoreLocationKey] : string.Empty;
        var storeName = settings.Values.ContainsKey(CertificateStoreNameKey) ? settings.Values[CertificateStoreNameKey] : string.Empty;

        if (certificateName == null)
        {
            throw new AppException("No CertificateName key found in the settings provided.");
        }

        var certificateInfo = new CertificateInfo
        {
            Name = certificateName
        };

        if (Enum.TryParse(findType, out X509FindType x509FindType))
        {
            certificateInfo.FindType = x509FindType;
        }
        if (Enum.TryParse(storeLocation, out StoreLocation storeLocation_))
        {
            certificateInfo.Location = storeLocation_;
        }
        if (Enum.TryParse(storeName, out StoreName storeName_))
        {
            certificateInfo.StoreName = storeName_;
        }

        return FindCertificate(certificateInfo);
    }

    public X509Certificate2 FindCertificate(CertificateInfo certificateInfo)
    {
        return FindCertificate(
            certificateInfo.Name,
            certificateInfo.FindType,
            certificateInfo.Location,
            certificateInfo.StoreName);
    }

    public X509Certificate2? FindCertificate(IConfiguration configuration, string sectionName)
    {
        var certificateInfo = configuration.GetSection(sectionName).Get<CertificateInfo>();

        return FindCertificate(certificateInfo);
    }

    /// <summary>
    /// Encrypt a text and return an encrypted version formated in a 64String.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <param name="x509">The certificate used to encrypt</param>
    /// <returns>The encrypted plain text.</returns>
    public string Encrypt(X509Certificate2 x509, string plainText)
    {
        if (x509 == null)
        {
            throw new ArgumentNullException(nameof(x509));
        }

        if (string.IsNullOrWhiteSpace(plainText))
        {
            throw new ArgumentNullException(nameof(plainText));
        }

        var plainBytes = Encoding.UTF8.GetBytes(plainText.Trim());
        byte[]? cipherBytes;

        using (var rsa = x509.GetRSAPublicKey())
        {
            cipherBytes = rsa.Encrypt(plainBytes, RSAEncryptionPadding.OaepSHA256);
        }

        return cipherBytes == null ? string.Empty : Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(X509Certificate2 x509, string base64CypherString)
    {
        if (x509 == null)
        {
            throw new ArgumentNullException(nameof(x509));
        }

        if (string.IsNullOrWhiteSpace(base64CypherString))
        {
            throw new ArgumentNullException(nameof(base64CypherString));
        }

        if (!x509.HasPrivateKey)
        {
            throw new CryptographicException($"The certificate {x509.FriendlyName} has no private key!");
        }

        var cipherBytes = Convert.FromBase64String(base64CypherString);

        using var rsa = x509.GetRSAPrivateKey();
        return Encoding.UTF8.GetString(rsa.Decrypt(cipherBytes, RSAEncryptionPadding.OaepSHA256));
    }
}
