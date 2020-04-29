
namespace SmartACDeviceAPI.Models
{
    public class DeviceServiceResponse
    {
        public string SerialNumber { get; set; }

        public string Status { get; set; }

        public string RegistrationDate { get; set; }

        public string FirmwareVersion { get; set; }


        public bool InAlarm { get; set; }

    }

}
