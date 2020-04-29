//using System;
//using System.IO;
//using Amazon;
//using Amazon.SecretsManager;
//using Amazon.SecretsManager.Model;
//using System.Text.Json;
//using System.Text.Json.Serialization;

//namespace SmartACDeviceAPI.Security
//{
//    public class AWSDBSecret
//    {

//        public static SmartACDBConnection GetSecret()
//        {
//            string secretName = "smartac-db";
//            string region = "us-west-1";
//            string secret = "smartac-db";

//            MemoryStream memoryStream = new MemoryStream();

//            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

//            GetSecretValueRequest request = new GetSecretValueRequest();
//            request.SecretId = secretName;
//            request.VersionStage = "AWSCURRENT";

//            GetSecretValueResponse response = null;

//            response = client.GetSecretValueAsync(request).Result;

//            if (response.SecretString != null)
//            {
//                secret = response.SecretString;
//            }
//            else
//            {
//                memoryStream = response.SecretBinary;
//                StreamReader reader = new StreamReader(memoryStream);
//                secret = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
//            }

//            return JsonSerializer.Deserialize<SmartACDBConnection>(secret);
//        }

//    }
//}