using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using SmartACDeviceAPI.Models;
using System.Linq;

namespace SmartACDeviceAPI.Services {
    public class DeviceAlarmRecorder {

        public const int CARBON_MONOXIDE_ALARM_LEVEL = 9;

        private readonly IDynamoDBContext _dbContext;

        private readonly ILogger<DeviceAlarmRecorder> _logger;

        public DeviceAlarmRecorder() {}

        public DeviceAlarmRecorder(IDynamoDBContext dbContext, ILogger<DeviceAlarmRecorder> logger) {
            _dbContext =dbContext;
            _logger = logger;
        }

        public virtual void RecordAlarms(string serialNumber, List<Measurement> measurements)
        {
            if (string.IsNullOrEmpty(serialNumber))
                throw new ArgumentNullException(nameof(serialNumber));
            if (measurements is null)
                throw new ArgumentNullException(nameof(measurements));


            var devices = new List<Device>();

            measurements.ForEach(async measurement =>
                {
                    if (measurement.CarbonMonoxide > CARBON_MONOXIDE_ALARM_LEVEL
                    && devices.FirstOrDefault(devices => devices.SerialNumber == measurement.DeviceSerialNumber) != null)
                    {
                        try
                        {
                            var device = await _dbContext.LoadAsync<Device>(measurement.DeviceSerialNumber);
                            if (device != null)
                            {
                                device.InAlarm = true;
                                try
                                {
                                    await _dbContext.SaveAsync<Device>(device);
                                }
                                catch (Exception ex)
                                {
                                    //Don't allow a DDB failure stop from saving records
                                    _logger.LogError(ex, "Error updating device alarm state");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error loading device");
                        }

                        return;
                    }
                });
        }

    }
}