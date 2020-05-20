using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;

namespace SmartACDeviceAPI.Models
{
    [DynamoDBTable("Devices")]
    public class Device
    {
        [DynamoDBHashKey]
        [Required]
        public string SerialNumber { get; set; }

        [DynamoDBProperty]
        [Required]
        public string Status { get; set; }

        [DynamoDBProperty]
        public string RegistrationDate { get; set; }

        [DynamoDBProperty]
        [Required]
        public string FirmwareVersion { get; set; }

        [DynamoDBProperty]
        [Required]
        public string Secret { get; set; }

        [DynamoDBProperty]
        public bool InAlarm { get; set; }

    }

}
