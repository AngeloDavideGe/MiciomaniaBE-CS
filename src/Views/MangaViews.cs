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
        public string? manga { get; set; }
        public string? mangamicio { get; set; }
        public string? canzonimicio { get; set; }

        public MangaUtenteGet(string? m, string? mm, string? cm)
        {
            manga = m;
            mangamicio = mm;
            canzonimicio = cm;
        }
    }
}
