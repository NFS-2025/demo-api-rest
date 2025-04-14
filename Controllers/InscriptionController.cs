using Microsoft.AspNetCore.Mvc;
using demo_api_rest.Models;
using demo_api_rest.Services;

namespace demo_api_rest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InscriptionController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<InscriptionController> _logger;

        public InscriptionController(IUserService userService, ILogger<InscriptionController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint pour l'inscription d'un nouvel utilisateur
        /// </summary>
        /// <param name="request">Données du formulaire d'inscription</param>
        /// <returns>Réponse de l'inscription</returns>
        [HttpPost]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Demande d'inscription invalide. Des erreurs de validation ont été détectées.");
                    return BadRequest(new RegisterResponse
                    {
                        Success = false,
                        Message = "Les données d'inscription sont invalides."
                    });
                }

                // Vérification de l'âge minimum (18 ans)
                if (DateTime.Now.AddYears(-18) < request.DateOfBirth)
                {
                    return BadRequest(new RegisterResponse
                    {
                        Success = false,
                        Message = "Vous devez avoir au moins 18 ans pour vous inscrire."
                    });
                }

                var response = await _userService.RegisterUserAsync(request);

                if (response.Success)
                {
                    _logger.LogInformation($"Nouvel utilisateur inscrit avec l'ID: {response.UserId}");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"Échec de l'inscription: {response.Message}");
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur s'est produite lors du traitement de l'inscription.");
                return StatusCode(500, new RegisterResponse
                {
                    Success = false,
                    Message = "Une erreur interne s'est produite lors du traitement de la demande."
                });
            }
        }

        /// <summary>
        /// Endpoint pour vérifier si un email est déjà utilisé
        /// </summary>
        /// <param name="email">L'adresse email à vérifier</param>
        /// <returns>True si l'email est déjà utilisé, false sinon</returns>
        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { success = false, message = "L'email ne peut pas être vide." });
            }

            bool isRegistered = await _userService.IsEmailRegisteredAsync(email);
            return Ok(new { isRegistered });
        }
    }
}