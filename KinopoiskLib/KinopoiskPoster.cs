namespace KinopoiskLib
{
    public class KinopoiskPoster
    {
        private readonly long posterId;

        internal KinopoiskPoster(long posterId)
        {
            this.posterId = posterId;
        }

        public string Url
        {
            get { return string.Format("http://tr-by.kinopoisk.ru/images/poster/{0}.jpg", this.posterId); }
        }

        public override string ToString()
        {
            return this.Url;
        }
    }
}