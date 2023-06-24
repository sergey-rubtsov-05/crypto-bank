namespace CryptoBank.WebAPI.Common.Services;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string hashedPassword, string providedPassword);
}
