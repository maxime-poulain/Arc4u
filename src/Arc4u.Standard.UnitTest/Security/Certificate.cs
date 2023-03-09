using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Arc4u.Security.Cryptography;
using FluentAssertions;
using System.Security.Cryptography.X509Certificates;

namespace Arc4u.Standard.UnitTest.Security;

[Trait("Category", "CI")]
public class CertificateTests
{
    public CertificateTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void decrypting_a_encrypted_plain_text_returns_the_original_plain_text()
    {
        // arrange
        var publicCert = @".\Configs\cert.pem";
        var privateCert = @".\Configs\key.pem";
        var plainText = "FileCertificateShouldBe()";

        // act
        var certificate = X509Certificate2.CreateFromPemFile(publicCert, privateCert);
        var cypherText = certificate.Encrypt(plainText);
        var sut = certificate.Decrypt(cypherText);

        // assert
        certificate.Should().NotBeNull();
        sut.Should().Be(plainText);
    }

    [Fact]
    public void decrypting_with_manager_class_a_encrypted_plain_text_returns_the_original_plain_text()
    {
        // Arrange
        var publicCert = @".\Configs\cert.pem";
        var privateCert = @".\Configs\key.pem";
        var plainText = "FileCertificateShouldBe()";
        var manager = new CertificateManager();

        // act
        var certificate = X509Certificate2.CreateFromPemFile(publicCert, privateCert);
        var cypherText = manager.Encrypt(certificate, plainText);
        var sut = manager.Decrypt(certificate, cypherText);

        // assert
        certificate.Should().NotBeNull();
        sut.Should().Be(plainText);
    }
}
