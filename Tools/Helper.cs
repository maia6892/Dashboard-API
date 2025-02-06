using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RestSharp;

namespace DashboardAPI.Tools
{
    public static class Helper
    {
        public static int GetPercentageDifference(decimal previousValue, decimal currentValue)
        {
            if (previousValue == 0)
            {
                return 100;
            };
            var res = Convert.ToInt32(Math.Truncate((currentValue - previousValue) / previousValue * 100));
            return res;
        }

        public static int GetPercentageDifference(int previousValue, int currentValue)
        {
            if (previousValue == 0)
            {
                return 100;
            };
            var res = (double)(currentValue - previousValue) / previousValue * 100;
            return (int)Math.Round(res);
        }

        public static int GetPercentage(decimal currentValue, decimal totalValue)
        {
            var res = Convert.ToInt32(Math.Round(currentValue * 100 / totalValue));
            return res;
        }
        public static int GetPercentage(int currentValue, int totalValue)
        {
            var res = currentValue * 100 / totalValue;
            return res;
        }

        public static Dictionary<string, DateTime> GetWeekDays(DateTime currentDate, bool includeWeekends)
        {
            var startOfCurrentWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek + (int)DayOfWeek.Monday);
            var startOfPreviousWeek = startOfCurrentWeek.AddDays(-7);
            var endOfPreviousWeek = startOfCurrentWeek.AddDays(-1);

            var week = new Dictionary<string, DateTime>
            {
                { "Monday", startOfPreviousWeek },
                { "Tuesday", startOfPreviousWeek.AddDays(1) },
                { "Wednesday", startOfPreviousWeek.AddDays(2) },
                { "Thursday", startOfPreviousWeek.AddDays(3) },
                { "Friday", startOfPreviousWeek.AddDays(4) }
            };

            if (includeWeekends)
            {
                week.Add("Saturday", startOfPreviousWeek.AddDays(5));
                week.Add("Sunday", endOfPreviousWeek);
            }

            return week;
        }

        public static Dictionary<string, DateTime> GetThisWeekDays(DateTime currentDate, bool includeWeekends)
        {
            var startOfCurrentWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek + (int)DayOfWeek.Monday);
            var endOfCurrentWeek = startOfCurrentWeek.AddDays(6);

            var week = new Dictionary<string, DateTime>
            {
                { "Monday", startOfCurrentWeek },
                { "Tuesday", startOfCurrentWeek.AddDays(1) },
                { "Wednesday", startOfCurrentWeek.AddDays(2) },
                { "Thursday", startOfCurrentWeek.AddDays(3) },
                { "Friday", startOfCurrentWeek.AddDays(4) }
            };

            if (includeWeekends)
            {
                week.Add("Saturday", endOfCurrentWeek.AddDays(-1));
                week.Add("Sunday", endOfCurrentWeek);
            }

            return week;
        }


        public static Dictionary<string, DateTime> GetWeekStartingToday(DateTime currentDate)
        {
            var week = new Dictionary<string, DateTime>
            {
                { currentDate.Date.ToString("MMM dd", new System.Globalization.CultureInfo("en-US")), currentDate },
                { currentDate.AddDays(1).ToString("MMM dd", new System.Globalization.CultureInfo("en-US")), currentDate.AddDays(1) },
                { currentDate.AddDays(2).ToString("MMM dd", new System.Globalization.CultureInfo("en-US")), currentDate.AddDays(2) },
                { currentDate.AddDays(3).ToString("MMM dd", new System.Globalization.CultureInfo("en-US")), currentDate.AddDays(3) },
                { currentDate.AddDays(4).ToString("MMM dd", new System.Globalization.CultureInfo("en-US")), currentDate.AddDays(4) }
            };

            return week;
        }


        public static Dictionary<string, DateTime> GetTwoWeekStartingToday(DateTime currentDate)
        {
            var weeks = new Dictionary<string, DateTime>();

            for (int i = 0; i < 14; i++)
            {
                var date = currentDate.AddDays(-i);
                weeks[date.ToString("dd", new System.Globalization.CultureInfo("en-US"))] = date;
            }

            return weeks;
        }


        public static Dictionary<string, DateTime> GetMonthsOfYear(DateTime currentDate)
        {
            var year = new Dictionary<string, DateTime>();

            for (int month = 1; month <= 12; month++)
            {
                var firstDayOfMonth = new DateTime(currentDate.Year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                var monthName = firstDayOfMonth.ToString("MMMM", CultureInfo.InvariantCulture);

                year.Add(monthName, firstDayOfMonth);
            }

            return year;
        }

        public static Dictionary<string, (DateTime StartDate, DateTime EndDate)> GetWeeksOfMonth(DateTime monthStart)
        {
            var weeks = new Dictionary<string, (DateTime, DateTime)>();
            var currentWeekStart = new DateTime(monthStart.Year, monthStart.Month, monthStart.Day, 0, 0, 0, DateTimeKind.Utc);

            int weekNumber = 1;

            while (currentWeekStart.Month == monthStart.Month)
            {
                var weekEnd = currentWeekStart.AddDays(6);
                if (weekEnd.Month != monthStart.Month)
                {
                    weekEnd = new DateTime(monthStart.Year, monthStart.Month, DateTime.DaysInMonth(monthStart.Year, monthStart.Month), 23, 59, 59, DateTimeKind.Utc);
                }

                weeks.Add($"Week {weekNumber}", (currentWeekStart, weekEnd));

                currentWeekStart = weekEnd.AddDays(1).Date;
                weekNumber++;
            }

            return weeks;
        }


        public static async Task<JsonDocument> GetJsonResponseAsync(string url, Dictionary<string, string> headers = null)
        {
            var options = new RestClientOptions(url);
            var client = new RestClient(options);
            var request = new RestRequest();

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.AddHeader(header.Key, header.Value);
                }
            }

            var response = await client.GetAsync(request);

            if (response == null || !response.IsSuccessful || string.IsNullOrEmpty(response.Content))
            {
                throw new Exception("Failed to fetch data from the API.");
            }

            return JsonDocument.Parse(response.Content);
        }

        public static Dictionary<string, DateTime> GetHoursOfToday(DateTime currentDate)
        {
            var hours = new Dictionary<string, DateTime>();

            for (int i = 0; i < 24; i++)
            {
                var hour = currentDate.Date.AddHours(i);
                hours[hour.ToString("HH:00", new System.Globalization.CultureInfo("en-US"))] = hour;
            }

            return hours;
        }

        public static Dictionary<string, (DateTime StartDate, DateTime EndDate)> GetYearsInDatabase(List<DateTime> dates)
        {
            var years = new Dictionary<string, (DateTime, DateTime)>();

            var uniqueYears = dates.Select(d => d.Year).Distinct().OrderBy(y => y);

            foreach (var year in uniqueYears)
            {
                DateTime startOfYear = new DateTime(year, 1, 1);
                DateTime endOfYear = new DateTime(year, 12, 31);

                string yearLabel = $"{year}";
                years[yearLabel] = (startOfYear, endOfYear);
            }

            return years;
        }


        public static int CalculateAge(DateTime birthDate, DateTime? referenceDate = null)
        {
            var today = referenceDate ?? DateTime.UtcNow;
            int age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }

        public static List<DateTime> GetAllDaysOfMonth(int year, int month)
        {
            var days = new List<DateTime>();
            int daysInMonth = DateTime.DaysInMonth(year, month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                days.Add(new DateTime(year, month, day));
            }

            return days;
        }

        public static DateTime GetStartOfWeek(DateTime selectedMonth, string week)
        {
            var firstDayOfMonth = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);

            if (!int.TryParse(week.Replace("Week ", ""), out int weekNumber) || weekNumber < 1 || weekNumber > 5)
                throw new ArgumentException("Invalid week format. Use 'Week 1', 'Week 2', etc.");

            int daysToMonday = ((int)DayOfWeek.Monday - (int)firstDayOfMonth.DayOfWeek + 7) % 7;
            var firstMonday = firstDayOfMonth.AddDays(daysToMonday);

            if (firstMonday > firstDayOfMonth && weekNumber == 1)
                return firstDayOfMonth;

            var startOfWeek = firstMonday.AddDays((weekNumber - 1) * 7);

            return startOfWeek.Month == selectedMonth.Month ? startOfWeek : firstMonday;
        }



    }
}