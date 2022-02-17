using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OQAS.XeroIntegration.Logic;
using OQAS.XeroIntegration.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OQAS.XeroIntegration.Presentation.Controllers
{
    [Route("api/cvx")]
    [ApiController]
    public class CvxController : ControllerBase
    {
        readonly IConfiguration configuration;

        public CvxController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpGet("get-configuration")]
        public IActionResult GetConfiguration()
        {
            try
            {
                return Ok(Utils.ReadConfigurationFile(GetConfigurationFilePath()));
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("get-bills")]
        public async Task<IActionResult> GetBills()
        {
            try
            {

                return Ok(await Utils.GetXeroBills(
                                Utils.ReadConfigurationFile(
                                        GetConfigurationFilePath()
                                )).ConfigureAwait(false));
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("generate-uri")]
        public IActionResult GenerateUri(XeroAuthorization authorization)
        {
            try
            {
                Utils.GenerateAuthorizationBaseUri(authorization);
                return Ok(authorization);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("import-voucher")]
        public IActionResult ImportVoucher(List<CheckManagerVoucher> vouchers)
        {
            try
            {
                string connectionString = configuration.GetConnectionString("DefaultConnection");
                return Ok(Utils.SaveVouchers(connectionString, vouchers));
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("generate-tokens")]
        public async Task<IActionResult> GenerateToken(XeroAuthorization authorization)
        {
            try
            {
                await Utils.GenerateToken(authorization).ConfigureAwait(false);
                return Ok(authorization);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("save-configuration")]
        public IActionResult SaveConfiguration(XeroAuthorization authorization)
        {
            try
            {
                Utils.SaveConfigurationFile(GetConfigurationFilePath(), authorization);
                return Ok(authorization);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("get-xero-tenants")]
        public async Task<IActionResult> GetXeroTenants(XeroAuthorization authorization)
        {
            try
            {
                return Ok(await Utils.GetXeroTenants(authorization).ConfigureAwait(false));
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [NonAction]
        private string GetConfigurationFilePath()
        {
            string configFilePath = configuration.GetSection("AppSettings:XeroConfigurationFilePath").Value;
            if (string.IsNullOrWhiteSpace(configFilePath))
                throw new CvxConfigurationException("Missing value for XeroConfigurationFilePath in AppSettings");
            return configFilePath;
        }
    }
}
