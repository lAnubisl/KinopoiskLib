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
        private readonly long kinopoiskId;
        private readonly string russianTitle,
                                originalTitle,
                                description,
                                mpaa;

        private readonly int durationMinutes, ageLimit;

        private readonly IList<KinopoiskPerson> persons;
        private readonly IList<KinopoiskPoster> posters;
        private readonly IList<string> genres;
        private readonly DateTime releaseDate;
        private readonly IList<long> relatedFilms; 

        internal KinopoiskFilm(long kinopoiskId, string kinopoiskHtmlPage, string castsPage, string postersPage, string relatedMoviesPage)
        {
            if (string.IsNullOrEmpty(kinopoiskHtmlPage))
            {
                return;
            }

            this.kinopoiskId = kinopoiskId;
            kinopoiskHtmlPage = kinopoiskHtmlPage.Replace("<br />", "").Replace("<br>", "");

            this.russianTitle = GetValue(kinopoiskHtmlPage, Settings.Default.RussianTitlePattern, "RussianTitle");
            this.originalTitle = GetValue(kinopoiskHtmlPage, Settings.Default.OriginalTitlePattern, "OriginalTitle");
            this.description = GetValue(kinopoiskHtmlPage, Settings.Default.DescriptionPattern, "Description");

            var date = GetValue(kinopoiskHtmlPage, Settings.Default.ReleaseDateRusPattern, "ReleaseDate");
            if (date != null)
            {
                this.releaseDate = DateTime.ParseExact(date, "d MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU"));
            }

            date = GetValue(kinopoiskHtmlPage, Settings.Default.ReleaseDatePattern, "ReleaseDate");
            if (date != null)
            {
                this.releaseDate = DateTime.ParseExact(date, "d MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU")); ;
            }

            this.genres = new List<string>();
            foreach (Match genreMatch in Regex.Matches(kinopoiskHtmlPage, Settings.Default.GenrePattern, RegexOptions.Compiled | RegexOptions.Singleline))
            {
                this.genres.Add(genreMatch.Groups["Genre"].Value);
            }

            this.posters = new List<KinopoiskPoster>();
            foreach (Match match in Regex.Matches(postersPage, Settings.Default.PosterUrlPattern, RegexOptions.Compiled | RegexOptions.Singleline))
            {
                this.posters.Add(new KinopoiskPoster(Convert.ToInt64(match.Groups["PosterId"].Value)));
            }

            var departments = Regex.Matches(castsPage, Settings.Default.DepartmentPattern)
                .Cast<Match>()
                .ToDictionary(m => MakeDepartmentSingular(m.Groups["Department"].Value), departmentMatch => departmentMatch.Index);


            this.persons = new List<KinopoiskPerson>();
            foreach (Match match in Regex.Matches(castsPage, Settings.Default.PersonPattern, RegexOptions.Compiled | RegexOptions.Singleline))
            {
                this.persons.Add(new KinopoiskPerson(match, FindDepartment(departments, match.Index)));
            }

            this.relatedFilms = new List<long>();
            foreach (Match match in Regex.Matches(relatedMoviesPage, Settings.Default.RelatedMoviesPattern))
            {
                this.relatedFilms.Add(Convert.ToInt64(match.Groups["KinopoiskId"].Value));
            }

            this.durationMinutes = Convert.ToInt32(GetValue(kinopoiskHtmlPage, Settings.Default.DurationPattern, "Duration"));
            this.ageLimit = Convert.ToInt32(GetValue(kinopoiskHtmlPage, Settings.Default.AgeLimitPattern, "AgeLimit"));
            this.mpaa = GetValue(kinopoiskHtmlPage, Settings.Default.MPAAPattern, "Rating");
        }

        public IList<long> RelatedFilms
        {
            get { return this.relatedFilms; }
        } 

        public long KinopoiskId
        {
            get { return this.kinopoiskId; }
        }

        public string RussianTitle
        {
            get { return this.russianTitle; }
        }

        public string OriginalTitle
        {
            get { return this.originalTitle; }
        }

        public string Description
        {
            get { return this.description; }
        }

        public IList<string> Genres
        {
            get { return this.genres; }
        }

        public DateTime ReleaseDate
        {
            get { return this.releaseDate; }
        }

        public IList<KinopoiskPoster> Posters
        {
            get { return this.posters; }
        }

        public IList<KinopoiskPerson> Persons
        {
            get { return this.persons; }
        } 

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
            return string.Format("KinopoiskFilm_{0}", kinopoiskId).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var objT = obj as KinopoiskFilm;
            if (objT == null)
            {
                return false;
            }

            return this.kinopoiskId == objT.kinopoiskId;
        }

        public override string ToString()
        {
            return string.Format("{0} / {1} ({2})", originalTitle, russianTitle, releaseDate.Year);
        }
    }
}