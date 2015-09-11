using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using KinopoiskLib.Properties;

namespace KinopoiskLib
{
    public class KinopoiskFilm
    {
        internal KinopoiskFilm(long kinopoiskId, string kinopoiskHtmlPage, string castsPage, string postersPage, string relatedMoviesPage)
        {
            if (string.IsNullOrEmpty(kinopoiskHtmlPage))
            {
                return;
            }

            this.KinopoiskId = kinopoiskId;
            kinopoiskHtmlPage = kinopoiskHtmlPage.Replace("<br />", "").Replace("<br>", "");

            this.RussianTitle = GetValue(kinopoiskHtmlPage, Settings.Default.RussianTitlePattern, "RussianTitle");
            this.OriginalTitle = GetValue(kinopoiskHtmlPage, Settings.Default.OriginalTitlePattern, "OriginalTitle");
            this.Description = GetValue(kinopoiskHtmlPage, Settings.Default.DescriptionPattern, "Description");

            var date = GetValue(kinopoiskHtmlPage, Settings.Default.ReleaseDateRusPattern, "ReleaseDate");
            if (date != null)
            {
                this.ReleaseDate = DateTime.ParseExact(date, "d MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU"));
            }

            date = GetValue(kinopoiskHtmlPage, Settings.Default.ReleaseDatePattern, "ReleaseDate");
            if (date != null)
            {
                this.ReleaseDate = DateTime.ParseExact(date, "d MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU")); ;
            }

            Genres = Regex.Matches(kinopoiskHtmlPage, Settings.Default.GenrePattern,
                RegexOptions.Compiled | RegexOptions.Singleline)
                .Cast<Match>()
                .Select(m => m.Groups["Genre"].Value);

            Posters = Regex.Matches(postersPage, Settings.Default.PosterUrlPattern, RegexOptions.Compiled | RegexOptions.Singleline)
                .Cast<Match>()
                .Select(m => new KinopoiskPoster(Convert.ToInt64(m.Groups["PosterId"].Value)));       

            var departments = Regex.Matches(castsPage, Settings.Default.DepartmentPattern)
                .Cast<Match>()
                .ToDictionary(m => MakeDepartmentSingular(m.Groups["Department"].Value), departmentMatch => departmentMatch.Index);

            Persons = Regex.Matches(castsPage, Settings.Default.PersonPattern, RegexOptions.Compiled)
                .Cast<Match>()
                .Select(m => new KinopoiskPerson(m, FindDepartment(departments, m.Index)));

            RelatedFilms = Regex.Matches(relatedMoviesPage, Settings.Default.RelatedMoviesPattern)
                .Cast<Match>()
                .Select(m => Convert.ToInt64(m.Groups["KinopoiskId"].Value));

            this.Runtime = Convert.ToInt32(GetValue(kinopoiskHtmlPage, Settings.Default.DurationPattern, "Duration"));
            this.AgeLimit = Convert.ToInt32(GetValue(kinopoiskHtmlPage, Settings.Default.AgeLimitPattern, "AgeLimit"));
            this.Mpaa = GetValue(kinopoiskHtmlPage, Settings.Default.MPAAPattern, "Rating");
        }

        public IEnumerable<long> RelatedFilms { get; }

        public long KinopoiskId { get; }

        public string RussianTitle { get; }

        public string OriginalTitle { get; }

        public string Description { get; }

        public IEnumerable<string> Genres { get; }

        public DateTime ReleaseDate { get; }

        public IEnumerable<KinopoiskPoster> Posters { get; }

        public IEnumerable<KinopoiskPerson> Persons { get; }

        public string Mpaa { get; }

        public int Runtime { get; }

        public int AgeLimit { get; }

        private static string MakeDepartmentSingular(string department)
        {
            return department.Replace("ы", "");
        }

        private static string FindDepartment(IEnumerable<KeyValuePair<string, int>> departments, int index)
        {
            string res = null;
            foreach (var department in departments)
            {
                if (index < department.Value)
                {
                    return res;
                }
                
                res = department.Key;
            }

            return res;
        }

        private static string GetValue(string kinopoiskHtmlPage, string regexPattern, string regexGroup)
        {
            var match = Regex.Match(kinopoiskHtmlPage, regexPattern, RegexOptions.Compiled | RegexOptions.Singleline);
            if (match != Match.Empty)
            {
                return HttpUtility.HtmlDecode(match.Groups[regexGroup].Value);
            }

            return null;
        }

        public override int GetHashCode()
        {
            return $"KinopoiskFilm_{KinopoiskId}".GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return KinopoiskId == (obj as KinopoiskFilm)?.KinopoiskId;
        }

        public override string ToString()
        {
            return $"{OriginalTitle} / {RussianTitle} ({ReleaseDate.Year})";
        }
    }
}