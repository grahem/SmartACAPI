using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartACDeviceAPI.Options;
using SmartACDeviceAPI.Exceptions;
using SmartACDeviceAPI.Models;
using SmartACDeviceAPI.Security;

namespace SmartACDeviceAPI.Services
{
    public interface IUserAuthZService
    {
        Task<string> Authorize(string username, string password);
    }

    public class UserAuthZService : IUserAuthZService
    {
        private readonly IDynamoDBContext _dbContext;

        private readonly IOptionsMonitor<AuthZOptions> _options;
        private readonly ILogger<UserAuthZService> _logger;

        public UserAuthZService(IDynamoDBContext dBContext, ILogger<UserAuthZService> logger, IOptionsMonitor<AuthZOptions> options)
        {
            _dbContext = dBContext;
            _logger = logger;
            _options = options;
        }

        public async Task<string> Authorize(string username, string password)
        {
            if (String.IsNullOrEmpty(username)
                || String.IsNullOrEmpty(password))
            {
                return null;
            }

            var user = await _dbContext.LoadAsync<User>(username);
            if (user is null)
            {
                return null;
            }
            else
            {
                if (String.Equals(HashPassword(password), user.Password))
                {
                    var tokenString = JWTHelper.GenerateJWTToken(_options);

                    return tokenString;
                }
                else
                {
                    _logger.LogInformation(string.Format("Authorization Failed. Bad Password for {0}", username));
                    throw new AuthZException();
                }
            }
        }

        private string HashPassword(string clearTextPassword)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: clearTextPassword,
            salt: new byte[128 / 8],
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

            return hashed;
        }
    }

}