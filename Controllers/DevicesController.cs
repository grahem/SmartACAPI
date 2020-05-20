using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartACDeviceAPI.Models;
using SmartACDeviceAPI.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SmartACDeviceAPI.Controllers
{
    
    //This class handles getting a list of devices as well as registering a new device.
    [ApiController]
    [Route("devices")]
    public class DevicesController : ControllerBase
    {
        private readonly DeviceService _deviceService;
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(DeviceService deviceService, ILogger<DevicesController> logger)
        {
            _deviceService = deviceService;
            _logger = logger;
        }

        [Authorize()]
        [HttpGet]
        ///Gets a list of devices.
        public async Task<IActionResult> Get([FromQuery] short? count)
        {
            var stopWatch = Stopwatch.StartNew();
            var limit = count ?? 200;

            _logger.LogDebug(String.Format("Getting a list of {0} devices", limit));
            
            try {
            var devices = await _deviceService.GetDevices(limit);
            stopWatch.Stop();
            if (devices != null)
                _logger.LogInformation(String.Format("Got {0} devices in {1}ms", devices.Count, stopWatch.ElapsedMilliseconds));
            
            return Ok(devices);
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting devices");
                return new StatusCodeResult(503);
            }
            
        }

        [AllowAnonymous]
        [HttpPost]
        ///Takes a <cref="SmartACDeviceAPI.Device">device</cref> as an Http POST, and saves to DynamoDB.
        ///If missing properties are detected, a 400 Bad Request is returned. Otherwise, a 200 OK is returned with the saved device.
        public async Task<IActionResult> Post(Device device)
        {
            
            var stopWatch = Stopwatch.StartNew();
            
            _logger.LogDebug(String.Format("Registering device serial number {0}", device.SerialNumber));
            
            try {
            var deviceResponse = await _deviceService.RegisterDevice(device);
            stopWatch.Stop();
            if (deviceResponse != null)
                _logger.LogInformation(String.Format("Registered device in {0}ms", stopWatch.ElapsedMilliseconds));
            
            return Ok(deviceResponse);
            } catch (Exception ex) {
                _logger.LogError(ex, "Error registering devices");
                return new StatusCodeResult(503);
            }
        }
        
    }
}
