using System;
using System.Collections.Generic;
using System.Reflection;
using Amazon.DynamoDBv2.Model;
using SmartACDeviceAPI.Models;

namespace SmartACDeviceAPI.Maps {
    public class DynamoMeasurementMapper {

        public static List<Measurement> Map(List<Dictionary<string, AttributeValue>> items) {
            var measurements= new List<Measurement>();
            foreach (Dictionary<string, AttributeValue> item in items)
            {
                Measurement measurement = new Measurement();
                foreach (var keyValuePair in item)
                {
                    switch (keyValuePair.Key)
                    {
                        case "AirHumidity":
                        case "CarbonMonoxide":
                        case "Temperature":
                            double val = 0;
                            if (!string.IsNullOrEmpty(keyValuePair.Value.N))
                                double.TryParse(keyValuePair.Value.N, out val);
                            typeof(Measurement).InvokeMember(keyValuePair.Key, BindingFlags.SetProperty, Type.DefaultBinder, measurement, new object[] {val});
                            break;
                        default:
                            typeof(Measurement).InvokeMember(keyValuePair.Key, BindingFlags.SetProperty, Type.DefaultBinder, measurement, new object[] {keyValuePair.Value.S});
                        break;
                    }
                }
                measurements.Add(measurement);
            }
            return measurements;
        }
    }   
}