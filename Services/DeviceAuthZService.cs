using SmartACDeviceAPI.Security;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using SmartACDeviceAPI.Options;
using SmartACDeviceAPI.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using SmartACDeviceAPI.Exceptions;

namespace SmartACDeviceAPI.Services
{
    public class DeviceAuthZService
    {
        private readonly IDynamoDBContext _dBContext;
        private readonly IOptionsMonitor<AuthZOptions> _options;
        private readonly ILogger<DeviceAuthZService> _logger;

        public DeviceAuthZService(IDynamoDBContext dBContext, IOptionsMonitor<AuthZOptions> options, ILogger<DeviceAuthZService> logger)
        {
            _dBContext = dBContext;
            _options = options;
            _logger = logger;
        }

        public async Task<string> Authorize(string serialNumber, string secret)
        {
            //Ensure we have the supplied device in DynamoDB
            var device = await _dBContext.LoadAsync<Device>(serialNumber);
            if (device == null)
            {
                return null;
            }
            else
            {
                //Check that the supplied secret matches the database.
                if (String.Equals(device.Secret, secret))
                {
                    var tokenString = JWTHelper.GenerateJWTToken(_options);
                    return tokenString;
                }
                else
                {
                    _logger.LogInformation(string.Format("Device Authorization Failed: {0}", serialNumber));
                    throw new AuthZException();
                }
            }
        }
    }
}