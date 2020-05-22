using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartACDeviceAPI.Maps;
using SmartACDeviceAPI.Models;
using SmartACDeviceAPI.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SmartACDeviceAPI.Controllers
{

    //This class handles searching and recording device measurements.
    [ApiController]
    [Route("devices/{deviceId}/measurements/{from?}/{to?}")]
    public class MeasurementController : ControllerBase
    {

        private readonly MeasurementService _measurementService;
        private readonly ILogger<MeasurementController> _logger;

        public MeasurementController(MeasurementService measurementService, ILogger<MeasurementController> logger)
        {
            _measurementService = measurementService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        ///Gets at most last 200 measurements or measurements defined by queryParams from and to (ISO 8601)
        public async Task<IActionResult> Get(
            [FromRoute(Name = "deviceId")] string deviceId,
            [FromQuery] MeasurementQueryModel queryModel)
        {
            var watch = Stopwatch.StartNew();
            _logger.LogDebug(String.Format("Getting Measurements for serial number: {0}", deviceId));

            var measurements = await _measurementService.GetMeasurements(deviceId, queryModel.FromDate, queryModel.ToDate);

            watch.Stop();
            _logger.LogInformation(string.Format("Found {0} Measurements for serial number {1} in {2}ms", measurements.Count, deviceId, watch.ElapsedMilliseconds));

            return Ok(measurements);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromRoute(Name = "deviceId")] string deviceId, [FromBody] PostMeasurementsModel measurements)
        {
            _logger.LogInformation(String.Format("Recording Measurements for SerialNumber: {0}", deviceId));

            var watch = Stopwatch.StartNew();

            //check if maintanence mode
            if (await _measurementService.IsInMaintananceMode()) {
                return new StatusCodeResult(503);
            }

            var count = await _measurementService.RecordMeasurements(deviceId, measurements.Measurements);

            if (count != measurements.Measurements.Count)
            {
                return BadRequest(string.Format("Not all measurements recorded. Recorded {0} of (1)", count, measurements.Measurements.Count));
            }

            return Ok(count);
        }

    }
}
