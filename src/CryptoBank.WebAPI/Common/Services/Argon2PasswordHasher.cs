using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Options;

namespace CryptoBank.WebAPI.Common.Services;

internal class Argon2PasswordHasher : IPasswordHasher
{
    private readonly Encoding _encoding;
    private readonly Argon2ConfigOptions _options;

    public Argon2PasswordHasher(IOptions<Argon2ConfigOptions> options)
    {
        _encoding = Encoding.UTF8;
        _options = options.Value;
    }

    public string Hash(string password)
    {
        var saltInBytes = RandomNumberGenerator.GetBytes(32);

        var config = new Argon2Config
        {
            Salt = saltInBytes,
            Password = _encoding.GetBytes(password),
            TimeCost = _options.TimeCost,
            MemoryCost = _options.MemoryCostInMb * 1024,
            Threads = _options.Threads,
            Lanes = _options.Lanes,
            HashLength = _options.HashLengthInBytes,
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
