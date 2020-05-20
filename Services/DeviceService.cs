using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Logging;
using SmartACDeviceAPI.Maps;
using SmartACDeviceAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartACDeviceAPI.Services
{

    ///Implements the various lookup and insert methods for a device
    public class DeviceService
    {
        private readonly IDynamoDBContext _dbContext;

        private readonly IAmazonDynamoDB _amazonDynamoDB;
        private readonly ILogger<DeviceService> _logger;

        public DeviceService(IDynamoDBContext dbContext, IAmazonDynamoDB amazonDynamoDB, ILogger<DeviceService> logger)
        {
            _dbContext = dbContext;
            _amazonDynamoDB = amazonDynamoDB;
            _logger = logger;
        }

        public async Task<DeviceServiceResponse> GetDeviceBySerialNumber(string serialNumber)
        {
            try
            {
                var device = await _dbContext.LoadAsync<Device>(serialNumber);
                if (device != null)
                {
                    //create and map a service object to hide the device's secret
                    return DeviceMapper.MapDevice(device);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("An error occured fetching device for serial number: {0}", serialNumber));
                throw ex;
            }

            return null;
        }

        public async Task<List<DeviceServiceResponse>> GetDevices(short count)
        {
            //response
            List<Device> devicesResult = new List<Device>();

            //limit the range of results to 1-200
            var limit = count < 1 ? 1 : count > 200 ? 200 : count;
  
            var healthyScanCondition = new ScanCondition("status", ScanOperator.NotEqual, new object[]{"healthy"});
            var devices = await _dbContext.ScanAsync<Device>(new List<ScanCondition>(){healthyScanCondition}).GetNextSetAsync();
            
            _logger.LogInformation(String.Format("Found {0} Unhealthy devices", devicesResult.Count));

            //If there are less than {count} unhealthy devices, 
            //then pull the remainder of healthy ordered devices by registration date.
            if (devices.Count < limit)
            {
                var healthyDevices = await _dbContext.ScanAsync<Device>(new List<ScanCondition>(){healthyScanCondition}).GetNextSetAsync();
                var extraDevices = devices
                    .OrderBy(device => device.RegistrationDate)
                    .Take(limit - devices.Count)
                    .ToList();

                devicesResult.AddRange(extraDevices);
            }

            return DeviceMapper.MapDevices(devicesResult);
        }

        public async Task<DeviceServiceResponse> RegisterDevice(Device device) {

            if (String.IsNullOrEmpty(device.RegistrationDate))
                device.RegistrationDate = DateTime.UtcNow.ToString("s");
            
            await _dbContext.SaveAsync<Device>(device, default(System.Threading.CancellationToken));
            Device deviceResponse = await _dbContext.LoadAsync<Device>(device.SerialNumber);

            //create a service object to hide the secret
            return DeviceMapper.MapDevice(deviceResponse);
        }
    }
}