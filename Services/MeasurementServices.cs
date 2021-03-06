using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using SmartACDeviceAPI.Maps;
using SmartACDeviceAPI.Models;
using System.Linq;

namespace SmartACDeviceAPI.Services
{

    public class MeasurementService : MaintenanceService
    {
        private readonly IDynamoDBContext _dbContext;
        private readonly IAmazonDynamoDB _dbClient;

        private readonly DeviceAlarmRecorder _alarmRecorder;
        private readonly ILogger<MeasurementService> _logger;

        public MeasurementService(IDynamoDBContext dbContext, IAmazonDynamoDB dbClient, DeviceAlarmRecorder alarmRecorder, ILogger<MeasurementService> logger) : base(dbContext)
        {
            _dbContext = dbContext;
            _dbClient = dbClient; 
            _alarmRecorder = alarmRecorder;
            _logger = logger;
        }

        public async Task<List<Measurement>> GetMeasurements(string serialNumber, string fromDate = null, string toDate = null)
        {
            bool includeTimeRange = !string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate);

            //Build DynamoDB qury expression
            string keyConditionExpression = "DeviceSerialNumber = :v_DeviceSerialNumber";
            var expressionAttributeValues = new Dictionary<string, AttributeValue> {
                    { ":v_DeviceSerialNumber", new AttributeValue { S = serialNumber }  }
                };

            //Logic for including search filter
            if (includeTimeRange)
            {
                keyConditionExpression += " and RecordedTime between :v_start and :v_end";
                expressionAttributeValues.Add(":v_start", new AttributeValue { S = fromDate });
                expressionAttributeValues.Add(":v_end", new AttributeValue { S = toDate });
            }

            var request = new QueryRequest
            {
                TableName = "Measurements",
                IndexName = "DeviceSerialNumber-RecordedTime-index",
                ReturnConsumedCapacity = "TOTAL",
                KeyConditionExpression = keyConditionExpression,
                ExpressionAttributeValues = expressionAttributeValues
            };

            //limit the request if no date range is provided. Date range queries return at most 1MB of data.
            if (!includeTimeRange) { request.Limit = 200; }

            try
            {
                var queryResult = await _dbClient.QueryAsync(request);

                //Populate a list of measurements from the DynamoDB query response
                List<Measurement> measurements = DynamoMeasurementMapper.Map(queryResult.Items);
                return measurements;
            }
            catch (Exception ex)
            {
                string message = string.Format("An error occured querying dynamo with an expression of {0}", keyConditionExpression);
                message += string.Format("\r\n With values Seiral Number {0} FromDate {1} ToDate {2}", serialNumber, fromDate, toDate);
                _logger.LogError(ex, message);
                throw ex;
            }
        }

        public async Task<int> RecordMeasurements(string serialNumber, List<Measurement> measurements)
        {

            //if carbom monoxide is above 9PPM, trigger an alarm
            _alarmRecorder.RecordAlarms(serialNumber, measurements);

            //force apply the serial number from the device in the URI
            measurements.ForEach(m => m.DeviceSerialNumber = serialNumber);

            var batchWrite = _dbContext.CreateBatchWrite<Measurement>();
            int counter = 0;

            for (int i = 0; i < measurements.Count; i += 25)
            {

                int range = i + 25;
                range = range > measurements.Count ? measurements.Count - i : 25;
                batchWrite.AddPutItems(measurements.GetRange(i, range));

                try
                {
                    await batchWrite.ExecuteAsync();
                    counter += range;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occured saving measurements");
                    throw ex;
                }
            }

            return counter;
        }

        
    }
}