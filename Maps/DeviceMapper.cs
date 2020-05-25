using System.Collections.Generic;
using SmartACDeviceAPI.Models;
using System.Reflection;
using System;
using System.Linq;
using System.Diagnostics;

namespace SmartACDeviceAPI.Maps
{

    public class DeviceMapper
    {

        public static DeviceServiceResponse MapDevice(Device device)
        {
            if (device == null) 
                return null;
            return MapDevices(new List<Device> { device }).FirstOrDefault();
        }

        public static List<DeviceServiceResponse> MapDevices(List<Device> devices)
        {
            List<DeviceServiceResponse> response = new List<DeviceServiceResponse>();
            if (devices == null || devices.Count == 0)
                return response;

            var deviceTypeProps = typeof(Device).GetProperties()
            .Where(prop => prop.Name != nameof(Device.Secret))
            .ToList();

            devices.ForEach(device =>
            {
                //use reflection to map all properties except for the secret
                //this future proofs in case we add more properites in the future
                //it's mostly coding just for fun
                var dsr = new DeviceServiceResponse();
                response.Add(dsr);
                var dsrType = typeof(DeviceServiceResponse);

                deviceTypeProps.ForEach(prop =>
                {
                    if (dsrType.GetProperty(prop.Name) != null)
                        dsrType.InvokeMember(prop.Name,
                        BindingFlags.SetProperty,
                        Type.DefaultBinder,
                        dsr,
                        new object[] { prop.GetValue(device) });
                });

            });
            
            return response;
        }
    }
}