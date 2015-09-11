using System;
using System.Text.RegularExpressions;
using System.Web;
using KinopoiskLib.Properties;

namespace KinopoiskLib
{
    public class KinopoiskPerson
    {
        internal KinopoiskPerson(Match match, string department)
        {
            KinopoiskId = Convert.ToInt64(match.Groups["PersonId"].Value);
            Department = department;
            RussianName = HttpUtility.HtmlDecode(match.Groups["RussianName"].Value).Trim();
            EnglishName = HttpUtility.HtmlDecode(match.Groups["EnglishName"].Value).Trim();
            Role = HttpUtility.HtmlDecode(match.Groups["Role"].Value).Trim();
            if (string.IsNullOrWhiteSpace(Role))
            {
                Role = null;
            }
        }

        public string EnglishName { get; }

        public string RussianName { get; }

        public string Department { get; }

        public string Role { get; }

        public string PhotoURL => string.Format(Settings.Default.PhotoUrlPattern, KinopoiskId);

        public long KinopoiskId { get; }

        public override string ToString()
        {
            return $"{this.EnglishName}/{this.RussianName}";
        }
    }
}