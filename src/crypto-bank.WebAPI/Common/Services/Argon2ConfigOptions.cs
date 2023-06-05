namespace crypto_bank.WebAPI.Common.Services;

internal record Argon2ConfigOptions
{
    public int TimeCost { get; init; }
    public int MemoryCostInMb { get; init; }
    public int Threads { get; init; }
    public int Lanes { get; init; }
    public int HashLengthInBytes { get; init; }
}
