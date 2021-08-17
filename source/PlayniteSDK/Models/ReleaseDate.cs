using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public struct ReleaseDate : IComparable, IComparable<ReleaseDate>, IEquatable<ReleaseDate>, ISerializable
    {
        private static readonly char[] serSplitter = new char[] { '-' };
        private readonly DateTime date;

        public static readonly ReleaseDate Empty = new ReleaseDate(0);

        public int? Day { get; private set; }
        public int? Month { get; private set; }
        public int Year { get; private set; }

        public ReleaseDate(int year)
        {
            if (year == default(int))
            {
                Year = default(int);
                Month = default(int?);
                Day = default(int?);
                date = default(DateTime);
            }
            else
            {
                Year = year;
                Day = null;
                Month = null;
                date = new DateTime(year, 1, 1);
            }
        }

        public ReleaseDate(int year, int month)
        {
            Year = year;
            Month = month;
            Day = null;
            date = new DateTime(year, month, 1);
        }

        public ReleaseDate(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
            date = new DateTime(year, month, day);
        }

        public ReleaseDate(DateTime dateTime) : this(dateTime.Year, dateTime.Month, dateTime.Day)
        {
        }

        public ReleaseDate(SerializationInfo info, StreamingContext context)
        {
            var serDate = Deserialize(info.GetString(nameof(ReleaseDate)));
            Year = serDate.Year;
            Month = serDate.Month;
            Day = serDate.Day;
            if (Year == default(int))
            {
                date = default(DateTime);
            }
            else
            {
                date = new DateTime(Year, Month ?? 1, Day ?? 1);
            }
        }

        public int CompareTo(object obj)
        {
            if (obj is ReleaseDate date)
            {
                return CompareTo(date);
            }
            else
            {
                return 1;
            }
        }

        public int CompareTo(ReleaseDate other)
        {
            return date.CompareTo(other.date);
        }

        public override bool Equals(object obj)
        {
            if (obj is ReleaseDate date)
            {
                return Equals(date);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(ReleaseDate other)
        {
            return Day == other.Day &&
                Month == other.Month &&
                Year == other.Year;
        }

        public static bool operator ==(ReleaseDate obj1, ReleaseDate obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(ReleaseDate obj1, ReleaseDate obj2)
        {
            return !obj1.Equals(obj2);
        }

        public override int GetHashCode()
        {
            return Year.GetHashCode() ^
                (Month ?? 0).GetHashCode() ^
                (Day ?? 0).GetHashCode();
        }

        public string Serialize()
        {
            if (Day != null)
            {
                return $"{Year}-{Month}-{Day}";
            }
            else if (Month != null)
            {
                return $"{Year}-{Month}";
            }
            else
            {
                return Year.ToString();
            }
        }

        public static bool TryDeserialize(string stringDate, out ReleaseDate date)
        {
            if (string.IsNullOrEmpty(stringDate))
            {
                date = Empty;
                return false;
            }

            var split = stringDate.Split(serSplitter);
            if (split.Length == 3)
            {
                if (DateTime.TryParseExact(stringDate, "yyyy-M-d", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    date = new ReleaseDate(parsedDate.Year, parsedDate.Month, parsedDate.Day);
                    return true;
                }
            }
            else if (split.Length == 2)
            {
                if (DateTime.TryParseExact(stringDate, "yyyy-M", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    date = new ReleaseDate(parsedDate.Year, parsedDate.Month);
                    return true;
                }
            }
            else if (split.Length == 1)
            {
                if (DateTime.TryParseExact(stringDate, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    date = new ReleaseDate(parsedDate.Year);
                    return true;
                }
            }

            date = Empty;
            return false;
        }

        public static ReleaseDate Deserialize(string stringDate)
        {
            if (string.IsNullOrEmpty(stringDate))
            {
                throw new ArgumentNullException(nameof(stringDate));
            }

            var split = stringDate.Split(serSplitter);
            if (split.Length == 3)
            {
                return new ReleaseDate(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]));
            }
            else if (split.Length == 2)
            {
                return new ReleaseDate(int.Parse(split[0]), int.Parse(split[1]));
            }
            else if (split.Length == 1)
            {
                return new ReleaseDate(int.Parse(split[0]));
            }
            else
            {
                throw new NotSupportedException("Uknown ReleaseDate string format.");
            }
        }

        public override string ToString()
        {
            if (Day != null)
            {
                return date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
            }
            else if (Month != null)
            {
                return date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.YearMonthPattern);
            }
            else
            {
                return Year.ToString();
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ReleaseDate), Serialize());
        }
    }
}
