# Smart AC API

This repository contains the source code for the API portion of the Theorem.co Smart AC code challenge.

The API is developed in **.NET Core 3.1** and uses **DynamoDB** as its data store.

The API is responsible for the following functions:
1. Registering a device
2. Authenticate a device
3. Get a devices detilas
4. Allow a device to meter sensor data
5. Authentictae a web user

# Using the API

##  Devices
When a device comes online, it POSTs a request to **/devices**. To register a device, the device does not need to be authenticated.

Through authorization, the device can publish sensor measurements. To authorize a device using a **JWT Token**, the device makes a POST to **/authenticate**.

Once authorized, the device can publish measurements to **/device/{SerialNumber/measurements**.

## Users
Users interact with the API via a website. Users are currently seeded in DynamoDB and are authorized via **/authenticateuser**, which also uses **JWT Toekns**.

Users can get a list of devices via a GET to **/devices**.

User can get a single devices details via a GET to **devices/{SerialNumber}**

Users can get a devices measurements via a GET to **devices/{SerialNumber}/measurements**

Finally, users can filter sensor data by date ranges using the query parameters **from** and **to** in ISO 8601 format.

# API Documentation

The API is service via: http://smartacdeviceapi-dev.us-west-1.elasticbeanstalk.com

## POST /authenticate

Authenticates a device and returns a JWT token.

| Body
```
{
    "SerialNumber":"string",
    "Secret":"string"
}
```

| Example
```
curl --header "Content-Type: application/json" \
     --request POST \
     --data '{"SerialNumber":"123","Secret":"321"}' \
     http://localhost:3000/authenticate
```

| Response
```
A JWT token
```

## POST /authenticateuser

Authenticates a user a returns a JWT token.

| Body
```
{
    "UserName":"string",
    "Password":"string"
}
```

| Example
```
curl --header "Content-Type: application/json" \
     --request POST \
     --data '{"UserName":"user","Password":"password"}' \
     http://localhost:3000/authenticateuser
```

| Response
```
A JWT token
```

## GET /devices
Returns a list of all registered devices.

| Example
```
curl --header "Content-Type: application/json" \
     --header "Authorize {JWT Token}
     --request GET \
     http://localhost:3000/devices
```

| Response
```
{
    "serialNumber": "string",
    "status": "string",
    "registrationDate": string,
    "firmwareVersion": "string",
    "secret": "string",
    "inAlarm": "bool"
}
```

## POST /devices
Registers a new device and if successful returns the device details.

| Body
```
{
    "serialNumber": "string",
    "status": "string",
    "registrationDate": string,
    "firmwareVersion": "string",
    "secret": "string",
    "inAlarm": "bool"
}
```

| Example
```
curl --header "Content-Type: application/json" \
     --request POST \
     --data '{ "serialNumber": "222", "status": "healthy", "registrationDate": "2020-04-27T15:54:22Z", "firmwareVersion": "1.0.4", "secret": "222", "inAlarm": false}' \
     http://localhost:3000/devices
```

| Response
```
{
    "serialNumber": "string",
    "status": "string",
    "registrationDate": string,
    "firmwareVersion": "string",
    "secret": "string",
    "inAlarm": "bool"
}
```

| Notes

The call will return BadRequest if SerialNumber, Status, FirmawareNumber, or Secret are missing.

## GET /devices/{SeiralNumber}

Gets device details

| Body
```
{
    "serialNumber": "string",
    "status": "string",
    "registrationDate": string,
    "firmwareVersion": "string",
    "secret": "string",
    "inAlarm": "bool"
}
```

| Example
```
curl --header "Content-Type: application/json" \
     --request GET \
     http://localhost:3000/devices/222
```

| Response
```
{
    "serialNumber": "string",
    "status": "string",
    "registrationDate": string,
    "firmwareVersion": "string",
    "secret": "string",
    "inAlarm": "bool"
}
```

## POST /devices/{SerialNumber}/measurements

Records a list of measurements from a device's sensors.

| Body
```
	[
		{
            "Id": "string",
            "RecordedTime":"string",
			"AirHumidity": "double",
			"CarbonMonoxide": "double",
			"Temperature": "double"
		}
	]
```

| Example
```
curl --header "Content-Type: application/json" \
     --header "Authorization: beaer {FWT Token}
     --request POST \
     --data '[{"Id": "123","RecordedTime": "2020-04-27T15:54:22Z","AirHumidity": 105.5,"CarbonMonoxide": 105.5,"Temperature": 105.5},{"Id": "456","RecordedTime": "2020-04-27T15:54:22Z","AirHumidity": 106.5,"CarbonMonoxide": 106.5,"Temperature": 106.5}]' \
     http://localhost:3000/devices/{SerialNumber}/measurements
```

| Response
```
{
    "serialNumber": "string",
    "status": "string",
    "registrationDate": string,
    "firmwareVersion": "string",
    "secret": "string",
    "inAlarm": "bool"
}
```

| Notes

The call will return BadRequest if Id or RecordedTime is missing.
This call may return with a **503 Service Unavailable** if the service is in maintenance mode. The server can be placed into maintenance with a switch in the DynamoDB table **SytemConfig.InMaintenance**. If the service is unavailable, the device should buffer it's request data and resend when the service is available.

## GET /devices/{SerialNumber}/measurements?{from?}&{to?}
Gets a list of a device's measurements which can be filtered by RecordedTime range.

| Example
```
curl --header "Content-Type: application/json" \
     --header "Authorization: beaer {FWT Token}
     --request GET \
     http://localhost:3000/devices/{SerialNumber}/measurements
```

| Response
```
{
    Status 200 OK
}
```

| Notes

The call will return BadRequest if only 1 query string parameter is found. For example, if a 'from' is set without a 'to' the API will return an error







