using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace Arc4u.Security.Cryptography;

public interface ICertificateManager
{
    X509Certificate2 FindCertificate(string find, X509FindType findType = X509FindType.FindBySubjectName, StoreLocation location = StoreLocation.LocalMachine, StoreName name = StoreName.My);
    X509Certificate2 FindCertificate(IKeyValueSettings settings);
    X509Certificate2 FindCertificate(CertificateInfo certificateInfo);
    X509Certificate2? FindCertificate(IConfiguration configuration, string sectionName);

    /// <summary>
    /// Encrypts a text and return an encrypted version encoded in a 64String.
    /// </summary>
    /// <param name="x509">The certificate used to encrypt</param>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <returns>The encrypted plain text.</returns>
    string Encrypt(X509Certificate2 x509, string plainText);

    string Decrypt(X509Certificate2 x509, string base64CypherString);
}
