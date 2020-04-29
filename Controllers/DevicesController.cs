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
            var query = _context.ScanAsync<Device>(null);
            var devices = query.GetRemainingAsync().Result;
            List<Device> devicesResult = new List<Device>();

            var healthyDevices = devices
                .Where(device => device.Status != "healthy")
                .OrderBy(device => device.RegistrationDate)
                .ToList();
            devicesResult.AddRange(healthyDevices);

            if (healthyDevices.Count < 50)
            {
                var extraDevices = devices
                    .Where(device => device.Status == "healthy")
                    .OrderBy(device => device.RegistrationDate)
                    .Take(50 - devices.Count)
                    .ToList();

                devicesResult.AddRange(extraDevices);
            }

            
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
            if (String.IsNullOrEmpty(device.SerialNumber) 
                || String.IsNullOrEmpty(device.Status)
                || String.IsNullOrEmpty(device.FirmwareVersion)
                || String.IsNullOrEmpty(device.Secret))
            {
                return BadRequest("One or more properties is missing");
            }

            await _context.SaveAsync<Device>(device, default(System.Threading.CancellationToken));
            Device result = await _context.LoadAsync<Device>(device.SerialNumber, device.Status, default(System.Threading.CancellationToken));

            return Ok(result);
        }
        
    }
}
