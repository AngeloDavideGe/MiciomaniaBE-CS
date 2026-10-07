using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserViews;
using UserForms;
using TaskOption;
using Utenti.Services;
using Library.Service.TaskServices;
using Library.Service.BackGroundService;
using CronModels;
using Cron.Services;

namespace Utenti.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtentiController : ControllerBase
    {
        private readonly UtentiService _utentiService;
        private readonly BackGroundService _backgroundService;
        private readonly AppTaskService _task;
        private readonly TokenService _tokenService;

        public UtentiController(
            UtentiService utentiService,
            AppTaskService task,
            BackGroundService backgroundService,
            TokenService tokenService)
        {
            _utentiService = utentiService;
            _task = task;
            _backgroundService = backgroundService;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpGet("get_all_utenti")]
        public async Task<ActionResult<List<UserParams>>> GetAllUtenti()
        {
            return await _task.SingleTask(
                new SingleTaskOptions<List<UserParams>>
                {
                    Task = _utentiService.GetAllUtenti,
                    ErrorMessage = "Errore recupero utenti"
                }
            );
        }

        [AllowAnonymous]
        [HttpGet("get_utente_by_email")]
        public async Task<ActionResult<object>> GetUtenteByCredentials(
            [FromQuery] string email,
            [FromQuery] string password)
        {
            ActionResult<UserDto?> result =
                await _task.SingleTask(
                    new SingleTaskOptions<UserDto?>
                    {
                        Task = () => _utentiService.GetUtenteByCredentials(
                            email,
                            password
                        ),
                        ErrorMessage = "Utente non trovato"
                    }
                );

            if (
                result.Result is OkObjectResult okResult &&
                okResult.Value is UserDto user
            )
            {
                string token = _tokenService.GenerateToken(user);

                _backgroundService.FireAndForget(async sp =>
                {
                    CronService cronService =
                        sp.GetRequiredService<CronService>();

                    await cronService.PostUtentiCron(
                        idUtente: user.id,
                        azione: "Ha effettuato il Login",
                        sezione: SezioneCron.Profilo
                    );
                });

                return Ok(new
                {
                    user,
                    token
                });
            }

            return result.Result
                ?? StatusCode(
                    StatusCodes.Status500InternalServerError
                );
        }

        [AllowAnonymous]
        [HttpGet("get_utente_by_id/{id}")]
        public async Task<ActionResult<UserDto?>> GetUserById(
            string id)
        {
            return await _task.SingleTask(
                new SingleTaskOptions<UserDto?>
                {
                    Task = () => _utentiService.GetUtenteById(id),
                    ErrorMessage = "Utente non trovato"
                }
            );
        }

        [AllowAnonymous]
        [HttpPost("post_utente")]
        public async Task<ActionResult> PostUser(
            [FromBody] UserPostForm userForm)
        {
            return await _task.SqlFunc(new SqlTaskOptions
            {
                Sql = () => _utentiService.AggiungiUtente(userForm),
                SuccessMessage = "Utente aggiunto con successo",
                ErrorMessage = "Errore creazione utente"
            }
            );
        }

        // [Authorize]
        [AllowAnonymous]
        [HttpPut("update_utente")]
        public async Task<ActionResult> UpdateUser(
            [FromBody] UserDto userForm)
        {
            string? idUtente =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (string.IsNullOrEmpty(idUtente))
            {
                return Unauthorized();
            }

            return await _task.SqlFunc(
                new SqlTaskOptions
                {
                    Sql = () => _utentiService.AggiornaUtente(
                        idUtente,
                        userForm
                    ),
                    SuccessMessage = "Utente aggiornato con successo",
                    ErrorMessage = "Errore aggiornamento utente"
                }
            );
        }

        // [Authorize]
        [AllowAnonymous]
        [HttpGet("get_utente_corrente")]
        public async Task<ActionResult<UserDto?>>
            GetUtenteCorrente()
        {
            string? idUtente = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (string.IsNullOrEmpty(idUtente))
            {
                return Unauthorized();
            }

            return await _task.SingleTask(new SingleTaskOptions<UserDto?>
            {
                Task = () => _utentiService.GetUtenteById(
                    idUtente
                ),
                ErrorMessage = "Errore recupero utente"
            }
            );
        }

        // [Authorize]
        [AllowAnonymous]
        [HttpDelete("delete_utente/{id}")]
        public async Task<ActionResult> DeleteUser(
            string id)
        {
            return await _task.SqlFunc(
                new SqlTaskOptions
                {
                    Sql = () => _utentiService.EliminaUtente(id),
                    SuccessMessage = "Utente eliminato con successo",
                    ErrorMessage = "Errore eliminazione utente"
                }
            );
        }

        // [Authorize]
        [AllowAnonymous]
        [HttpPut("update_ruolo_admin/{idUtente}")]
        public async Task<ActionResult> UpdateRuoloAdmin(
            string idUtente,
            [FromBody] UserRuoloUpdateForm ruoloForm)
        {
            ActionResult result =
                await _task.SqlFunc(
                    new SqlTaskOptions
                    {
                        Sql = () => _utentiService.AggiornaRuoloAdmin(
                            idUtente,
                            ruoloForm
                        ),
                        SuccessMessage = "Ruolo aggiornato con successo",
                        ErrorMessage = "Errore aggiornamento ruolo"
                    }
                );

            if (result is OkObjectResult)
            {
                _backgroundService.FireAndForget(async sp =>
                {
                    CronService cronService =
                        sp.GetRequiredService<CronService>();

                    await cronService.PostUtentiCron(
                        idUtente: idUtente,
                        azione: $"Ha aggiornato il suo ruolo in {ruoloForm.ruolo}",
                        sezione: SezioneCron.Admin
                    );
                });
            }

            return result;
        }
    }
}