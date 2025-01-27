using BikeVille.BLogic;
using BikeVille.Models;
using Microsoft.AspNetCore.Mvc;

namespace BikeVille.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly DbManager _dbManager;

        public CustomersController(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        [HttpPost("register-customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] CustomerMainData customer)
        {
            try
            {
                if (string.IsNullOrEmpty(customer.Password))
                {
                    return BadRequest("Password cannot be null or empty.");
                }

                Customer cust = new(
                    $"{customer.FirstName}",
                    $"{customer.LastName}",
                    $"{customer.EmailAddress}"
                );

                bool success = await _dbManager.RegisterCustomer(cust, customer.Password);

                if (success)
                {
                    return CreatedAtAction("RegisterCustomer", new { id = cust.CustomerId }, cust);
                }

                return BadRequest("Customer registration failed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }

    public class CustomerMainData
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
