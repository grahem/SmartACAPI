using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartACAPI.Options;
using SmartACDeviceAPI.Models;
using SmartACDeviceAPI.Security;
using System;

namespace SmartACDeviceAPI.Controllers
{
    //This class is used to authenticate a web user given a SeiralNumber and a Secret via JWT auth.
    [Authorize]
    [ApiController]
    [Route("authenticate-user")]
    public class UserAuthenticateController : ControllerBase
    {

        private readonly IOptionsMonitor<AuthZOptions> _options;
        private readonly IDynamoDBContext _context;
        private readonly ILogger<DevicesController> logger;

        public UserAuthenticateController(IOptionsMonitor<AuthZOptions> options, IDynamoDBContext context, ILogger<DevicesController> logger)
        {
            _options = options;
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
                    var tokenString = JWTHelper.GenerateJWTToken(_options);
                    
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
