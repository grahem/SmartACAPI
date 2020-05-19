using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartACDeviceAPI.Models;
using SmartACDeviceAPI.Services;
using System;
using System.Diagnostics;
using System.Linq;

namespace SmartACDeviceAPI.Controllers
{

    //This class is used exclusively to get Devices. Posting is done in DevicesManager.
    //TODO: Determine why .NET Core routing fails when combining the POST and GET in the same file.
    //For some reason, it does not like the variable in the URL to be listed on the method route for POST.
    [Authorize]
    [ApiController]
    [Route("devices/{serialNumber}")]
    public class DeviceController : ControllerBase
    {
        private readonly DeviceService _deviceSerive;
        private readonly Stopwatch _stopwatch;
        private readonly ILogger<DeviceController> _logger;

        public DeviceController(DeviceService deviceService,
                ILogger<DeviceController> logger,
                Stopwatch stopwatch)
        {
            _deviceSerive = deviceService;
            _stopwatch = stopwatch;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        ///Gets a Device with the given Id.
        public IActionResult Get([FromRoute] string serialNumber)
        {
            _stopwatch.Start();

            try
            {
                _logger.LogDebug(String.Format("Getting device for serial number {0}", serialNumber));
                var serviceResponse = _deviceSerive.GetDeviceBySerialNumber(serialNumber);

                _stopwatch.Stop();
                _logger.LogDebug(string.Format("Got Device for serial number {0} in {1} ms", serialNumber, _stopwatch.ElapsedMilliseconds));

                if (serviceResponse.Result != null)
                    return Ok(serviceResponse.Result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Device lookup failed for serial number: {0}", serialNumber));
                return new StatusCodeResult(503);
            }

            _logger.LogDebug(string.Format("Could not find device for serial number: {0}", serialNumber));
            return BadRequest();
        }
    }
}
