using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using SmartACDeviceAPI.Models;

namespace SmartACDeviceAPI {

 public class MeasurementService {
     private readonly IDynamoDBContext _dbContext;
        private readonly IAmazonDynamoDB _dbClient;
        private readonly ILogger<MeasurementService> _logger;

        public MeasurementService(DynamoDBContext dBContext, IAmazonDynamoDB dbClient, ILogger<MeasurementService> logger) {
            _dbContext = dBContext;
            _dbClient = dbClient;
            _logger = logger;
        }

        public async Task<List<Measurement>> GetMeasurements(string deviceId, string fromDate, string toDate) {
            return null;

            
        }
 }   
}