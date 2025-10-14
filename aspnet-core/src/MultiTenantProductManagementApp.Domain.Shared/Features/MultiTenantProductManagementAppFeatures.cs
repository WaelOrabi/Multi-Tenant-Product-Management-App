using Volo.Abp;

namespace MultiTenantProductManagementApp.Features;

public static class MultiTenantProductManagementAppFeatures
{
    public const string GroupName = "MultiTenantProductManagementApp";

    public static class Product
    {
        public const string Variants = GroupName + ".Variants";
    }

    public static class Inventory
    {
        public const string Stock = GroupName + ".Stock";
    }
}
