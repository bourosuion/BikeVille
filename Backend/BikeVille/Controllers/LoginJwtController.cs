using BikeVille.BLogic;
using BikeVille.Models.MongoModels;
using Microsoft.AspNetCore.Mvc;

namespace BikeVille.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginJwtController : ControllerBase
    {
        private readonly DbManager _dbManager;
        private readonly JwtSettings _jwtSettings;

        public LoginJwtController(DbManager dbManager, JwtSettings jwtSettings)
        {
            _dbManager = dbManager;
            _jwtSettings = jwtSettings;
        }

        [HttpPost("AuthenticateAdmin")]
        public async Task<ActionResult> AuthenticateAdmin([FromBody] Credentials credentials)
        {
            if (credentials == null || string.IsNullOrEmpty(credentials.Email) || string.IsNullOrEmpty(credentials.Password))
            {
                return BadRequest("Invalid credentials");
            }

            var token = await _dbManager.AuthenticateAdmin(credentials.Email, credentials.Password);

            if (token != null)
            {
                return Ok(new { token });
            }

            return Unauthorized();
        }

        [HttpPost("AuthenticateCustomer")]
        public async Task<ActionResult> AuthenticateCustomer([FromBody] Credentials credentials)
        {
            if (credentials == null || string.IsNullOrEmpty(credentials.Email) || string.IsNullOrEmpty(credentials.Password))
            {
                return BadRequest("Invalid credentials");
            }

            var token = await _dbManager.AuthenticateCustomer(credentials.Email, credentials.Password);

            if (token != null)
            {
                return Ok(new { token });
            }

            return Unauthorized();
        }

    }

    public class Credentials
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
