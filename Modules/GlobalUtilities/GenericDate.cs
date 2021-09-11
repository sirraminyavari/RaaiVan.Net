using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaiVan.Modules.GlobalUtilities
{
    public enum CalendarType
    {
        None,
        Gregorian,
        Jalali
    }

    public class GenericDate
    {
        private int? _Year;
        private int? _Month;
        private int? _Day;
        private int? _Hour;
        private int? _Minute;


        public CalendarType Type { get; }

        public int Year
        {
            get { return !_Year.HasValue ? 0 : _Year.Value; }
        }

        public int Month
        {
            get { return !_Month.HasValue ? 0 : _Month.Value; }
        }

        public int Day
        {
            get { return !_Day.HasValue ? 0 : _Day.Value; }
        }

        public int Hour
        {
            get { return !_Hour.HasValue ? 0 : _Hour.Value; }
        }

        public int Minute
        {
            get { return !_Minute.HasValue ? 0 : _Minute.Value; }
        }

        public GenericDate(CalendarType type, int year, int month, int day, int hour = 0, int minute = 0) {
            Type = type;
            _Year = year < 0 ? 0 : year;
            _Month = month < 0 ? 0 : month;
            _Day = day < 0 ? 0 : day;
            _Hour = hour < 0 || hour > 23 ? 0 : hour;
            _Minute = minute < 0 || minute > 59 ? 0 : minute;
        }

        private string datePartString(int value)
        {
            return (value < 10 ? "0" : "") + value.ToString();
        }

        private string monthString
        {
            get { return datePartString(Month); }
        }

        private string dayString
        {
            get { return datePartString(Day); }
        }

        private string hourString
        {
            get { return datePartString(Hour); }
        }

        private string minuteString
        {
            get { return datePartString(Minute); }
        }

        public string toString(string format)
        {
            if (string.IsNullOrEmpty(format)) format = Type == CalendarType.Jalali ? "yyyy-MM-dd" : "MM-dd-yyyy";

            return format
                .Replace("yyyy", Year.ToString())
                .Replace("MM", monthString)
                .Replace("dd", dayString)
                .Replace("hh", hourString)
                .Replace("mm", minuteString);
        }

        public string toString(bool detail, bool reverse, char delimiter = '/')
        {
            string timeFormat = "hh:mm";
            string dateFormat = reverse ? 
                (Type == CalendarType.Jalali ? "dd/MM/yyyy" : "MM/dd/yyyy") : 
                (Type == CalendarType.Jalali ? "yyyy/MM/dd" : "MM/dd/yyyy");

            dateFormat = dateFormat.Replace('/', delimiter);

            string format = !detail ? dateFormat :
                (Type == CalendarType.Jalali ? timeFormat + " " + dateFormat : dateFormat + " " + timeFormat); 

            return toString(format);
        }

        public static GenericDate fromDateTime(DateTime date, RVLang language)
        {
            return PublicMethods.uses_jalali_calendar(language) ? jalali(date) : gregorian(date);
        }

        public static GenericDate gregorian(DateTime date)
        {
            return new GenericDate(CalendarType.Gregorian, date.Year, date.Month, date.Day, date.Hour, date.Minute);
        }

        public static GenericDate jalali(DateTime date)
        {
            PersianCalendar PCalendar = new PersianCalendar();

            int day = PCalendar.GetDayOfMonth(date);
            int month = PCalendar.GetMonth(date);
            int year = PCalendar.GetYear(date);

            return new GenericDate(CalendarType.Jalali, year, month, day, date.Hour, date.Minute);
        }
    }
}
