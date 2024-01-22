namespace CryptoBank.WebAPI.Tests.Integration.Features.Auth;

[CollectionDefinition(Name)]
public class AuthTestsCollection : ICollectionFixture<AuthTestFixture>
{
    public const string Name = nameof(AuthTestsCollection);
}
