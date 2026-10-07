using Microsoft.AspNetCore.Mvc;
using MangaViews;
using MangaModels;
using MangaForms;
using TaskOption;
using Library.Service.TaskServices;
using Manga.Services;
using Library.Service.BackGroundService;
using CronModels;
using Cron.Services;
using MangaMiciomaniaModels;
using CanzoniModels;
using Microsoft.AspNetCore.Authorization;

namespace Manga.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MangaController
    {
        private readonly MangaService _mangaService;
        private readonly AppTaskService _tasks;
        private readonly BackGroundService _backgroundService;

        public MangaController(MangaService mangaService, AppTaskService tasks, BackGroundService backgroundService)
        {
            _mangaService = mangaService;
            _tasks = tasks;
            _backgroundService = backgroundService;
        }

        [AllowAnonymous]
        [HttpGet("get_all_manga")]
        public async Task<ActionResult<MangaEPreferiti>> GetAllMangaEPreferiti()
        {
            return await _tasks.MultiTask(new MultiTaskOptions<List<MangaClass>, List<MangaMiciomania>, MangaEPreferiti>
            {
                Task1 = _mangaService.GetAllMangaCache,
                Task2 = _mangaService.GetAllMangaMiciomaniaCache,
                ResultFactory = (manga, miciomanga) => new MangaEPreferiti(manga, miciomanga),
                ErrorMessage = "Errore nel recupero Manga e Preferiti"
            });
        }

        [AllowAnonymous]
        [HttpGet("get_all_canzoni")]
        public async Task<ActionResult<List<CanzoniMiciomania>>> GetAllcanzoni()
        {
            return await _tasks.SingleTask(new SingleTaskOptions<List<CanzoniMiciomania>>
            {
                Task = _mangaService.GetAllCanzoniMiciomaniaCache,
                ErrorMessage = "Errore nel recupero della canzoni"
            });
        }

        // [Authorize]
        [AllowAnonymous]
        [HttpGet("get_manga_utente/{idUtente}")]
        public async Task<ActionResult<MangaUtenteGet>> GetMangaUtente(string idUtente)
        {
            return await _tasks.SingleTask(new SingleTaskOptions<MangaUtenteGet>
            {
                Task = () => _mangaService.GetMangaUtente(idUtente),
                ErrorMessage = "Errore nel recupero della canzoni"
            });
        }

        // [Authorize]
        [AllowAnonymous]
        [HttpPost("post_manga")]
        public async Task<ActionResult> PostManga([FromBody] MangaClass mangaForm)
        {
            ActionResult result = await _tasks.SqlFunc(new SqlTaskOptions
            {
                Sql = () => _mangaService.AggiungiManga(mangaForm),
                SuccessMessage = "Manga aggiunto con successo",
                ErrorMessage = "Errore inserimento manga"
            });

            if (result is OkObjectResult)
            {
                _backgroundService.FireAndForget(async sp =>
                {
                    CronService cronService = sp.GetRequiredService<CronService>();

                    await cronService.PostUtentiCron(
                        idUtente: "indykun",
                        azione: $"Ha aggiunto un nuovo manga",
                        sezione: SezioneCron.Manga
                    );
                });
            }

            return result;
        }

        // [Authorize]
        [AllowAnonymous]
        [HttpPut("upsert_manga_preferiti/{idUtente}")]
        public async Task<ActionResult> UpdateMangaPreferiti(string idUtente, [FromBody] MangaUtenteForm nuoviManga)
        {
            ActionResult result = await _tasks.SqlFunc(new SqlTaskOptions
            {
                Sql = () => _mangaService.UpsertManga(idUtente, nuoviManga),
                SuccessMessage = $"Manga utente aggiornati per {idUtente}",
                ErrorMessage = "Errore aggiornamento manga utente"
            });

            if (result is OkObjectResult)
            {
                _backgroundService.FireAndForget(async sp =>
                {
                    CronService cronService = sp.GetRequiredService<CronService>();

                    await cronService.PostUtentiCron(
                        idUtente: idUtente,
                        azione: $"Ha aggiornato i suoi manga preferiti",
                        sezione: SezioneCron.Manga
                    );
                });
            }

            return result;
        }
    }
}
