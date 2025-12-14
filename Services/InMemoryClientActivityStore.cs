using System.Collections.Concurrent;
using knkwebapi_v2.Models.ClientActivity;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Services;

/// <summary>
/// Thread-safe in-memory implementation of client activity tracking.
/// Maintains a rolling 60-minute window of request metrics per client.
/// </summary>
public class InMemoryClientActivityStore : IClientActivityStore
{
    private class ClientActivityEntry
    {
        /// <summary>The client's metadata.</summary>
        public ClientInfo Client { get; set; } = new();

        /// <summary>Last request information.</summary>
        public RequestInfo? LastRequest { get; set; }

        /// <summary>When this client was last seen.</summary>
        public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Rolling 60-minute window buckets (one per minute).</summary>
        public RollingWindowBucket?[] Buckets { get; set; } = new RollingWindowBucket?[60];

        /// <summary>Lock for this entry (per-entry locking to avoid hotspot).</summary>
        public readonly object Lock = new();
    }

    /// <summary>Dictionary mapping clientKey -> ClientActivityEntry.</summary>
    private readonly ConcurrentDictionary<string, ClientActivityEntry> _clients = new();

    /// <summary>
    /// Maximum number of clients to track. Cleanup runs if exceeded.
    /// Can be set via constructor for testing/configuration.
    /// </summary>
    private readonly int _maxClients;

    /// <summary>
    /// Cleanup interval: how often to check and remove inactive clients.
    /// </summary>
    private readonly TimeSpan _cleanupInterval;

    /// <summary>Timestamp of last cleanup operation.</summary>
    private DateTime _lastCleanupUtc = DateTime.UtcNow;

    public InMemoryClientActivityStore(int maxClients = 1000, TimeSpan? cleanupInterval = null)
    {
        _maxClients = maxClients;
        _cleanupInterval = cleanupInterval ?? TimeSpan.FromMinutes(10);
    }

    public void RecordRequest(ClientInfo client, RequestInfo request)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        if (request == null) throw new ArgumentNullException(nameof(request));

        var key = client.GetKey();
        
        // Trigger cleanup if needed.
        TriggerCleanupIfNeeded();

        var entry = _clients.AddOrUpdate(key,
            _ => new ClientActivityEntry
            {
                Client = client,
                LastRequest = request,
                LastSeenUtc = DateTime.UtcNow,
            },
            (_, existingEntry) =>
            {
                lock (existingEntry.Lock)
                {
                    // Update last request and last seen.
                    existingEntry.LastRequest = request;
                    existingEntry.LastSeenUtc = DateTime.UtcNow;

                    // Update rolling window bucket.
                    var minuteIndex = DateTime.UtcNow.Minute;
                    var bucket = existingEntry.Buckets[minuteIndex];
                    
                    if (bucket == null || bucket.MinuteIndex != minuteIndex)
                    {
                        // Create new bucket for this minute.
                        bucket = new RollingWindowBucket
                        {
                            MinuteIndex = minuteIndex,
                            BucketStartUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 
                                DateTime.UtcNow.Day, DateTime.UtcNow.Hour, minuteIndex, 0, 0, DateTimeKind.Utc)
                        };
                        existingEntry.Buckets[minuteIndex] = bucket;
                    }

                    bucket.RecordRequest(request);
                }

                return existingEntry;
            });

        // Ensure first insertion also updates the bucket.
        if (!_clients.TryGetValue(key, out var existing) || existing == entry)
        {
            lock (entry.Lock)
            {
                var minuteIndex = DateTime.UtcNow.Minute;
                var bucket = entry.Buckets[minuteIndex];
                
                if (bucket == null || bucket.MinuteIndex != minuteIndex)
                {
                    bucket = new RollingWindowBucket
                    {
                        MinuteIndex = minuteIndex,
                        BucketStartUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month,
                            DateTime.UtcNow.Day, DateTime.UtcNow.Hour, minuteIndex, 0, 0, DateTimeKind.Utc)
                    };
                    entry.Buckets[minuteIndex] = bucket;
                }

                if (entry.LastRequest == request) // Only record if we just set it above.
                {
                    bucket.RecordRequest(request);
                }
            }
        }
    }

    public IReadOnlyList<ClientSnapshot> GetClients(TimeSpan activeWithin)
    {
        var threshold = DateTime.UtcNow.Subtract(activeWithin);
        var results = new List<ClientSnapshot>();

        foreach (var kvp in _clients)
        {
            var entry = kvp.Value;
            lock (entry.Lock)
            {
                if (entry.LastSeenUtc >= threshold)
                {
                    results.Add(SnapshotFromEntry(entry));
                }
            }
        }

        // Return sorted by most recently seen first.
        return results.OrderByDescending(s => s.LastSeenUtc).ToList();
    }

    public ClientSnapshot? GetClient(string clientType, string clientId)
    {
        var key = $"{clientType}:{clientId}";
        if (_clients.TryGetValue(key, out var entry))
        {
            lock (entry.Lock)
            {
                return SnapshotFromEntry(entry);
            }
        }
        return null;
    }

    public IReadOnlyList<ClientSnapshot> GetAllClients()
    {
        var results = new List<ClientSnapshot>();
        foreach (var kvp in _clients)
        {
            var entry = kvp.Value;
            lock (entry.Lock)
            {
                results.Add(SnapshotFromEntry(entry));
            }
        }
        return results.OrderByDescending(s => s.LastSeenUtc).ToList();
    }

    public int Cleanup(TimeSpan inactiveOlderThan)
    {
        var threshold = DateTime.UtcNow.Subtract(inactiveOlderThan);
        var keysToRemove = new List<string>();

        foreach (var kvp in _clients)
        {
            var entry = kvp.Value;
            lock (entry.Lock)
            {
                if (entry.LastSeenUtc < threshold)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
        }

        foreach (var key in keysToRemove)
        {
            _clients.TryRemove(key, out _);
        }

        return keysToRemove.Count;
    }

    /// <summary>
    /// Triggers cleanup if the interval has elapsed and we're over max clients.
    /// </summary>
    private void TriggerCleanupIfNeeded()
    {
        var now = DateTime.UtcNow;
        if (now - _lastCleanupUtc > _cleanupInterval && _clients.Count > _maxClients)
        {
            _lastCleanupUtc = now;
            // Remove clients inactive for more than 24 hours.
            Cleanup(TimeSpan.FromHours(24));
        }
    }

    /// <summary>
    /// Creates a snapshot from an entry. Assumes caller holds the lock.
    /// </summary>
    private static ClientSnapshot SnapshotFromEntry(ClientActivityEntry entry)
    {
        var snapshot = new ClientSnapshot
        {
            Client = entry.Client,
            LastSeenUtc = entry.LastSeenUtc,
            LastRequest = entry.LastRequest,
            BucketsLast60Minutes = new RollingWindowBucket?[60],
        };

        // Copy buckets array.
        Array.Copy(entry.Buckets, snapshot.BucketsLast60Minutes, 60);

        return snapshot;
    }
}
