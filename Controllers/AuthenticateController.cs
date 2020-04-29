using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartACDeviceAPI.Models;
using SmartACDeviceAPI.Security;
using System;
using System.Linq;

namespace SmartACDeviceAPI.Controllers
{
    //This class is used to authenticate a device given a SeiralNumber and a Secret via JWT auth.
    [Authorize]
    [ApiController]
    [Route("authenticate")]
    public class AuthenticateController : ControllerBase
    {

        private readonly IConfiguration _config;
        private readonly IDynamoDBContext _context;
        private readonly ILogger<DevicesController> logger;

        public AuthenticateController(IConfiguration config, IDynamoDBContext context, ILogger<DevicesController> logger)
        {
            _config = config;
            _context = context;
            this.logger = logger;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Authenticate([FromBody]AuthorizationModel authorizationModel)
        {
            if (String.IsNullOrEmpty(authorizationModel.SerialNumber)
                || String.IsNullOrEmpty(authorizationModel.Secret))
            {
                return BadRequest();
            }

            //Ensure we have the supplied device in DynamoDB
            var query = _context.QueryAsync<Device>(authorizationModel.SerialNumber);
            var resultList = query.GetRemainingAsync().Result;
            if (resultList.Count == 0)
            {
                return Unauthorized();
            }
            else
            {
                //Check that the supplied secret matches the database.
                if (String.Equals(authorizationModel.Secret, resultList.First().Secret))
                {
                    var tokenString = JWTHelper.GenerateJWTToken(_config);
                    return Ok(tokenString);
                }
                else
                {
                    logger.LogInformation(string.Format("Device Authorization Failed: {0}", authorizationModel.SerialNumber));
                    return Unauthorized();
                }
            }            
        }
    }
}
