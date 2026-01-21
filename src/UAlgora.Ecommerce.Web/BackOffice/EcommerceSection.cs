namespace UAlgora.Ecommerce.Web.BackOffice;

/// <summary>
/// Constants for the e-commerce backoffice integration.
/// </summary>
public static class EcommerceConstants
{
    /// <summary>
    /// The name used for the Swagger API group.
    /// </summary>
    public const string ApiName = "Ecommerce Management API";

    /// <summary>
    /// Base route for all e-commerce management API endpoints.
    /// </summary>
    public const string ApiRouteBase = "ecommerce";

    public static class Routes
    {
        public const string Products = "product";
        public const string Categories = "category";
        public const string Orders = "order";
        public const string Customers = "customer";
        public const string Discounts = "discount";
        public const string Dashboard = "dashboard";
    }

    public static class Icons
    {
        public const string Section = "icon-shopping-basket-alt-2";
        public const string Products = "icon-box";
        public const string Product = "icon-box";
        public const string Categories = "icon-folders";
        public const string Category = "icon-folder";
        public const string Orders = "icon-receipt-dollar";
        public const string Order = "icon-receipt";
        public const string Customers = "icon-users";
        public const string Customer = "icon-user";
        public const string Discounts = "icon-tag";
        public const string Discount = "icon-tag";
        public const string Settings = "icon-settings";
    }
}
