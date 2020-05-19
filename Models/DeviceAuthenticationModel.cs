using System.ComponentModel.DataAnnotations;

namespace SmartACDeviceAPI.Models
{
    public class DeviceAuthenticationModel
    { 
        [Required]
        public string SerialNumber { get; set; }

        [Required]
        public string Secret { get; set;  }
    }
}
