namespace MultiTenantProductManagementApp.Permissions;

public static class MultiTenantProductManagementAppPermissions
{
    public const string GroupName = "MultiTenantProductManagementApp";



    public static class Products
    {
        public const string Default = GroupName + ".Products";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Stocks
    {
        public const string Default = GroupName + ".Stocks";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
}
