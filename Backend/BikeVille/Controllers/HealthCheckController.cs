using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace BikeVille.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HealthCheckController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/HealthCheck
        [HttpGet]
        public IActionResult Get()
        {
            // Return a simple success message
            return Ok(new { message = "Server is up and running!" });
        }

        // Optional: Test a database connection
        [HttpGet("database")]
        public IActionResult TestDatabaseConnection()
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("MainSqlConnection");

                // Example of testing database connection
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                    {
                        return Ok(new { message = "Database connection is successful!" });
                    }
                }
                return StatusCode(500, new { message = "Database connection failed!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Database connection failed!", error = ex.Message });
            }
        }
    }
}
