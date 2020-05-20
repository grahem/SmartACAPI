using System;
using System.Collections.Generic;
using System.Reflection;
using Amazon.DynamoDBv2.Model;
using SmartACDeviceAPI.Models;

namespace SmartACDeviceAPI.Maps {
    public class DynamoDeviceMapper
    {

        public static List<Device> Map(List<Dictionary<string,AttributeValue>> devices) {
            var deviceRespone = new List<Device>();
            foreach (var item in devices) {
                Device device = new Device();
                foreach (var kvp in item) {
                    typeof(Device).InvokeMember(kvp.Key, BindingFlags.SetProperty, Type.DefaultBinder, device, new object[] {kvp.Value.S});
                }
                deviceRespone.Add(device);
                
            }
            return deviceRespone;
        }
    }

}