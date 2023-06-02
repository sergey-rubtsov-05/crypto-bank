using System.Text;
using Isopoh.Cryptography.Argon2;

namespace crypto_bank.WebAPI.Common.Services;

internal class PasswordHasher : IPasswordHasher
{
    private readonly Encoding _encoding;

    public PasswordHasher()
    {
        _encoding = Encoding.UTF8;
    }

    public string Hash(string password, string salt)
    {
        var config = new Argon2Config
        {
            Salt = _encoding.GetBytes(salt),
            Password = _encoding.GetBytes(password),
            TimeCost = 3,
            MemoryCost = 8192,
            Threads = 1,
            Lanes = 1,
            HashLength = 32,
            Version = Argon2Version.Nineteen,
        };

        var hashedPassword = Argon2.Hash(config);

        return hashedPassword;
    }

    public bool Verify(string encodedHash, string password)
    {
        return Argon2.Verify(encodedHash, _encoding.GetBytes(password));
    }
}
