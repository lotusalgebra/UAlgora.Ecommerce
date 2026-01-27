using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for country operations.
/// </summary>
public interface ICountryRepository : IRepository<Country>
{
    /// <summary>
    /// Gets a country by its ISO code.
    /// </summary>
    Task<Country?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets all active countries.
    /// </summary>
    Task<IReadOnlyList<Country>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets all countries with their states.
    /// </summary>
    Task<IReadOnlyList<Country>> GetAllWithStatesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets a country with its states by ID.
    /// </summary>
    Task<Country?> GetWithStatesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a country with its states by code.
    /// </summary>
    Task<Country?> GetWithStatesByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets countries that allow shipping.
    /// </summary>
    Task<IReadOnlyList<Country>> GetShippingCountriesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets countries that allow billing.
    /// </summary>
    Task<IReadOnlyList<Country>> GetBillingCountriesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets featured countries (shown at top of lists).
    /// </summary>
    Task<IReadOnlyList<Country>> GetFeaturedAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets states for a country.
    /// </summary>
    Task<IReadOnlyList<StateProvince>> GetStatesAsync(Guid countryId, CancellationToken ct = default);

    /// <summary>
    /// Gets states for a country by code.
    /// </summary>
    Task<IReadOnlyList<StateProvince>> GetStatesByCountryCodeAsync(string countryCode, CancellationToken ct = default);

    /// <summary>
    /// Gets a state by ID.
    /// </summary>
    Task<StateProvince?> GetStateByIdAsync(Guid stateId, CancellationToken ct = default);

    /// <summary>
    /// Gets a state by country and state code.
    /// </summary>
    Task<StateProvince?> GetStateByCodeAsync(string countryCode, string stateCode, CancellationToken ct = default);

    /// <summary>
    /// Adds a state to a country.
    /// </summary>
    Task<StateProvince> AddStateAsync(StateProvince state, CancellationToken ct = default);

    /// <summary>
    /// Updates a state.
    /// </summary>
    Task<StateProvince> UpdateStateAsync(StateProvince state, CancellationToken ct = default);

    /// <summary>
    /// Deletes a state.
    /// </summary>
    Task DeleteStateAsync(Guid stateId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a country code exists.
    /// </summary>
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// Checks if a state code exists in a country.
    /// </summary>
    Task<bool> StateCodeExistsAsync(Guid countryId, string code, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// Counts the number of countries.
    /// </summary>
    Task<int> CountAsync(CancellationToken ct = default);
}
