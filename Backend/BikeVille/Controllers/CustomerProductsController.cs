using Microsoft.AspNetCore.Mvc;
using BikeVille.BLogic;
using BikeVille.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BikeVille.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class CustomerProductsController : ControllerBase
    {
        private readonly DbManager _dbManager;

        public CustomerProductsController(DbManager dbManager)
        {
            _dbManager = dbManager;
        }
        private async Task<int> GetCustomerIdFromToken()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value
                ?? throw new Exception("Email non trovata nel token");
            return await _dbManager.GetCustomerIdByEmail(email);
        }

        // GET(All) -> WhishList -> InCart = 0   -> Pulsante: NavBar   -> Visualizzo i prodotti associati al mio ID nella WishList
        // GET(All) -> Cart      -> InCart = 1   -> Pulsante: NavBar   -> Visualizzo i prodotti associati al mio ID nel Cart

        [HttpGet]
        public async Task<ActionResult> GetProducts([FromQuery] bool isCart)
        {
            try
            {
                var customerId = await GetCustomerIdFromToken();
                var productIds = await _dbManager.GetCustomerProductsAsync(customerId, isCart);

                var products = new List<Product>();
                foreach (var id in productIds)
                {
                    var product = await _dbManager.GetProductByIdAsync(id);
                    if (product != null)
                        products.Add(product);
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // POST     -> WhishList -> InCart = 0   -> Pulsante: Cuore    -> Vedo i prodotti nella ricerca e li inserisco con <3 nella WishList
        // POST     -> Cart      -> InCart = 1   -> Pulsante: Carrello -> Vedo i prodotti nella ricerca e li inserisco con 'cart' nel Carrello

        [HttpPost("{productId}")]
        public async Task<ActionResult> AddProduct(int productId, [FromQuery] bool isCart)
        {
            try
            {
                var customerId = await GetCustomerIdFromToken();
                await _dbManager.AddCustomerProductAsync(customerId, productId, isCart);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // PUT      -> WishList  -> InCart = 0/1 -> Pulsante: Carrello -> Vedo il prodotto nella WishList e lo voglio spostare nel Cart, quindi modifico valore di InCart
        // PUT      -> Cart      -> InCart = 1/0 -> Pulsante: Vedo il prodotto nel Cart ma non sono più sicuro di volerlo acquistare,

        [HttpPut("{productId}")]
        public async Task<ActionResult> MoveProduct(int productId, [FromQuery] bool isCart)
        {
            try
            {
                var customerId = await GetCustomerIdFromToken();
                await _dbManager.UpdateInCartStatusAsync(customerId, productId, isCart);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPut("{productId}/quantity")]
        public async Task<ActionResult> UpdateQuantity(int productId, [FromBody] int quantity)
        {
            try
            {
                var customerId = await GetCustomerIdFromToken();
                await _dbManager.UpdateCartQuantityAsync(customerId, productId, quantity);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPut("checkout/confirm")]
        public async Task<IActionResult> ConfirmPurchase()
        {
            try
            {
                var customerId = await GetCustomerIdFromToken();

                // Verifica cart vuoto
                var cartProducts = await _dbManager.GetCustomerProductsAsync(customerId, true);
                if (!cartProducts.Any())
                {
                    return BadRequest(new { message = "Il carrello è vuoto" });
                }

                // Verifica indirizzo
                var addresses = await _dbManager.GetCustomerAddressesAsync(customerId);
                if (!addresses.Any())
                {
                    return BadRequest(new
                    {
                        message = "È necessario aggiungere un indirizzo di spedizione prima di procedere all'acquisto"
                    });
                }

                await _dbManager.ConfirmPurchaseAsync(customerId);
                return Ok(new
                {
                    message = "Acquisto confermato con successo"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Errore durante la conferma dell'acquisto: {ex.Message}"
                });
            }
        }

        // quindi lo sposto nella WishList
        // DELETE   -> Cart      -> InCart

        [HttpDelete("{productId}")]
        public async Task<ActionResult> RemoveProduct(int productId)
        {
            try
            {
                var customerId = await GetCustomerIdFromToken();
                await _dbManager.RemoveCustomerProductAsync(customerId, productId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }


    }
}
