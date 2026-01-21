using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using Umbraco.Cms.Api.Management.Routing;

using ServiceCreateCustomerRequest = UAlgora.Ecommerce.Core.Interfaces.Services.CreateCustomerRequest;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for customer operations in the Umbraco backoffice.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/{EcommerceConstants.Routes.Customers}")]
public class CustomerManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerManagementApiController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Gets the tree structure for customer views.
    /// </summary>
    [HttpGet("tree")]
    [ProducesResponseType<CustomerTreeResponse>(StatusCodes.Status200OK)]
    public IActionResult GetTree()
    {
        var nodes = new List<CustomerTreeNodeModel>
        {
            new()
            {
                Id = "all-customers",
                Name = "All Customers",
                Icon = EcommerceConstants.Icons.Customers,
                HasChildren = true,
                FilterType = "all"
            },
            new()
            {
                Id = "top-spenders",
                Name = "Top Spenders",
                Icon = "icon-medal",
                HasChildren = true,
                FilterType = "top-spenders"
            },
            new()
            {
                Id = "recent-customers",
                Name = "Recently Active",
                Icon = "icon-calendar",
                HasChildren = true,
                FilterType = "recent"
            }
        };

        return Ok(new CustomerTreeResponse { Nodes = nodes });
    }

    /// <summary>
    /// Gets customers for a specific tree node filter.
    /// </summary>
    [HttpGet("tree/{nodeId}/children")]
    [ProducesResponseType<CustomerListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTreeChildren(string nodeId, [FromQuery] int take = 50)
    {
        IEnumerable<Core.Models.Domain.Customer> customers;

        switch (nodeId)
        {
            case "top-spenders":
                customers = await _customerService.GetTopBySpentAsync(take);
                break;

            case "recent-customers":
                var parameters = new CustomerQueryParameters { Page = 1, PageSize = take };
                var result = await _customerService.GetPagedAsync(parameters);
                customers = result.Items
                    .Where(c => c.LastOrderAt.HasValue)
                    .OrderByDescending(c => c.LastOrderAt)
                    .Take(take);
                break;

            case "all-customers":
            default:
                var allParams = new CustomerQueryParameters { Page = 1, PageSize = take };
                var allResult = await _customerService.GetPagedAsync(allParams);
                customers = allResult.Items;
                break;
        }

        var items = customers.Select(MapToCustomerItem).ToList();

        return Ok(new CustomerListResponse
        {
            Items = items,
            Total = items.Count,
            Skip = 0,
            Take = take
        });
    }

    /// <summary>
    /// Gets a paged list of customers.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<CustomerListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] string? search = null)
    {
        var parameters = new CustomerQueryParameters
        {
            Page = (skip / take) + 1,
            PageSize = take,
            SearchTerm = search
        };

        var result = await _customerService.GetPagedAsync(parameters);

        return Ok(new CustomerListResponse
        {
            Items = result.Items.Select(MapToCustomerItem).ToList(),
            Total = result.TotalCount,
            Skip = skip,
            Take = take
        });
    }

    /// <summary>
    /// Gets a single customer by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CustomerDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        return Ok(MapToCustomerDetail(customer));
    }

    /// <summary>
    /// Gets customer statistics.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType<CustomerStatisticsModel>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics()
    {
        var parameters = new CustomerQueryParameters { Page = 1, PageSize = 1 };
        var result = await _customerService.GetPagedAsync(parameters);
        var topSpenders = await _customerService.GetTopBySpentAsync(1);

        return Ok(new CustomerStatisticsModel
        {
            TotalCustomers = result.TotalCount,
            TopSpender = topSpenders.FirstOrDefault()?.TotalSpent ?? 0
        });
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<CustomerDetailModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest(new { message = "Email, First Name, and Last Name are required" });
        }

        try
        {
            var serviceRequest = new ServiceCreateCustomerRequest
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                Company = request.Company,
                Tags = request.Tags,
                AcceptsMarketing = request.AcceptsMarketing
            };

            var created = await _customerService.CreateAsync(serviceRequest);

            // Update additional fields not in service request
            if (!string.IsNullOrEmpty(request.Notes) || !string.IsNullOrEmpty(request.Status))
            {
                created.Notes = request.Notes;
                if (!string.IsNullOrEmpty(request.Status) &&
                    Enum.TryParse<CustomerStatus>(request.Status, true, out var status))
                {
                    created.Status = status;
                }
                created = await _customerService.UpdateAsync(created);
            }

            // Add addresses if provided
            if (request.Addresses?.Any() == true)
            {
                foreach (var addr in request.Addresses)
                {
                    var address = MapToAddress(addr);
                    await _customerService.AddAddressAsync(created.Id, address);
                }
                created = await _customerService.GetByIdAsync(created.Id);
            }

            return CreatedAtAction(nameof(GetById), new { id = created!.Id }, MapToCustomerDetail(created));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<CustomerDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        var existing = await _customerService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        try
        {
            existing.Email = request.Email ?? existing.Email;
            existing.FirstName = request.FirstName ?? existing.FirstName;
            existing.LastName = request.LastName ?? existing.LastName;
            existing.Phone = request.Phone;
            existing.Company = request.Company;
            existing.Notes = request.Notes;
            existing.Tags = request.Tags ?? existing.Tags;
            existing.AcceptsMarketing = request.AcceptsMarketing;

            if (!string.IsNullOrEmpty(request.Status) &&
                Enum.TryParse<CustomerStatus>(request.Status, true, out var status))
            {
                existing.Status = status;
            }

            var updated = await _customerService.UpdateAsync(existing);

            return Ok(MapToCustomerDetail(updated));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a customer.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _customerService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        await _customerService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Gets orders for a specific customer.
    /// </summary>
    [HttpGet("{id:guid}/orders")]
    [ProducesResponseType<CustomerOrderListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerOrders(
        Guid id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        var orders = await _customerService.GetOrderHistoryAsync(id);
        var orderedList = orders.OrderByDescending(o => o.CreatedAt).ToList();

        var pagedOrders = orderedList.Skip(skip).Take(take).Select(o => new CustomerOrderItemModel
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            Status = o.Status.ToString(),
            GrandTotal = o.GrandTotal,
            CurrencyCode = o.CurrencyCode,
            ItemCount = o.Lines.Count,
            CreatedAt = o.CreatedAt
        }).ToList();

        return Ok(new CustomerOrderListResponse
        {
            Items = pagedOrders,
            Total = orderedList.Count,
            Skip = skip,
            Take = take
        });
    }

    /// <summary>
    /// Adds or removes loyalty points for a customer.
    /// </summary>
    [HttpPost("{id:guid}/loyalty-points")]
    [ProducesResponseType<CustomerDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AdjustLoyaltyPoints(Guid id, [FromBody] AdjustLoyaltyPointsRequest request)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        try
        {
            await _customerService.AddLoyaltyPointsAsync(id, request.Points, request.Reason);
            var updated = await _customerService.GetByIdAsync(id);
            return Ok(MapToCustomerDetail(updated!));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Adds or removes store credit for a customer.
    /// </summary>
    [HttpPost("{id:guid}/store-credit")]
    [ProducesResponseType<CustomerDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AdjustStoreCredit(Guid id, [FromBody] AdjustStoreCreditRequest request)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        try
        {
            await _customerService.AddStoreCreditAsync(id, request.Amount, request.Reason);
            var updated = await _customerService.GetByIdAsync(id);
            return Ok(MapToCustomerDetail(updated!));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Adds an address to a customer.
    /// </summary>
    [HttpPost("{id:guid}/addresses")]
    [ProducesResponseType<CustomerDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddAddress(Guid id, [FromBody] CustomerAddressRequest request)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        try
        {
            var address = MapToAddress(request);
            await _customerService.AddAddressAsync(id, address);
            var updated = await _customerService.GetByIdAsync(id);
            return Ok(MapToCustomerDetail(updated!));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an address for a customer.
    /// </summary>
    [HttpPut("{id:guid}/addresses/{addressId:guid}")]
    [ProducesResponseType<CustomerDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAddress(Guid id, Guid addressId, [FromBody] CustomerAddressRequest request)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        try
        {
            var address = MapToAddress(request);
            address.Id = addressId;
            await _customerService.UpdateAddressAsync(address);
            var updated = await _customerService.GetByIdAsync(id);
            return Ok(MapToCustomerDetail(updated!));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes an address from a customer.
    /// </summary>
    [HttpDelete("{id:guid}/addresses/{addressId:guid}")]
    [ProducesResponseType<CustomerDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAddress(Guid id, Guid addressId)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        await _customerService.DeleteAddressAsync(id, addressId);
        var updated = await _customerService.GetByIdAsync(id);
        return Ok(MapToCustomerDetail(updated!));
    }

    /// <summary>
    /// Toggles the customer's status.
    /// </summary>
    [HttpPost("{id:guid}/toggle-status")]
    [ProducesResponseType<CustomerDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        // Cycle through statuses: Active -> Suspended -> Inactive -> Active
        customer.Status = customer.Status switch
        {
            CustomerStatus.Active => CustomerStatus.Suspended,
            CustomerStatus.Suspended => CustomerStatus.Inactive,
            CustomerStatus.Inactive => CustomerStatus.Active,
            _ => CustomerStatus.Active
        };

        var updated = await _customerService.UpdateAsync(customer);
        return Ok(MapToCustomerDetail(updated));
    }

    /// <summary>
    /// Suspends a customer.
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType<CustomerDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Suspend(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        customer.Status = CustomerStatus.Suspended;
        var updated = await _customerService.UpdateAsync(customer);
        return Ok(MapToCustomerDetail(updated));
    }

    /// <summary>
    /// Activates a customer.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType<CustomerDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        customer.Status = CustomerStatus.Active;
        var updated = await _customerService.UpdateAsync(customer);
        return Ok(MapToCustomerDetail(updated));
    }

    /// <summary>
    /// Toggles the customer's marketing opt-in status.
    /// </summary>
    [HttpPost("{id:guid}/toggle-marketing")]
    [ProducesResponseType<CustomerDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleMarketing(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        customer.AcceptsMarketing = !customer.AcceptsMarketing;
        if (customer.AcceptsMarketing)
        {
            customer.MarketingConsentAt = DateTime.UtcNow;
        }

        var updated = await _customerService.UpdateAsync(customer);
        return Ok(MapToCustomerDetail(updated));
    }

    /// <summary>
    /// Marks a customer's email as verified.
    /// </summary>
    [HttpPost("{id:guid}/verify-email")]
    [ProducesResponseType<CustomerDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyEmail(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        customer.EmailVerified = true;
        customer.EmailVerifiedAt = DateTime.UtcNow;

        var updated = await _customerService.UpdateAsync(customer);
        return Ok(MapToCustomerDetail(updated));
    }

    /// <summary>
    /// Resets loyalty points for a customer.
    /// </summary>
    [HttpPost("{id:guid}/reset-points")]
    [ProducesResponseType<CustomerDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPoints(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        // Deduct all current points
        if (customer.LoyaltyPoints > 0)
        {
            await _customerService.AddLoyaltyPointsAsync(id, -customer.LoyaltyPoints, "Points reset by admin");
        }

        var updated = await _customerService.GetByIdAsync(id);
        return Ok(MapToCustomerDetail(updated!));
    }

    private static Core.Models.Domain.Address MapToAddress(CustomerAddressRequest request)
    {
        return new Core.Models.Domain.Address
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Company = request.Company,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            StateProvince = request.StateProvince,
            PostalCode = request.PostalCode,
            Country = request.Country,
            Phone = request.Phone
        };
    }

    private static CustomerItemModel MapToCustomerItem(Core.Models.Domain.Customer customer)
    {
        return new CustomerItemModel
        {
            Id = customer.Id,
            Email = customer.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            FullName = customer.FullName,
            Phone = customer.Phone,
            TotalSpent = customer.TotalSpent,
            TotalOrders = customer.TotalOrders,
            LastOrderAt = customer.LastOrderAt,
            CreatedAt = customer.CreatedAt
        };
    }

    private static CustomerDetailModel MapToCustomerDetail(Core.Models.Domain.Customer customer)
    {
        return new CustomerDetailModel
        {
            Id = customer.Id,
            Email = customer.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            FullName = customer.FullName,
            Phone = customer.Phone,
            Company = customer.Company,
            TotalSpent = customer.TotalSpent,
            TotalOrders = customer.TotalOrders,
            LastOrderAt = customer.LastOrderAt,
            Notes = customer.Notes,
            Tags = customer.Tags,
            UmbracoMemberId = customer.UmbracoMemberId,
            Addresses = customer.Addresses.Select(a => new CustomerAddressModel
            {
                FirstName = a.FirstName,
                LastName = a.LastName,
                Company = a.Company,
                AddressLine1 = a.AddressLine1,
                AddressLine2 = a.AddressLine2,
                City = a.City,
                StateProvince = a.StateProvince,
                PostalCode = a.PostalCode,
                Country = a.Country,
                Phone = a.Phone
            }).ToList(),
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            Status = customer.Status.ToString(),
            LoyaltyPoints = customer.LoyaltyPoints,
            StoreCreditBalance = customer.StoreCreditBalance,
            CustomerTier = customer.CustomerTier,
            AverageOrderValue = customer.AverageOrderValue,
            AcceptsMarketing = customer.AcceptsMarketing,
            Source = customer.Source
        };
    }
}

#region Response Models

public class CustomerTreeResponse
{
    public List<CustomerTreeNodeModel> Nodes { get; set; } = [];
}

public class CustomerTreeNodeModel
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Icon { get; set; }
    public bool HasChildren { get; set; }
    public required string FilterType { get; set; }
}

public class CustomerListResponse
{
    public List<CustomerItemModel> Items { get; set; } = [];
    public int Total { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

public class CustomerItemModel
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string FullName { get; set; }
    public string? Phone { get; set; }
    public decimal TotalSpent { get; set; }
    public int TotalOrders { get; set; }
    public DateTime? LastOrderAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CustomerDetailModel : CustomerItemModel
{
    public string? Company { get; set; }
    public string? Notes { get; set; }
    public List<string> Tags { get; set; } = [];
    public int? UmbracoMemberId { get; set; }
    public List<CustomerAddressModel> Addresses { get; set; } = [];
    public DateTime? UpdatedAt { get; set; }
    public string? Status { get; set; }
    public int LoyaltyPoints { get; set; }
    public decimal StoreCreditBalance { get; set; }
    public string? CustomerTier { get; set; }
    public decimal AverageOrderValue { get; set; }
    public bool AcceptsMarketing { get; set; }
    public string? Source { get; set; }
}

public class CustomerAddressModel
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Company { get; set; }
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public required string City { get; set; }
    public string? StateProvince { get; set; }
    public required string PostalCode { get; set; }
    public required string Country { get; set; }
    public string? Phone { get; set; }
}

public class CustomerStatisticsModel
{
    public int TotalCustomers { get; set; }
    public decimal TopSpender { get; set; }
}

public class CustomerOrderListResponse
{
    public List<CustomerOrderItemModel> Items { get; set; } = [];
    public int Total { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

public class CustomerOrderItemModel
{
    public Guid Id { get; set; }
    public required string OrderNumber { get; set; }
    public required string Status { get; set; }
    public decimal GrandTotal { get; set; }
    public required string CurrencyCode { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion

#region Request Models

public class CreateCustomerApiRequest
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Notes { get; set; }
    public List<string>? Tags { get; set; }
    public string? Status { get; set; }
    public bool AcceptsMarketing { get; set; }
    public List<CustomerAddressRequest>? Addresses { get; set; }
}

public class UpdateCustomerRequest
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Notes { get; set; }
    public List<string>? Tags { get; set; }
    public string? Status { get; set; }
    public bool AcceptsMarketing { get; set; }
}

public class CustomerAddressRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Company { get; set; }
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public required string City { get; set; }
    public string? StateProvince { get; set; }
    public required string PostalCode { get; set; }
    public required string Country { get; set; }
    public string? Phone { get; set; }
    public bool IsDefaultShipping { get; set; }
    public bool IsDefaultBilling { get; set; }
}

public class AdjustLoyaltyPointsRequest
{
    public int Points { get; set; }
    public string? Reason { get; set; }
}

public class AdjustStoreCreditRequest
{
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}

#endregion
