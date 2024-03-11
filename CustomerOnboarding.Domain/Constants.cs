namespace CustomerOnboarding
{
    public class Constants
    {
    }

    public static class CustomClaimTypes
    {
        public const string Uid = "uid";
    }

    public class RoleNames
    {
        public const string SuperAdmin = "SuperAdmin";// Admin for the tenant
        public const string Admin = "Admin";// Admin for the entire system
        public const string Customer = "Customer";
        public const string Auditor = "Auditor";
        public const string Support = "Support";
    }
}
