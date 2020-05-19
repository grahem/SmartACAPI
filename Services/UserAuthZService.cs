using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartACAPI.Options;
using SmartACDeviceAPI.Exceptions;
using SmartACDeviceAPI.Models;
using SmartACDeviceAPI.Security;

namespace SmartACDeviceAPI.Services
{
    public class UserAuthZService
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
                //TODO: encrypt password at rest
                if (String.Equals(password, user.Password))
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
    }

}