using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartACDeviceAPI.Exceptions;
using SmartACDeviceAPI.Models;
using SmartACDeviceAPI.Security;
using SmartACDeviceAPI.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SmartACDeviceAPI.Controllers
{
    //This class is used to authenticate a device given a SeiralNumber and a Secret via JWT auth.
    [Authorize]
    [ApiController]
    [Route("authenticate-device")]
    public class DeviceAuthZController : ControllerBase
    {
        private readonly DeviceAuthZService _deviceAuthZService;
        private readonly ILogger<DeviceAuthZController> _logger;

        public DeviceAuthZController(DeviceAuthZService deviceAuthZService, ILogger<DeviceAuthZController> logger)
        {
            _deviceAuthZService = deviceAuthZService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Authorize([FromBody] DeviceAuthenticationModel authorizationModel)
        {
            var watch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogDebug(String.Format("Authenticating serial number {0}", authorizationModel.SerialNumber));
                var token = await _deviceAuthZService.Authorize(authorizationModel.SerialNumber, authorizationModel.Secret);
                if (token != null)
                {
                    watch.Stop();
                    _logger.LogDebug(String.Format("Authorized serial number {0} in {1}ms", authorizationModel.SerialNumber, watch.ElapsedMilliseconds));
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
