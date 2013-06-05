using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KinopoiskLib.Properties;

namespace KinopoiskLib
{
    public class KinopoiskAPI
    {
        public static KinopoiskFilm Search(string filename)
        {
            var kinopoiskId = FindKinopoiskId(filename);
            return kinopoiskId != 0L ? Search(kinopoiskId) : null;
        }

        public static KinopoiskFilm Search(long kinopoiskId)
        {
            string detailsPage = null;
            string castPage = null;
            string postersPage = null;

            Parallel.Invoke(
                () => detailsPage = DownloadHtml("http://www.kinopoisk.ru/film/" + kinopoiskId),
                () => castPage = DownloadHtml(string.Format("http://www.kinopoisk.ru/film/{0}/cast/", kinopoiskId)),
                () => postersPage = DownloadHtml(string.Format("http://www.kinopoisk.ru/film/{0}/posters/", kinopoiskId))
            );

            return new KinopoiskFilm(kinopoiskId, detailsPage, castPage, postersPage);
        }

        private static string DownloadHtml(string url)
        {
            var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows; U; Windows NT 5.1; ru; rv:1.9.2.3) Gecko/20100401 Firefox/3.6.3 (.NET CLR 3.5.30729)");
            webClient.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            webClient.Headers.Add("Accept-Language", "ru,en-us;q=0.7,en;q=0.3");
            webClient.Headers.Add("Accept-Charset", "windows-1251,utf-8;q=0.7,*;q=0.7");
            webClient.Headers.Add("Keep-Alive", "115");
            return Encoding.GetEncoding(1251).GetString(webClient.DownloadData(url));
        }

        private static long FindKinopoiskId(string fileName)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(string.Format(Settings.Default.KinopoiskSearchUrlPattern, fileName));
            httpWebRequest.AllowAutoRedirect = false;
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
            var response = httpWebRequest.GetResponse();
            if (response.Headers["Location"] != null)
            {
                var match = Regex.Match(response.Headers["Location"], "/film/(?<KinopoiskId>[0-9]+)/");
                if (match != Match.Empty)
                {
                    return Convert.ToInt64(match.Groups["KinopoiskId"].Value);
                }
            }

            return 0L;
        }
    }
}