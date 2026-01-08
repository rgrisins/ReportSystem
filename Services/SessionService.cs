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

        public void SaveRefreshToken(string refreshToken, string userId, TimeSpan expiry)
        {
            _db.StringSet(refreshToken, userId, expiry);
        }

        public string? GetUserIdByRefreshToken(string refreshToken)
        {
            return _db.StringGet(refreshToken);
        }

        public void DeleteRefreshToken(string refreshToken)
        {
            _db.KeyDelete(refreshToken);
        }
    }
}
