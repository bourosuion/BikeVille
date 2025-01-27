using Microsoft.AspNetCore.Mvc;
using BikeVille.BLogic;
using BikeVille.Models;

namespace BikeVille.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly DbManager _dbManager;

        public ProductsController(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            try
            {
                var product = await _dbManager.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound();

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("GetFiltered")]
        public async Task<ActionResult> GetFilteredProducts([FromQuery] ProductFilterParameters parameters)
        {
            try
            {
                var (products, totalCount) = await _dbManager.GetFilteredProductsAsync(parameters);

                return Ok(new
                {
                    products,
                    totalCount,
                    pageNumber = parameters.PageNumber,
                    totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("Search")]
        public async Task<ActionResult> SearchProducts([FromQuery] string searchTerm, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var (products, totalCount) = await _dbManager.SearchProductsAsync(searchTerm, pageNumber, pageSize);

                return Ok(new
                {
                    products,
                    totalCount,
                    pageNumber,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }

    public class ProductFilterParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public string[]? SelectedColors { get; set; }
        public string[]? SelectedSizes { get; set; }
        public int[]? PriceRange { get; set; } // [minPrice, maxPrice]
        public string? SortBy { get; set; }
        public bool SortAscending { get; set; } = true;
    }
}