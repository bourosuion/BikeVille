using System.Security.Cryptography;
using System.Text;


namespace BikeVille.Utilities
{
    public static class PasswordHasher
    {
        public static (string Hash, string Salt) HashPassword(string password, string? existingSalt = null)
        {
            byte[] salt;

            if (string.IsNullOrEmpty(existingSalt))
            {
                salt = RandomNumberGenerator.GetBytes(32); // Genera un nuovo salt
            }
            else
            {
                salt = Convert.FromBase64String(existingSalt); // Usa il salt esistente
            }

            // byte[] salt = RandomNumberGenerator.GetBytes(32); // Generate a 256-bit salt
            string saltString = Convert.ToBase64String(salt);
            Console.WriteLine($"Salt: {saltString}");

            using (var sha256 = SHA256.Create())
            {
                byte[] saltedPassword = Encoding.UTF8.GetBytes(password + saltString);
                byte[] hashBytes = sha256.ComputeHash(saltedPassword);
                string hashString = Convert.ToBase64String(hashBytes);
                Console.WriteLine($"Generated Hash: {hashString}");
                return (hashString, saltString);
            }
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] saltedPassword = Encoding.UTF8.GetBytes(password + storedSalt);
                byte[] computedHash = sha256.ComputeHash(saltedPassword);
                string computedHashString = Convert.ToBase64String(computedHash);
                return computedHashString == storedHash;
            }
        }
    }
}
