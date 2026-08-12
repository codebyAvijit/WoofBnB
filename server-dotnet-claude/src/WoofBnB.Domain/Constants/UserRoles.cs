namespace WoofBnB.Domain.Constants;

/// <summary>Mirrors server/src/modules/auth/auth.model.js's role enum: ["admin", "super_admin"].</summary>
public static class UserRoles
{
    public const string Admin = "admin";
    public const string SuperAdmin = "super_admin";

    public static readonly IReadOnlyList<string> All = [Admin, SuperAdmin];
}
