using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartACDeviceAPI.Maps;
using SmartACDeviceAPI.Models;
using System;
using System.Collections.Generic;
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
        //This class requires both a IDynamoDBContext and a AmazonDynamoDBClient
        //since it requires working at a lower leve of the DynamoDB SDK when getting measurements
        private readonly IDynamoDBContext _db;
        private readonly AmazonDynamoDBClient _dbClient;
        private readonly ILogger<MeasurementController> logger;

        public MeasurementController(IDynamoDBContext db, ILogger<MeasurementController> logger)
        {
            this._db = db;
            this._dbClient =  new AmazonDynamoDBClient();
            this.logger = logger;   
        }

        [Authorize]
        [HttpGet]
        ///Gets at most last 200 measurements or measurements defined by queryParams from and to (ISO 8601)
        public IActionResult Get(
            [FromRoute(Name = "deviceId")] string deviceId,
            [FromQuery] MeasurementQueryModel queryModel)
        {

            logger.LogInformation(String.Format("Getting Measurements for SerialNumber: {0}", deviceId));

            bool includeTimeRange = false;
            if (!String.IsNullOrEmpty(queryModel.FromDate))
            {
                includeTimeRange = true;
                Console.WriteLine("Time range will be applied");
            }

            //Build DynamoDB qury expression. Logic for including search filter
            string keyConditionExpression = "DeviceSerialNumber = :v_DeviceSerialNumber";
            var expressionAttributeValues = new Dictionary<string, AttributeValue> {
                    { ":v_DeviceSerialNumber", new AttributeValue { S = deviceId }  } 
                };
            if (includeTimeRange)
            {
                keyConditionExpression += " and RecordedTime between :v_start and :v_end";
                expressionAttributeValues.Add(":v_start", new AttributeValue { S = queryModel.FromDate });
                expressionAttributeValues.Add(":v_end", new AttributeValue { S = queryModel.ToDate });
            }

            var request = new QueryRequest
            {
                TableName = "Measurements",
                IndexName = "DeviceSerialNumber-RecordedTime-index",
                ReturnConsumedCapacity = "TOTAL",
                KeyConditionExpression = keyConditionExpression, 
                ExpressionAttributeValues = expressionAttributeValues
            };
            if (!includeTimeRange) { request.Limit = 200;}

            var task = _dbClient.QueryAsync(request);
            task.Wait();

            //Populate a list of measurements from the DynamoDB query response
            List<Measurement> measurements = DynamoMeasurementMapper.Map(task.Result.Items);
            

            logger.LogInformation(String.Format("Found {0} Measurements for SerialNumber: {1}", measurements.Count, deviceId));

            return Ok(measurements);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromRoute(Name = "deviceId")] string deviceId, List<Measurement> measurements)
        {
            logger.LogInformation(String.Format("Recording Measurements for SerialNumber: {0}", deviceId));

            //check if maintanence mode
            var sys = _db.LoadAsync<SystemConfig>("InMaintenance").Result;
            if (sys.ConfigValue.Equals("true"))
            {
                return new StatusCodeResult(503);
            }

            if (measurements.Count == 0 || measurements.Count > 500)
            {
                return BadRequest();
            }

            //TODO: optomize for batch writes. latest DynamoDB sdk removed it?
            foreach (var measurment in measurements)
            {
                if (String.IsNullOrEmpty(measurment.Id) || String.IsNullOrEmpty(measurment.RecordedTime))
                {
                    return BadRequest();
                }
                //if carbom monoxide is above 9PPM, trigger an alarm
                if (measurment.CarbonMonoxide > 9)
                {
                    var query = _db.QueryAsync<Device>(deviceId);
                    var resultList = query.GetRemainingAsync().Result;
                    if (resultList.Count == 1)
                    {
                        Device device = resultList.First();
                        //update device
                        device.InAlarm = true;
                        try
                        {
                            await _db.SaveAsync<Device>(device, default(System.Threading.CancellationToken));
                        } catch (Exception ex)
                        {
                            //Don't allow a DDB failure stop from saving records
                            logger.LogError(ex, "Error updating device alarm state");
                        }
                    }
                }

                //apply the serial number from the device in the URI
                measurment.DeviceSerialNumber = deviceId;
                
                try
                {
                    await _db.SaveAsync<Measurement>(measurment, default(System.Threading.CancellationToken));
                } catch (Exception ex)
                {
                    //TODO: handle failure and fork 400 vs 503 response.
                    logger.LogError(ex, "Error occured saving measurements");
                    return new StatusCodeResult(503);
                }
            }
            
            return Ok();
        }

    }
}
