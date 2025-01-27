using BikeVille.BLogic;
using BikeVille.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BikeVille.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly DbManager _dbManager;

        public AddressController(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        private async Task<int> GetCustomerIdFromToken()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value
                ?? throw new Exception("Email non trovata nel token");
            return await _dbManager.GetCustomerIdByEmail(email);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Address>>> GetAddresses()
        {
            try
            {
                var customerId = await GetCustomerIdFromToken();
                var addresses = await _dbManager.GetCustomerAddressesAsync(customerId);
                return Ok(addresses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Errore nel recupero degli indirizzi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Address>> AddAddress(AddressDTO addressDTO)
        {
            try
            {
                var customerId = await GetCustomerIdFromToken();

                var address = new Address
                {
                    AddressLine1 = addressDTO.AddressLine1,
                    AddressLine2 = addressDTO.AddressLine2,
                    City = addressDTO.City,
                    StateProvince = addressDTO.StateProvince,
                    CountryRegion = addressDTO.CountryRegion,
                    PostalCode = addressDTO.PostalCode
                };

                var addressId = await _dbManager.AddCustomerAddressAsync(customerId, address);
                address.AddressId = addressId;

                return CreatedAtAction(nameof(GetAddresses), new { id = addressId }, address);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Errore nell'inserimento dell'indirizzo: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(int id, AddressDTO addressDTO)
        {
            try
            {
                var customerId = await GetCustomerIdFromToken();

                var address = new Address
                {
                    AddressId = id,
                    AddressLine1 = addressDTO.AddressLine1,
                    AddressLine2 = addressDTO.AddressLine2,
                    City = addressDTO.City,
                    StateProvince = addressDTO.StateProvince,
                    CountryRegion = addressDTO.CountryRegion,
                    PostalCode = addressDTO.PostalCode
                };

                await _dbManager.UpdateCustomerAddressAsync(customerId, address);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Errore nell'aggiornamento dell'indirizzo: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            try
            {
                var customerId = await GetCustomerIdFromToken();
                await _dbManager.DeleteCustomerAddressAsync(customerId, id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Errore nella cancellazione dell'indirizzo: {ex.Message}" });
            }
        }
    }

    public class AddressDTO
    {
        public string AddressLine1 { get; set; } = null!;     
        public string? AddressLine2 { get; set; }             
        public string City { get; set; } = null!;             
        public string StateProvince { get; set; } = null!;    
        public string CountryRegion { get; set; } = null!;    
        public string PostalCode { get; set; } = null!;       
    }
}
