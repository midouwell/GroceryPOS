namespace Identity.Api.Domain;

public static class UserRoles
{
    public const string Administrator = "Administrator";
    public const string Manager = "Manager";
    public const string Cashier = "Cashier";

    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Administrator,
        Manager,
        Cashier
    };
}
