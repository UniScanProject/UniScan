using System.Collections.Concurrent;

namespace UniScan.Server.Authentication.Session;

public class SessionManager
{
    private readonly ConcurrentDictionary<Guid, User> _activeSessions = new();

    public Guid CreateSession(User user)
    {
        Guid sessionId = Guid.NewGuid();
        _activeSessions.TryAdd(sessionId, user);
        
        return sessionId;
    }

    public User? GetSession(Guid sessionId) => _activeSessions.GetValueOrDefault(sessionId);
    
    public void RemoveSession(Guid sessionId) => _activeSessions.TryRemove(sessionId, out _);
    public void ClearSessions() => _activeSessions.Clear();
}