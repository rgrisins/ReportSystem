using StackExchange.Redis;

namespace ReportSystem.Services
{
    public class SessionService
    {
        private readonly IDatabase _db;
        public SessionService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public void SaveSession(string sessionId, string token, TimeSpan expiry)
        {
            _db.StringSet(sessionId, token, expiry);
        }

        public string? GetToken(string sessionId)
        {
            return _db.StringGet(sessionId);
        }

        public void DeleteSession(string sessionId)
        {
            _db.KeyDelete(sessionId);
        }
    }
}
