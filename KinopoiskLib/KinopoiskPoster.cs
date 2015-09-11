namespace KinopoiskLib
{
    public class KinopoiskPoster
    {
        private readonly long posterId;

        internal KinopoiskPoster(long posterId)
        {
            this.posterId = posterId;
        }

        public string Url => $"http://tr-by.kinopoisk.ru/images/poster/{posterId}.jpg";

        public override string ToString()
        {
            return Url;
        }
    }
}