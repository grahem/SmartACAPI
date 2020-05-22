using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;

namespace SmartACDeviceAPI.Models
{
    [DynamoDBTable("Measurements")]
    public class Measurement
    {

        [DynamoDBHashKey]
        [Required]
        public string Id { get; set; }

        [DynamoDBRangeKey]
        public string DeviceSerialNumber { get; set; }

        [DynamoDBGlobalSecondaryIndexRangeKey]
        [Required]
        public string RecordedTime { get; set; }
        
        [DynamoDBProperty]
        public double AirHumidity { get; set; }

        [DynamoDBProperty]
        public double CarbonMonoxide { get; set; }

        [DynamoDBProperty]
        public double Temperature { get; set; }
    }
}
