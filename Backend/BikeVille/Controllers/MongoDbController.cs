using BikeVille.BLogic;
using BikeVille.Models.MongoCredentials;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeVille.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MongoDbController : Controller
    {
        private readonly DbManager _dbManager;

        public MongoDbController(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        [Authorize]
        [HttpGet("customers")]
        public async Task<List<CustomerCredentials>> GetAllCustomers()
        {
            return await _dbManager.GetAllCustomers();
        }

        [Authorize]
        [HttpGet("admins")]
        public async Task<List<AdminCredentials>> GetAllAdmins()
        {
            return await _dbManager.GetAllAdmins();
        }

        [HttpPut("update-admin-password")]
        public async Task<IActionResult> UpdateAdminPassword(string email, string newPassword)
        {
            try
            {
                var success = await _dbManager.UpdateAdminPassword(email, newPassword);

                if (success)
                {
                    return Ok("Admin password updated successfully.");
                }

                return NotFound("Admin not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("update-customer-password")]
        public async Task<IActionResult> UpdateCustomerPassword(string email, string newPassword)
        {
            try
            {
                var success = await _dbManager.UpdateCustomerPassword(email, newPassword);

                if (success)
                {
                    return Ok("Customer password updated successfully.");
                }

                return NotFound("Customer not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
