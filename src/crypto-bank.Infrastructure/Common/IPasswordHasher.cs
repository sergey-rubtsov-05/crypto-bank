namespace crypto_bank.Infrastructure.Common;

public interface IPasswordHasher
{
    string Hash(string password, string salt);
}
