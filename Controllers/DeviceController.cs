using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartACDeviceAPI.Models;
using SmartACDeviceAPI.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly ILogger<DeviceController> _logger;

        public DeviceController(DeviceService deviceService, ILogger<DeviceController> logger)
        {
            _deviceSerive = deviceService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        ///Gets a Device with the given Id.
        public async Task<IActionResult> Get([FromRoute] string serialNumber)
        {

            if (string.IsNullOrEmpty(serialNumber))
                return BadRequest();

            var watch = Stopwatch.StartNew();

            try
            {
                var serviceResponse = await _deviceSerive.GetDeviceBySerialNumber(serialNumber);
                watch.Stop();
                _logger.LogInformation(string.Format("Got Device for {0} in {1} ms", serialNumber, watch.ElapsedMilliseconds));

                if (serviceResponse == null)
                    return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Device lookup failed for serial number: {0}", serialNumber));
                return new StatusCodeResult(503);
            }

            return Ok();
        }
    }
}
