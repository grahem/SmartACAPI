using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;

namespace SmartACDeviceAPI.Models
{
    [DynamoDBTable("Users")]
    public class User
    {
        
        [Required]
        [DynamoDBHashKey("username")]
        public string UserName { get; set;  }

        [Required]
        [DynamoDBProperty("password")]
        public string Password { get; set; }
    }
}
