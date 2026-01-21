namespace UAlgora.Ecommerce.Core.Constants;

/// <summary>
/// Product status indicating visibility and availability.
/// </summary>
public enum ProductStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}

/// <summary>
/// Stock availability status.
/// </summary>
public enum StockStatus
{
    InStock = 0,
    OutOfStock = 1,
    OnBackorder = 2,
    PreOrder = 3
}

/// <summary>
/// Order lifecycle status.
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Completed = 5,
    Cancelled = 6,
    Refunded = 7,
    OnHold = 8,
    Failed = 9
}

/// <summary>
/// Payment transaction status.
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Authorized = 1,
    Captured = 2,
    PartiallyRefunded = 3,
    Refunded = 4,
    Failed = 5,
    Voided = 6
}

/// <summary>
/// Order fulfillment status.
/// </summary>
public enum FulfillmentStatus
{
    Unfulfilled = 0,
    PartiallyFulfilled = 1,
    Fulfilled = 2,
    Returned = 3,
    PartiallyReturned = 4
}

/// <summary>
/// Customer account status.
/// </summary>
public enum CustomerStatus
{
    Active = 0,
    Inactive = 1,
    Suspended = 2
}

/// <summary>
/// Discount type indicating how the discount is applied.
/// </summary>
public enum DiscountType
{
    Percentage = 0,
    FixedAmount = 1,
    FreeShipping = 2,
    BuyXGetY = 3
}

/// <summary>
/// Discount application scope.
/// </summary>
public enum DiscountScope
{
    Order = 0,
    Product = 1,
    Category = 2,
    Shipping = 3
}

/// <summary>
/// Address type for customer addresses.
/// </summary>
public enum AddressType
{
    Shipping = 0,
    Billing = 1,
    Both = 2
}

/// <summary>
/// Shipment status.
/// </summary>
public enum ShipmentStatus
{
    Pending = 0,
    LabelCreated = 1,
    PickedUp = 2,
    InTransit = 3,
    OutForDelivery = 4,
    Delivered = 5,
    Exception = 6,
    Returned = 7
}

/// <summary>
/// Payment method types.
/// </summary>
public enum PaymentMethodType
{
    CreditCard = 0,
    DebitCard = 1,
    PayPal = 2,
    Stripe = 3,
    BankTransfer = 4,
    CashOnDelivery = 5,
    GiftCard = 6,
    StoreCredit = 7,
    Other = 99
}

/// <summary>
/// Product sorting options.
/// </summary>
public enum ProductSortBy
{
    Newest = 0,
    Oldest = 1,
    PriceLowToHigh = 2,
    PriceHighToLow = 3,
    NameAscending = 4,
    NameDescending = 5,
    BestSelling = 6,
    TopRated = 7
}

/// <summary>
/// Order sorting options.
/// </summary>
public enum OrderSortBy
{
    Newest = 0,
    Oldest = 1,
    TotalHighToLow = 2,
    TotalLowToHigh = 3,
    Status = 4
}
