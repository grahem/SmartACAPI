using Amazon.DynamoDBv2.DataModel;

namespace SmartACDeviceAPI.Models
{
    [DynamoDBTable("Devices")]
    public class Device
    {
        [DynamoDBHashKey]
        public string SerialNumber { get; set; }

        [DynamoDBRangeKey]
        public string Status { get; set; }

        [DynamoDBProperty]
        public string RegistrationDate { get; set; }

        [DynamoDBProperty]
        public string FirmwareVersion { get; set; }

        [DynamoDBProperty]
        public string Secret { get; set; }

        [DynamoDBProperty]
        public bool InAlarm { get; set; }

    }

}
