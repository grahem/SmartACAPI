using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI.Common;
using SmartACDeviceAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartACDeviceAPI.Controllers
{

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
