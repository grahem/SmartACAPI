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
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace SmartACDeviceAPI.Controllers
{

    [ApiController]
    [Route("devices/{deviceId}/measurements/{from?}/{to?}")]
    public class MeasurementController : ControllerBase
    {
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
        ///Gets last 500 measurements.
        public IActionResult Get(
            [FromRoute(Name = "deviceId")] string deviceId,
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "to")] string to)
        {

            //TODO: allow for open ended searched (from null or to null)
            bool includeTimeRange = false;
            if (!String.IsNullOrEmpty(from) && String.IsNullOrEmpty(to))
            {
                return BadRequest("from parameter supplied. Must includefrom");
            } else if (!String.IsNullOrEmpty(to) && String.IsNullOrEmpty(from))
            {
                return BadRequest("to parameter supplied. Must include from");
            } else if (!String.IsNullOrEmpty(from))
            {
                includeTimeRange = true;
            }

            string keyConditionExpression = "DeviceSerialNumber = :v_DeviceSerialNumber";
            var expressionAttributeValues = new Dictionary<string, AttributeValue> {
                    { ":v_DeviceSerialNumber", new AttributeValue { S = deviceId }  } 
                };
            if (includeTimeRange)
            {
                keyConditionExpression += " and RecordedTime between :v_start and :v_end";
                expressionAttributeValues.Add(":v_start", new AttributeValue { S = from });
                expressionAttributeValues.Add(":v_end", new AttributeValue { S = to });
            }


            var request = new QueryRequest
            {
                TableName = "Measurements",
                IndexName = "DeviceSerialNumber-RecordedTime-index",
                ReturnConsumedCapacity = "TOTAL",
                KeyConditionExpression = keyConditionExpression, 
                ExpressionAttributeValues = expressionAttributeValues
            };

            var task = _dbClient.QueryAsync(request);
            task.Wait();

            List<Measurement> measurements = new List<Measurement>();
            foreach (Dictionary<string, AttributeValue> item in task.Result.Items)
            {
                Console.WriteLine("Item:");
                Measurement measurement = new Measurement();
                foreach (var keyValuePair in item)
                {
                    switch (keyValuePair.Key)
                    {
                        case "Id":
                            measurement.Id = keyValuePair.Value.S;
                            break;
                        case "DeviceSerialNumber":
                            measurement.DeviceSerialNumber = keyValuePair.Value.S;
                            break;
                        case "AirHumidity":
                            double air = 0;
                            if (!String.IsNullOrEmpty(keyValuePair.Value.N))
                            {
                                double.TryParse(keyValuePair.Value.N, out air);
                            }
                            measurement.AirHumidity = air;
                            break;
                        case "CarbonMonoxide":
                            double carbon = 0;
                            if (!String.IsNullOrEmpty(keyValuePair.Value.N))
                            {
                                double.TryParse(keyValuePair.Value.N, out carbon);
                            }
                            measurement.CarbonMonoxide = carbon;
                            break;
                        case "Temperature":
                            double temp = 0;
                            if (!String.IsNullOrEmpty(keyValuePair.Value.N))
                            {
                                double.TryParse(keyValuePair.Value.N, out temp);
                            }
                            measurement.Temperature = temp;
                            break;
                        case "RecordedTime":
                            measurement.RecordedTime = keyValuePair.Value.S;
                            break;
                    }
                }
                measurements.Add(measurement);
            }

            return Ok(measurements);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromRoute(Name = "deviceId")] string deviceId, List<Measurement> measurements)
        {

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

                //force the serial number
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
