using demo_api_rest.Data;
using demo_api_rest.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace demo_api_rest.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> IsEmailRegisteredAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<RegisterResponse> RegisterUserAsync(RegisterRequest request)
        {
            try
            {
                // Vérifier si l'email existe déjà
                if (await IsEmailRegisteredAsync(request.Email))
                {
                    return new RegisterResponse 
                    { 
                        Success = false, 
                        Message = "Cet email est déjà utilisé." 
                    };
                }

                // Hashage du mot de passe
                string hashedPassword = HashPassword(request.Password);

                // Créer le nouvel utilisateur
                var user = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Password = hashedPassword,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    City = request.City,
                    PostalCode = request.PostalCode,
                    Country = request.Country,
                    Company = request.Company,
                    JobTitle = request.JobTitle,
                    DateOfBirth = request.DateOfBirth,
                    RegistrationDate = DateTime.Now,
                    AcceptTerms = request.AcceptTerms,
                    ProfilePictureUrl = request.ProfilePictureUrl,
                    IsActive = true
                };

                // Sauvegarder l'utilisateur dans la base de données
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                return new RegisterResponse
                {
                    Success = true,
                    Message = "Inscription réussie.",
                    UserId = user.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'inscription de l'utilisateur");
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Une erreur s'est produite lors de l'inscription. Veuillez réessayer ultérieurement."
                };
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                // ComputeHash - retourne un tableau d'octets
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convertir le tableau d'octets en chaîne
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}