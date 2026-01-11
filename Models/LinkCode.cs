using System;

namespace knkwebapi_v2.Models;

/// <summary>
/// Represents a temporary link code used to connect web app and Minecraft server accounts.
/// 
/// Link codes enable several account flows:
/// 1. Web app first: Generate code during signup → user enters code in Minecraft
/// 2. Server first: Generate code via /account link command → user uses code in web app
/// 3. Account merging: Validate code before merging conflicting accounts
/// 
/// CRITICAL PROPERTIES:
/// - Code: 8 alphanumeric characters (e.g., ABC12XYZ)
/// - ExpiresAt: 20 minutes from creation
/// - Status: Active, Used, or Expired
/// - Uniqueness: Code is globally unique and must not be reused
/// </summary>
public class LinkCode
{
    /// <summary>
    /// Primary key - auto-generated.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to User. Nullable for web app-first flow.
    /// Set when:
    /// 1. Generated via /account link on Minecraft server
    /// 2. After user consumes code (consumed → user populated)
    /// Null initially for web app-first flow until consumption.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// The actual link code string.
    /// Format: 8 alphanumeric characters (e.g., ABC12XYZ)
    /// Display format (with hyphen): ABC-12XYZ
    /// 
    /// CRITICAL: Must be globally unique. No code can be reused.
    /// Entropy: ~218 trillion combinations (52^8).
    /// Generated using cryptographically secure random.
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Timestamp when the code was created.
    /// Immutable once set.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the code expires.
    /// Set to CreatedAt + 20 minutes.
    /// After this time, code cannot be used.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Current status of the link code.
    /// - Active: Code is valid and can be used
    /// - Used: Code has been consumed/linked
    /// - Expired: ExpiresAt has passed
    /// </summary>
    public LinkCodeStatus Status { get; set; } = LinkCodeStatus.Active;

    /// <summary>
    /// Timestamp when the code was consumed/used.
    /// Null if code has not been used.
    /// Set when code transitions to Used status.
    /// </summary>
    public DateTime? UsedAt { get; set; }

    // ===== RELATIONSHIPS =====

    /// <summary>
    /// Navigation property to the associated User.
    /// Null for web app-first flow until consumption.
    /// </summary>
    public User? User { get; set; }
}

/// <summary>
/// Enum representing the lifecycle status of a link code.
/// </summary>
public enum LinkCodeStatus
{
    /// <summary>
    /// Code is valid and ready to be used.
    /// Has not expired and has not been consumed yet.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Code has been successfully consumed/used.
    /// UsedAt timestamp is set.
    /// Cannot be reused.
    /// </summary>
    Used = 1,

    /// <summary>
    /// Code has expired (ExpiresAt < UtcNow).
    /// No longer valid for use.
    /// User must request a new code.
    /// </summary>
    Expired = 2
}
