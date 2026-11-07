using MangaMiciomaniaModels;
using MangaModels;

namespace MangaViews
{
    public class MangaEPreferiti
    {
        public List<MangaClass> listaManga { get; set; }
        public List<MangaMiciomania> micioManga { get; set; }

        public MangaEPreferiti(List<MangaClass> lm, List<MangaMiciomania> mm)
        {
            listaManga = lm;
            micioManga = mm;
        }
    }

    public class MangaUtenteGet
    {
        public string? mangaPreferito { get; set; }
        public string? mangaMicioPreferito { get; set; }
        public string? canzonimicio { get; set; }
        public string? mangaLeggendo { get; set; }
        public string? mangaMicioLeggendo { get; set; }

        public MangaUtenteGet(string? m, string? mm, string? cm, string? ml, string? mml)
        {
            mangaPreferito = m;
            mangaMicioPreferito = mm;
            canzonimicio = cm;
            mangaLeggendo = ml;
            mangaMicioLeggendo = mml;
        }
    }
}
