
using System.Collections.Concurrent;
using System.Threading.Tasks;
using KinopoiskLib;

namespace ConsoleApplication1
{
    class Program
    {
        private static readonly ConcurrentDictionary<long, KinopoiskFilm> bag = new ConcurrentDictionary<long, KinopoiskFilm>();

        static void Main(string[] args)
        {
            ExtractGraph("Убийцы вампирш-лесбиянок");
        }

        private static void ExtractGraph(string entryMovieName)
        {
            var entry = KinopoiskAPI.Search(entryMovieName);
            //// Parallel.ForEach(entry.RelatedFilms, ExtractGraph);
        }

        private static void ExtractGraph(long kinopoiskId)
        {
            if (bag.ContainsKey(kinopoiskId))
            {
                return;
            }

            bag.TryAdd(kinopoiskId, null);

            var entry = KinopoiskAPI.Search(kinopoiskId);
            bag[kinopoiskId] = entry;
            Parallel.ForEach(entry.RelatedFilms, ExtractGraph);
        }
    }
}
