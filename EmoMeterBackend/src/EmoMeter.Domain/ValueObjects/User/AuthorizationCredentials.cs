using Azure.Core;
using CSharpFunctionalExtensions;
using EmoMeter.Domain.Shared;
using EmoMeter.Domain.ValueObjects.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Domain.ValueObjects.User
{
    public class AuthorizationCredentials : ValueObject
    {
        //ef core
        private AuthorizationCredentials()
        {
        }

        private AuthorizationCredentials(
            string accessToken, 
            string refreshToken, 
            int tokenExpiresIn)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            TokenExpiresIn = DateTimeOffset.UtcNow.AddSeconds(tokenExpiresIn);
        }

        public string AccessToken { get; }

        public string RefreshToken { get; }

        public DateTimeOffset TokenExpiresIn { get; }

        public static Result<AuthorizationCredentials, Error> Create(
            string accessToken, 
            int tokenExpiringTimeInSeconds, 
            string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return Errors.Validation("AcccessToken");

            if (string.IsNullOrWhiteSpace(refreshToken))
                return Errors.Validation("RefreshToken");

            if (tokenExpiringTimeInSeconds < 0)
                return Errors.Validation("TokenExpiringTime");

            return new AuthorizationCredentials(accessToken, refreshToken, tokenExpiringTimeInSeconds);
        }

        public bool IsExpired() =>
               DateTimeOffset.UtcNow >= TokenExpiresIn;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return AccessToken;
            yield return RefreshToken;
            yield return TokenExpiresIn;
        }
    }
}
