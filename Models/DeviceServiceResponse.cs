
namespace SmartACDeviceAPI.Models
{
    //This class is used to map a Device DTO to a Service Response. 
    //It's main purpose is to strip out a device's secret token.
    public class DeviceServiceResponse
    {
        public string SerialNumber { get; set; }

        public string Status { get; set; }

        public string RegistrationDate { get; set; }

        public string FirmwareVersion { get; set; }


        public bool InAlarm { get; set; }

    }

}
