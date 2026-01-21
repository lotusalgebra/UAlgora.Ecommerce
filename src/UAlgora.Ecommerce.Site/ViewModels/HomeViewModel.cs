using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Site.ViewModels;

public class HomeViewModel
{
    public List<Product> FeaturedProducts { get; set; } = [];
    public List<Product> NewArrivals { get; set; } = [];
    public List<Product> BestSellers { get; set; } = [];
    public List<Product> DealsOfTheDay { get; set; } = [];
    public List<Category> Categories { get; set; } = [];
}

public class ProductListViewModel
{
    public List<Product> Products { get; set; } = [];
    public List<Category> Categories { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? SortBy { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? OnSale { get; set; }
    public bool? InStock { get; set; }
}

public class ProductDetailViewModel
{
    public Product Product { get; set; } = null!;
    public List<Product> RelatedProducts { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public Category? Category { get; set; }
}

public class CartViewModel
{
    public Cart? Cart { get; set; }
    public List<CartItemViewModel> Items { get; set; } = [];
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Shipping { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public string? DiscountCode { get; set; }
}

public class CartItemViewModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public class CheckoutViewModel
{
    public CartViewModel Cart { get; set; } = new();
    public CheckoutStep CurrentStep { get; set; } = CheckoutStep.Information;
    public CustomerInfoModel CustomerInfo { get; set; } = new();
    public ShippingAddressModel ShippingAddress { get; set; } = new();
    public BillingAddressModel BillingAddress { get; set; } = new();
    public List<ShippingMethodModel> ShippingMethods { get; set; } = [];
    public string? SelectedShippingMethod { get; set; }
    public List<PaymentMethodModel> PaymentMethods { get; set; } = [];
    public string? SelectedPaymentMethod { get; set; }
    public bool SameAsBilling { get; set; } = true;
}

public enum CheckoutStep
{
    Information,
    Shipping,
    Payment,
    Review
}

public class CustomerInfoModel
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class ShippingAddressModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Address1 { get; set; } = string.Empty;
    public string? Address2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "US";
    public string Phone { get; set; } = string.Empty;
}

public class BillingAddressModel : ShippingAddressModel
{
}

public class ShippingMethodModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string EstimatedDelivery { get; set; } = string.Empty;
}

public class PaymentMethodModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public class OrderConfirmationViewModel
{
    public Order Order { get; set; } = null!;
    public string OrderNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class AccountViewModel
{
    public Customer? Customer { get; set; }
    public List<Order> RecentOrders { get; set; } = [];
    public List<Address> Addresses { get; set; } = [];
    public List<Wishlist> Wishlists { get; set; } = [];
}

public class WishlistViewModel
{
    public Wishlist? Wishlist { get; set; }
    public List<WishlistItemViewModel> Items { get; set; } = [];
}

public class WishlistItemViewModel
{
    public Guid Id { get; set; }
    public Product Product { get; set; } = null!;
    public DateTime AddedAt { get; set; }
}
