using StackExchange.Redis;

namespace ReportSystem.Services
{
    public class SessionService
    {
        private readonly IDatabase _db;

        // Constructor that sets the Redis connection dependency
        public SessionService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        // Method to save a session with a given session ID, token, and expiry time
        public void SaveSession(string sessionId, string token, TimeSpan expiry)
        {
            _db.StringSet(sessionId, token, expiry);
        }

        // Method to retrieve the token associated with a given session ID
        public string? GetToken(string sessionId)
        {
            return _db.StringGet(sessionId);
        }

        // Method to delete a session by its session ID
        public void DeleteSession(string sessionId)
        {
            _db.KeyDelete(sessionId);
        }
    }
}
