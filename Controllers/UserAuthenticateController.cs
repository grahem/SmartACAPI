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
    [Authorize]
    [ApiController]
    [Route("authenticateuser")]
    public class UserAuthenticateController : ControllerBase
    {

        private readonly IConfiguration _config;
        private readonly IDynamoDBContext _context;
        private readonly ILogger<DevicesController> logger;

        public UserAuthenticateController(IConfiguration config, IDynamoDBContext context, ILogger<DevicesController> logger)
        {
            _config = config;
            _context = context;
            this.logger = logger;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Authenticate([FromBody]UserAuthorizationModel authorizationModel)
        {
            logger.LogInformation(string.Format("Authorizing User: {0}", authorizationModel.Username));
            if (String.IsNullOrEmpty(authorizationModel.Username)
                || String.IsNullOrEmpty(authorizationModel.Password))
            {
                return BadRequest();
            }

            var user = _context.LoadAsync<User>(authorizationModel.Username).Result;
            if (user is null)
            {
                logger.LogInformation(string.Format("User not found: {0}", authorizationModel.Username));
                return Unauthorized();
            }
            else
            {
                //TODO: encrypt password at rest
                if (String.Equals(authorizationModel.Password, user.Password))
                {
                    logger.LogInformation(string.Format("Authorized User: {0}", authorizationModel.Username));
                    var tokenString = JWTHelper.GenerateJWTToken(_config);
                    
                    return Ok(tokenString);
                }
                else
                {
                    logger.LogInformation(string.Format("Authorization Failed. Bad Password for {0}", authorizationModel.Username));
                    return Unauthorized();
                }
            }            
        }

        

       
    }
}
