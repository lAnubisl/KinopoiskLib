using System;
using System.Text.RegularExpressions;
using System.Web;
using KinopoiskLib.Properties;

namespace KinopoiskLib
{
    public class KinopoiskPerson
    {
        private readonly long kinopoiskId;
        private readonly string englishName, russianName, department;

        internal KinopoiskPerson(Match match, string department)
        {
            this.kinopoiskId = Convert.ToInt64(match.Groups["PersonId"].Value);
            this.department = department;
            this.russianName = HttpUtility.HtmlDecode(match.Groups["RussianName"].Value).Trim();
            this.englishName = HttpUtility.HtmlDecode(match.Groups["EnglishName"].Value).Trim();
        }

        public string EnglishName
        {
            get { return this.englishName; }
        }

        public string RussianName
        {
            get { return this.russianName; }
        }

        public string Department
        {
            get { return this.department; }
        }

        public string PhotoURL
        {
            get { return string.Format(Settings.Default.PhotoUrlPattern, this.kinopoiskId); }
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}", this.englishName, this.russianName);
        }
    }
}