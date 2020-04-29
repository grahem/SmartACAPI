using Amazon.DynamoDBv2.DataModel;

namespace SmartACDeviceAPI.Models
{
    [DynamoDBTable("User")]
    public class User
    {
        [DynamoDBHashKey]
        public string UserName { get; set;  }

        [DynamoDBProperty]
        public string Password { get; set; }
    }
}
