using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using SmartACDeviceAPI.Models;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartACDeviceAPI.Services
{

    ///Implements the various lookup and insert methods for a device
    public class DeviceService
    {
        private readonly IDynamoDBContext _context;
        private readonly ILogger<DeviceService> _logger;

        public DeviceService(IDynamoDBContext context, ILogger<DeviceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DeviceServiceResponse> GetDeviceBySerialNumber(string serialNumber)
        {
            try
            {
                var resultList = await _context.QueryAsync<Device>(serialNumber).GetNextSetAsync();
                if (resultList.Count > 0)
                {
                    var device = resultList.First();

                    //create a service object to hide the device's secret
                    var serviceResponse = new DeviceServiceResponse();
                    serviceResponse.SerialNumber = device.SerialNumber;
                    serviceResponse.FirmwareVersion = device.FirmwareVersion;
                    serviceResponse.InAlarm = device.InAlarm;
                    serviceResponse.Status = device.Status;

                    var serializedServiceResponse = JsonSerializer.Serialize(serviceResponse);
                    _logger.LogDebug(String.Format("Get Device Service Response: {0}", serializedServiceResponse));
                    return serviceResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("An error occured fetching device for serial number: {0}", serialNumber));
                throw ex;
            }

            return null;
        }
    }
}