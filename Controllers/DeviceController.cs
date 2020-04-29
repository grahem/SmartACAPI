using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartACDeviceAPI.Models;
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
        private readonly IDynamoDBContext _context;
        private readonly ILogger<DeviceController> logger;

        public DeviceController(IDynamoDBContext context, ILogger<DeviceController> logger)
        {
            _context = context;
            this.logger = logger;
        }

        [Authorize]
        [HttpGet]
        ///Gets a Device with the given Id.
        public IActionResult Get([FromRoute]string serialNumber)
        {

            if (string.IsNullOrEmpty(serialNumber))
            {
                return BadRequest();
            }

            //Get the device from DynamoDB
            var query = _context.QueryAsync<Device>(serialNumber);
            var resultList = query.GetRemainingAsync().Result;
            if (resultList.Count == 0)
            {
                return NotFound();
            }
            else
            {
                var device = resultList.First();

                //create a service object to hide the secret
                var serviceResponse = new DeviceServiceResponse();
                serviceResponse.SerialNumber = device.SerialNumber;
                serviceResponse.FirmwareVersion = device.FirmwareVersion;
                serviceResponse.InAlarm = device.InAlarm;
                serviceResponse.Status = device.Status;
                return Ok(serviceResponse);
            }
        }
    }
}
