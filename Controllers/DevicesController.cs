using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartACDeviceAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartACDeviceAPI.Controllers
{
    
    //This class handles getting a list of devices as well as registering a new device.
    [ApiController]
    [Route("devices")]
    public class DevicesController : ControllerBase
    {
        private readonly IDynamoDBContext _context;
        private readonly ILogger<DevicesController> logger;

        public DevicesController(IDynamoDBContext context, ILogger<DevicesController> logger)
        {
            _context = context;
            this.logger = logger;
        }

        [Authorize]
        [HttpGet]
        ///Gets a Device with the given Id.
        public IActionResult Get()
        {
            logger.LogInformation(String.Format("Getting a list of devices"));
            var query = _context.ScanAsync<Device>(null);
            var devices = query.GetRemainingAsync().Result;
            List<Device> devicesResult = new List<Device>();

            //First return devices that are not healthy by registration date. 
            //This sorts unhealthy devices at the top of the list
            var healthyDevices = devices
                .Where(device => device.Status != "healthy")
                .OrderBy(device => device.RegistrationDate)
                .ToList();
            devicesResult.AddRange(healthyDevices);

            logger.LogInformation(String.Format("Found {0} Unhealthy devices", devicesResult.Count));

            //Only return at most 50 records.
            //If there are less than 50 unhealthy devices, 
            //then pull the remainder of healthy ordered devices by registration date.
            if (healthyDevices.Count < 50)
            {
                var extraDevices = devices
                    .Where(device => device.Status == "healthy")
                    .OrderBy(device => device.RegistrationDate)
                    .Take(50 - devices.Count)
                    .ToList();

                devicesResult.AddRange(extraDevices);
            }

            //If we have no devices, return not found
            //TODO: consider returning an OK here. It might be ok that we don't have any devices registered.
            if (devicesResult.Count == 0) {
                return NotFound();
            }
            else {
                return Ok(devicesResult);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        ///Takes a <cref="SmartACDeviceAPI.Device">device</cref> as an Http POST, and saves to DynamoDB.
        ///If missing properties are detected, a 400 Bad Request is returned. Otherwise, a 200 OK is returned with the saved device.
        ///
        public async Task<IActionResult> Post(Device device)
        {
            
            //Validate device properties
            if (String.IsNullOrEmpty(device.SerialNumber) 
                || String.IsNullOrEmpty(device.Status)
                || String.IsNullOrEmpty(device.FirmwareVersion)
                || String.IsNullOrEmpty(device.Secret))
            {
                return BadRequest("One or more properties is missing");
            }

            logger.LogInformation(String.Format("Registering a device: SerialNumber {0}", device.SerialNumber));

            await _context.SaveAsync<Device>(device, default(System.Threading.CancellationToken));
            Device result = await _context.LoadAsync<Device>(device.SerialNumber, device.Status, default(System.Threading.CancellationToken));

            //create a service object to hide the secret
            var serviceResponse = new DeviceServiceResponse();
            serviceResponse.SerialNumber = result.SerialNumber;
            serviceResponse.FirmwareVersion = result.FirmwareVersion;
            serviceResponse.InAlarm = result.InAlarm;
            serviceResponse.Status = result.Status;
            return Ok(serviceResponse);
        }
        
    }
}
