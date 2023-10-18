namespace CryptoBank.WebAPI.Tests.Integration.Features.Deposits;

[CollectionDefinition(Name)]
public class DepositsTestsCollection : ICollectionFixture<DepositsTestFixture>
{
    public const string Name = nameof(DepositsTestsCollection);
}
