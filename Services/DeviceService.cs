using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using SmartACDeviceAPI.Maps;
using SmartACDeviceAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
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
            //limit the range of results to 1-200
            var limit = count < 1 ? 1 : count > 200 ? 200 : count;
  
            var query = await _amazonDynamoDB.QueryAsync(BuildQueryRequest("unhealthy"));
            var devices = DynamoDeviceMapper.Map(query.Items.Take(limit - query.Items.Count).ToList());

            _logger.LogInformation(String.Format("Found {0} Unhealthy devices", devices.Count));

            //If there are less than {count} unhealthy devices, 
            //then pull the remainder of healthy ordered devices by registration date.
            if (devices.Count < limit)
            {
                //re-write queryRequest to only include healthy devices
                query = await _amazonDynamoDB.QueryAsync(BuildQueryRequest("healthy"));
                
                var healthyDevices = DynamoDeviceMapper.Map(query.Items.Take(limit - query.Items.Count).ToList());
                devices.AddRange(healthyDevices);
            }

            return DeviceMapper.MapDevices(devices);
        }

        private QueryRequest BuildQueryRequest(string status) {
            var queryRequest = new QueryRequest();
            queryRequest.TableName = "Devices";
            queryRequest.IndexName = "Status-RegistrationDate-index";
            queryRequest.ExpressionAttributeNames.Add("#Status","Status");
            queryRequest.KeyConditionExpression = "#Status = :v_Status";
            queryRequest.ExpressionAttributeValues.Add(":v_Status", new AttributeValue { S = status});
            return queryRequest;
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