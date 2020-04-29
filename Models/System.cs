
using Amazon.DynamoDBv2.DataModel;

namespace SmartACDeviceAPI.Models
{
    [DynamoDBTable("SystemConfig")]
    public class SystemConfig
    {

        [DynamoDBHashKey]
        public string ConfigKey { get; set; }

        [DynamoDBProperty]
        public string ConfigValue { get; set; }

    }
}
