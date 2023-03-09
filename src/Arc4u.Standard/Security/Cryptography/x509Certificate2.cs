using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace Arc4u.Security.Cryptography;

public static class Certificate
{
    private static readonly Lazy<ICertificateManager> Instance = new(() => new CertificateManager());

    public static X509Certificate2 FindCertificate(string find, X509FindType findType = X509FindType.FindBySubjectName, StoreLocation location = StoreLocation.LocalMachine, StoreName name = StoreName.My)
        => Instance.Value.FindCertificate(find, findType, location, name);

    public static X509Certificate2 FindCertificate(IKeyValueSettings settings)
        => Instance.Value.FindCertificate(settings);

    public static X509Certificate2 FindCertificate(CertificateInfo certificateInfo)
        => Instance.Value.FindCertificate(certificateInfo);

    public static X509Certificate2? FindCertificate(IConfiguration configuration, string sectionName)
        => Instance.Value.FindCertificate(configuration, sectionName);

    /// <inheritdoc cref="ICertificateManager.Encrypt"/>
    public static string Encrypt(this X509Certificate2 x509, string plainText)
        => Instance.Value.Encrypt(x509, plainText);

    public static string Decrypt(this X509Certificate2 x509, string base64CypherString)
        => Instance.Value.Decrypt(x509, base64CypherString);
}
