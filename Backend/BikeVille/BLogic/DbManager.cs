using BikeVille.Models.MongoCredentials;
using BikeVille.Models.MongoModels;
using BikeVille.Models;
using BikeVille.Utilities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Data;
using BikeVille.Controllers;

namespace BikeVille.BLogic
{
    public class DbManager
    {
        private readonly IMongoCollection<CustomerCredentials> _mongoCustomerCollection;
        private readonly IMongoCollection<AdminCredentials> _mongoAdminCollection;
        private readonly string _sqlConnectionString;
        private readonly JwtSettings _jwtSettings;

        public DbManager(IConfiguration configuration, IOptions<MongoDbConfig> options, JwtSettings jwtSettings)
        {
            try
            {
                // Configurazione SSMS
                _sqlConnectionString = configuration.GetConnectionString("MainSqlConnection")
                           ?? throw new InvalidOperationException("Connection string 'MainSqlConnection' not found.");
                _jwtSettings = jwtSettings;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in SQL Server configuration: ", ex);
            }

            try
            {
                // Configurazione MongoDB
                var mongoClient = new MongoClient(options.Value.ConnectionString);
                var mongoDB = mongoClient.GetDatabase(options.Value.DatabaseName);
                _mongoCustomerCollection = mongoDB.GetCollection<CustomerCredentials>(options.Value.CustomerCollectionName);
                _mongoAdminCollection = mongoDB.GetCollection<AdminCredentials>(options.Value.AdminCollectionName);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in MongoDB configuration: ", ex);
            }
        }

        // EQUIVALENTE DEL CHECKOPEN E CHECKCLOSE
        public string IsSqlServerOnlineDetailed()
        {
            try
            {
                using var connection = new SqlConnection(_sqlConnectionString);
                connection.Open();
                return "Connection successful.";
            }
            catch (Exception ex)
            {
                return $"Connection failed: {ex.Message}";
            }
        }

        public void ExecuteSqlCommand(string query, Action<SqlCommand> configureCommand)
        {
            using var connection = new SqlConnection(_sqlConnectionString);
            connection.Open();
            using var command = new SqlCommand(query, connection);
            configureCommand(command);
        }

        public async Task<string> AuthenticateAdmin(string email, string password)
        {
            Console.WriteLine($"Authenticating admin with email: {email}");

            try
            {
                // Recupera il documento dall'admin collection
                var admin = await _mongoAdminCollection
                    .Find(Builders<AdminCredentials>.Filter.Eq("EmailAddress", email))
                    .FirstOrDefaultAsync();

                if (admin == null)
                {
                    Console.WriteLine("Admin not found in the database.");
                    return null;
                }

                Console.WriteLine($"Admin found: {admin.Name}");
                Console.WriteLine($"Stored Salt: {admin.PasswordSalt}");
                Console.WriteLine($"Stored Hash: {admin.PasswordHash}");

                var (computedHash, _) = PasswordHasher.HashPassword(password, admin.PasswordSalt);
                Console.WriteLine($"Computed Hash: {computedHash}");

                // Verifica della password
                bool isPasswordValid = PasswordHasher.VerifyPassword(password, admin.PasswordHash, admin.PasswordSalt);
                Console.WriteLine($"Password verification result: {isPasswordValid}");

                if (!isPasswordValid)
                {
                    Console.WriteLine("Password verification failed.");
                    return null; // Password errata
                }

                // Generazione del token JWT
                string token = GenerateJwtToken(email);
                Console.WriteLine($"JWT Token generated: {token}");

                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during authentication: {ex.Message}");
                return null;
            }
        }

        public async Task<string> AuthenticateCustomer(string email, string password)
        {
            Console.WriteLine($"Authenticating customer with email: {email}");

            try
            {
                // Recupera il documento dall'admin collection
                var customer = await _mongoCustomerCollection
                    .Find(Builders<CustomerCredentials>.Filter.Eq("EmailAddress", email))
                    .FirstOrDefaultAsync();

                if (customer == null)
                {
                    Console.WriteLine("Customer not found in the database.");
                    return null;
                }

                Console.WriteLine($"Customer found: {customer.CompanyName}");
                Console.WriteLine($"Stored Salt: {customer.PasswordSalt}");
                Console.WriteLine($"Stored Hash: {customer.PasswordHash}");

                var (computedHash, _) = PasswordHasher.HashPassword(password, customer.PasswordSalt);
                Console.WriteLine($"Computed Hash: {computedHash}");

                // Verifica della password
                bool isPasswordValid = PasswordHasher.VerifyPassword(password, customer.PasswordHash, customer.PasswordSalt);
                Console.WriteLine($"Password verification result: {isPasswordValid}");

                if (!isPasswordValid)
                {
                    Console.WriteLine("Password verification failed.");
                    return null; // Password errata
                }

                // Generazione del token JWT
                string token = GenerateJwtToken(email);
                Console.WriteLine($"JWT Token generated: {token}");

                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during authentication: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> RegisterCustomer(Customer customer, string plainPassword)
        {
            try
            {
                if (customer == null)
                {
                    throw new ArgumentNullException(nameof(customer));
                }

                var (hash, salt) = PasswordHasher.HashPassword(plainPassword);

                int newCustomerId = 0;
                using (var connection = new SqlConnection(_sqlConnectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("spAddCustomer", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Aggiungi i parametri per la stored procedure
                        command.Parameters.AddWithValue("@FirstName", $"{customer.FirstName}");
                        command.Parameters.AddWithValue("@LastName", $"{customer.LastName}");
                        command.Parameters.AddWithValue("@EmailAddress", $"{customer.EmailAddress}");

                        // Recupero l'ID del nuovo cliente
                        newCustomerId = Convert.ToInt32(await command.ExecuteScalarAsync());
                        Console.WriteLine($"New CustomerID: {newCustomerId}");
                    }
                }

                var customerCredentials = new CustomerCredentials
                {
                    CustomerID = newCustomerId,
                    CompanyName = customer.FirstName,
                    EmailAddress = customer.EmailAddress,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    ModifiedDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };

                await _mongoCustomerCollection.InsertOneAsync(customerCredentials);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during customer registration: {ex.Message}");
                return false;
            }
        }

        private string GenerateJwtToken(string email)
        {
            var secretKey = _jwtSettings.SecretKey;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, email)
                }),
                Expires = DateTime.Now.AddMinutes(_jwtSettings.TokenExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> UpdateAdminPassword(string email, string newPassword)
        {
            try
            {
                var admin = await _mongoAdminCollection
                    .Find(Builders<AdminCredentials>.Filter.Eq("EmailAddress", email))
                    .FirstOrDefaultAsync();

                if (admin == null)
                {
                    Console.WriteLine("Admin not found.");
                    return false;
                }

                var (newHash, newSalt) = PasswordHasher.HashPassword(newPassword);
                admin.PasswordHash = newHash;
                admin.PasswordSalt = newSalt;
                admin.ModifiedDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); ;

                var result = await _mongoAdminCollection.ReplaceOneAsync(
                    Builders<AdminCredentials>.Filter.Eq("EmailAddress", email),
                    admin);

                return result.IsAcknowledged && result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating admin password: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateCustomerPassword(string email, string newPassword)
        {
            try
            {
                var customer = await _mongoCustomerCollection
                    .Find(Builders<CustomerCredentials>.Filter.Eq("EmailAddress", email))
                    .FirstOrDefaultAsync();

                if (customer == null)
                {
                    Console.WriteLine("Admin not found.");
                    return false;
                }

                var (newHash, newSalt) = PasswordHasher.HashPassword(newPassword);
                customer.PasswordHash = newHash;
                customer.PasswordSalt = newSalt;
                customer.ModifiedDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

                var result = await _mongoCustomerCollection.ReplaceOneAsync(
                    Builders<CustomerCredentials>.Filter.Eq("EmailAddress", email),
                    customer);

                return result.IsAcknowledged && result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating admin password: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CustomerCredentials>> GetAllCustomers()
        {
            return await _mongoCustomerCollection.Find(_ => true).ToListAsync();
        }

        public async Task<List<AdminCredentials>> GetAllAdmins()
        {
            return await _mongoAdminCollection.Find(_ => true).ToListAsync();
        }

        public async Task<(List<Product> Products, int TotalCount)> GetFilteredProductsAsync(ProductFilterParameters parameters)
        {
            var products = new List<Product>();
            var totalCount = 0;

            using (var connection = new SqlConnection(_sqlConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("spGetFilteredProducts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@SubCategoryId", parameters.SubCategoryId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CategoryId", parameters.CategoryId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Colors", parameters.SelectedColors != null ? string.Join(",", parameters.SelectedColors) : (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Sizes", parameters.SelectedSizes != null ? string.Join(",", parameters.SelectedSizes) : (object)DBNull.Value);
                    command.Parameters.AddWithValue("@MinPrice", parameters.PriceRange?.Length > 0 ? parameters.PriceRange[0] : (object)DBNull.Value);
                    command.Parameters.AddWithValue("@MaxPrice", parameters.PriceRange?.Length > 1 ? parameters.PriceRange[1] : (object)DBNull.Value);
                    command.Parameters.AddWithValue("@SortBy", parameters.SortBy ?? "");
                    command.Parameters.AddWithValue("@SortAscending", parameters.SortAscending);
                    command.Parameters.AddWithValue("@PageNumber", parameters.PageNumber);
                    command.Parameters.AddWithValue("@PageSize", parameters.PageSize);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Product
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                ProductNumber = reader.GetString(reader.GetOrdinal("ProductNumber")),
                                Color = reader.IsDBNull(reader.GetOrdinal("Color")) ? null : reader.GetString(reader.GetOrdinal("Color")),
                                StandardCost = reader.GetDecimal(reader.GetOrdinal("StandardCost")),
                                ListPrice = reader.GetDecimal(reader.GetOrdinal("ListPrice")),
                                Size = reader.IsDBNull(reader.GetOrdinal("Size")) ? null : reader.GetString(reader.GetOrdinal("Size")),
                                Weight = reader.IsDBNull(reader.GetOrdinal("Weight")) ? null : reader.GetDecimal(reader.GetOrdinal("Weight")),
                                ProductCategoryId = reader.IsDBNull(reader.GetOrdinal("ProductCategoryId")) ? null : reader.GetInt32(reader.GetOrdinal("ProductCategoryId")),
                                ProductModelId = reader.IsDBNull(reader.GetOrdinal("ProductModelId")) ? null : reader.GetInt32(reader.GetOrdinal("ProductModelId")),
                                SellStartDate = reader.GetDateTime(reader.GetOrdinal("SellStartDate")),
                                SellEndDate = reader.IsDBNull(reader.GetOrdinal("SellEndDate")) ? null : reader.GetDateTime(reader.GetOrdinal("SellEndDate")),
                                DiscontinuedDate = reader.IsDBNull(reader.GetOrdinal("DiscontinuedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("DiscontinuedDate")),
                                ThumbNailPhoto = reader.IsDBNull(reader.GetOrdinal("ThumbNailPhoto")) ? null : (byte[])reader["ThumbNailPhoto"],
                                ThumbnailPhotoFileName = reader.IsDBNull(reader.GetOrdinal("ThumbnailPhotoFileName")) ? null : reader.GetString(reader.GetOrdinal("ThumbnailPhotoFileName")),
                                ModifiedDate = reader.GetDateTime(reader.GetOrdinal("ModifiedDate"))
                            });
                        }

                        await reader.NextResultAsync();
                        if (await reader.ReadAsync())
                        {
                            totalCount = reader.GetInt32(0);
                        }
                    }
                }
            }

            return (products, totalCount);
        }

        public async Task<(List<Product> Products, int TotalCount)> SearchProductsAsync(string searchTerm, int pageNumber, int pageSize)
        {
            var products = new List<Product>();
            var totalCount = 0;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return (products, totalCount);
            }

            using (var connection = new SqlConnection(_sqlConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("spSearchProducts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@SearchTerm", searchTerm);
                    command.Parameters.AddWithValue("@PageNumber", pageNumber);
                    command.Parameters.AddWithValue("@PageSize", pageSize);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Product
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                ProductNumber = reader.GetString(reader.GetOrdinal("ProductNumber")),
                                Color = reader.IsDBNull(reader.GetOrdinal("Color")) ? null : reader.GetString(reader.GetOrdinal("Color")),
                                StandardCost = reader.GetDecimal(reader.GetOrdinal("StandardCost")),
                                ListPrice = reader.GetDecimal(reader.GetOrdinal("ListPrice")),
                                Size = reader.IsDBNull(reader.GetOrdinal("Size")) ? null : reader.GetString(reader.GetOrdinal("Size")),
                                Weight = reader.IsDBNull(reader.GetOrdinal("Weight")) ? null : reader.GetDecimal(reader.GetOrdinal("Weight")),
                                ProductCategoryId = reader.IsDBNull(reader.GetOrdinal("ProductCategoryId")) ? null : reader.GetInt32(reader.GetOrdinal("ProductCategoryId")),
                                ProductModelId = reader.IsDBNull(reader.GetOrdinal("ProductModelId")) ? null : reader.GetInt32(reader.GetOrdinal("ProductModelId")),
                                SellStartDate = reader.GetDateTime(reader.GetOrdinal("SellStartDate")),
                                SellEndDate = reader.IsDBNull(reader.GetOrdinal("SellEndDate")) ? null : reader.GetDateTime(reader.GetOrdinal("SellEndDate")),
                                DiscontinuedDate = reader.IsDBNull(reader.GetOrdinal("DiscontinuedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("DiscontinuedDate")),
                                ThumbNailPhoto = reader.IsDBNull(reader.GetOrdinal("ThumbNailPhoto")) ? null : (byte[])reader["ThumbNailPhoto"],
                                ThumbnailPhotoFileName = reader.IsDBNull(reader.GetOrdinal("ThumbnailPhotoFileName")) ? null : reader.GetString(reader.GetOrdinal("ThumbnailPhotoFileName")),
                                ModifiedDate = reader.GetDateTime(reader.GetOrdinal("ModifiedDate"))
                            });
                        }

                        await reader.NextResultAsync();
                        if (await reader.ReadAsync())
                        {
                            totalCount = reader.GetInt32(0);
                        }
                    }
                }
            }

            return (products, totalCount);
        }

        public async Task<List<int>> GetCustomerProductsAsync(int customerId, bool isCart)
        {
            using (var connection = new SqlConnection(_sqlConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("spGetCustomerProducts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    command.Parameters.AddWithValue("@IsCart", isCart);

                    var productIds = new List<int>();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            productIds.Add(reader.GetInt32(0));
                        }
                    }
                    return productIds;
                }
            }
        }

        public async Task<int> GetCustomerIdByEmail(string email)
        {
            using (var connection = new SqlConnection(_sqlConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT CustomerID FROM SalesLT.Customer WHERE EmailAddress = @Email", connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    var result = await command.ExecuteScalarAsync();
                    return result != null ? (int)result : throw new Exception("Customer non trovato");
                }
            }
        }

        public async Task<Product> GetProductByIdAsync(int productId)
        {
            using (var connection = new SqlConnection(_sqlConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("spGetProductById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProductId", productId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Product
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                ProductNumber = reader.GetString(reader.GetOrdinal("ProductNumber")),
                                Color = reader.IsDBNull(reader.GetOrdinal("Color")) ? null : reader.GetString(reader.GetOrdinal("Color")),
                                StandardCost = reader.GetDecimal(reader.GetOrdinal("StandardCost")),
                                ListPrice = reader.GetDecimal(reader.GetOrdinal("ListPrice")),
                                Size = reader.IsDBNull(reader.GetOrdinal("Size")) ? null : reader.GetString(reader.GetOrdinal("Size")),
                                Weight = reader.IsDBNull(reader.GetOrdinal("Weight")) ? null : reader.GetDecimal(reader.GetOrdinal("Weight")),
                                ProductCategoryId = reader.IsDBNull(reader.GetOrdinal("ProductCategoryID")) ? null : reader.GetInt32(reader.GetOrdinal("ProductCategoryID")),
                                ProductModelId = reader.IsDBNull(reader.GetOrdinal("ProductModelID")) ? null : reader.GetInt32(reader.GetOrdinal("ProductModelID")),
                                SellStartDate = reader.GetDateTime(reader.GetOrdinal("SellStartDate")),
                                SellEndDate = reader.IsDBNull(reader.GetOrdinal("SellEndDate")) ? null : reader.GetDateTime(reader.GetOrdinal("SellEndDate")),
                                DiscontinuedDate = reader.IsDBNull(reader.GetOrdinal("DiscontinuedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("DiscontinuedDate")),
                                ThumbNailPhoto = reader.IsDBNull(reader.GetOrdinal("ThumbNailPhoto")) ? null : (byte[])reader["ThumbNailPhoto"],
                                ThumbnailPhotoFileName = reader.IsDBNull(reader.GetOrdinal("ThumbnailPhotoFileName")) ? null : reader.GetString(reader.GetOrdinal("ThumbnailPhotoFileName")),
                                ModifiedDate = reader.GetDateTime(reader.GetOrdinal("ModifiedDate"))
                            };
                        }
                        return null;
                    }
                }
            }
        }

        public async Task UpdateInCartStatusAsync(int customerId, int productId, bool isCart)
        {
            using (var connection = new SqlConnection(_sqlConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("spUpdateInCartStatus", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.Parameters.AddWithValue("@IsCart", isCart);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task AddCustomerProductAsync(int customerId, int productId, bool isCart)
        {
            using (var connection = new SqlConnection(_sqlConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("spAddCustomerProduct", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.Parameters.AddWithValue("@IsCart", isCart);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task RemoveCustomerProductAsync(int customerId, int productId)
        {
            using (var connection = new SqlConnection(_sqlConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("spRemoveCustomerProduct", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    command.Parameters.AddWithValue("@ProductId", productId);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateCartQuantityAsync(int customerId, int productId, int quantity)
        {
            using (var connection = new SqlConnection(_sqlConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("spUpdateCartQuantity", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.Parameters.AddWithValue("@Quantity", quantity);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<Address>> GetCustomerAddressesAsync(int customerId)
        {
            var addresses = new List<Address>();
            using (var connection = new SqlConnection(_sqlConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("dbo.spGetCustomerAddresses", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CustomerId", customerId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            addresses.Add(new Address
                            {
                                AddressId = reader.GetInt32(reader.GetOrdinal("AddressID")),
                                AddressLine1 = reader.GetString(reader.GetOrdinal("AddressLine1")),
                                AddressLine2 = reader.IsDBNull(reader.GetOrdinal("AddressLine2")) ? null : reader.GetString(reader.GetOrdinal("AddressLine2")),
                                City = reader.GetString(reader.GetOrdinal("City")),
                                StateProvince = reader.GetString(reader.GetOrdinal("StateProvince")),
                                CountryRegion = reader.GetString(reader.GetOrdinal("CountryRegion")),
                                PostalCode = reader.GetString(reader.GetOrdinal("PostalCode"))
                            });
                        }
                    }
                }
            }
            return addresses;
        }

        public async Task<int> AddCustomerAddressAsync(int customerId, Address address)
        {
            using (var connection = new SqlConnection(_sqlConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("dbo.spAddCustomerAddress", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    command.Parameters.AddWithValue("@AddressLine1", address.AddressLine1);
                    command.Parameters.AddWithValue("@AddressLine2", (object?)address.AddressLine2 ?? DBNull.Value);
                    command.Parameters.AddWithValue("@City", address.City);
                    command.Parameters.AddWithValue("@StateProvince", address.StateProvince);
                    command.Parameters.AddWithValue("@CountryRegion", address.CountryRegion);
                    command.Parameters.AddWithValue("@PostalCode", address.PostalCode);
                    command.Parameters.AddWithValue("@AddressType", "Shipping");
                    command.Parameters.AddWithValue("@IsPrimary", true);

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task UpdateCustomerAddressAsync(int customerId, Address address)
        {
            using (var connection = new SqlConnection(_sqlConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("dbo.spUpdateCustomerAddress", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    command.Parameters.AddWithValue("@AddressId", address.AddressId);
                    command.Parameters.AddWithValue("@AddressLine1", address.AddressLine1);
                    command.Parameters.AddWithValue("@AddressLine2", (object?)address.AddressLine2 ?? DBNull.Value);
                    command.Parameters.AddWithValue("@City", address.City);
                    command.Parameters.AddWithValue("@StateProvince", address.StateProvince);
                    command.Parameters.AddWithValue("@CountryRegion", address.CountryRegion);
                    command.Parameters.AddWithValue("@PostalCode", address.PostalCode);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteCustomerAddressAsync(int customerId, int addressId)
        {
            using (var connection = new SqlConnection(_sqlConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("dbo.spDeleteCustomerAddress", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    command.Parameters.AddWithValue("@AddressId", addressId);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task ConfirmPurchaseAsync(int customerId)
        {
            using (var connection = new SqlConnection(_sqlConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("spConfirmPurchase", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
