namespace crypto_bank.Domain.Authorization;

[Flags]
public enum PolicyNames
{
    UserRole = 1,
    AdministratorRole = 2,
}
