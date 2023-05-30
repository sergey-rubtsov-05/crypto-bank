namespace crypto_bank.WebAPI.Common.Services;

public interface IPasswordHasher
{
    string Hash(string password, string salt);
}
