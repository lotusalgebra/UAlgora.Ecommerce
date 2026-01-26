using UAlgora.Ecommerce.Core.Interfaces.Services;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// BCrypt implementation of password hashing.
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    // Work factor determines the computational cost. 12 is a good balance between security and performance.
    private const int WorkFactor = 12;

    /// <inheritdoc />
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <inheritdoc />
    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            // If verification fails due to invalid hash format, return false
            return false;
        }
    }
}
