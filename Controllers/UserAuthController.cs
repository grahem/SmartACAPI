using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartACAPI.Options;
using SmartACDeviceAPI.Exceptions;
using SmartACDeviceAPI.Models;
using SmartACDeviceAPI.Security;
using SmartACDeviceAPI.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SmartACDeviceAPI.Controllers
{
    //This class is used to authenticate a web user given a SeiralNumber and a Secret via JWT auth.
    [Authorize]
    [ApiController]
    [Route("authenticate-user")]
    public class UserAuthController : ControllerBase
    {
        private readonly UserAuthZService _authServize;
        private readonly ILogger<DevicesController> _logger;
        private readonly Stopwatch _stopwatch;

        public UserAuthController(UserAuthZService authServize, ILogger<DevicesController> logger, Stopwatch stopwatch)
        {
            _authServize = authServize;
            _logger = logger;
            _stopwatch = stopwatch;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Authorize([FromBody] UserAuthenticationModel authModel)
        {
            _stopwatch.Start();

            try
            {
                _logger.LogDebug(String.Format("Authorizing user {0}", authModel.Username));
                var token = await _authServize.Authorize(authModel.Username, authModel.Password);
                if (token != null)
                {
                    _stopwatch.Stop();
                    _logger.LogDebug(String.Format("Authorized user {0} in {1}ms", authModel.Username, _stopwatch.ElapsedMilliseconds));
                    return Ok(token);
                }
            }
            catch (AuthZException)
            {
                return Unauthorized();
            }

            return BadRequest();
        }

        

       
    }
}
