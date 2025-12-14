using knkwebapi_v2.Models.ClientActivity;

namespace knkwebapi_v2.Services.Interfaces;

/// <summary>
/// Abstraction for client activity storage.
/// Tracks which clients are active, their request patterns, and performance metrics.
/// Designed to be extensible: in-memory now, Redis/external store later.
/// </summary>
public interface IClientActivityStore
{
    /// <summary>
    /// Records a request from a client.
    /// Thread-safe; can be called from multiple middleware instances.
    /// </summary>
    /// <param name="client">Client information (headers)</param>
    /// <param name="request">Request information (method, route, status, duration)</param>
    void RecordRequest(ClientInfo client, RequestInfo request);

    /// <summary>
    /// Retrieves all clients that have been active within the specified time span.
    /// </summary>
    /// <param name="activeWithin">Time span to consider "active"</param>
    /// <returns>List of client snapshots, ordered by most recently seen first</returns>
    IReadOnlyList<ClientSnapshot> GetClients(TimeSpan activeWithin);

    /// <summary>
    /// Retrieves detailed snapshot for a specific client.
    /// </summary>
    /// <param name="clientType">Client type from header</param>
    /// <param name="clientId">Client id from header</param>
    /// <returns>Snapshot if client exists and has been seen, null otherwise</returns>
    ClientSnapshot? GetClient(string clientType, string clientId);

    /// <summary>
    /// Retrieves all known clients regardless of recent activity.
    /// Useful for admin dashboards showing all registered clients.
    /// </summary>
    IReadOnlyList<ClientSnapshot> GetAllClients();

    /// <summary>
    /// Removes clients that haven't been seen for longer than the specified duration.
    /// Called periodically to prevent unbounded memory growth.
    /// </summary>
    /// <param name="inactiveOlderThan">Clients with no activity older than this are removed</param>
    /// <returns>Number of clients removed</returns>
    int Cleanup(TimeSpan inactiveOlderThan);
}
