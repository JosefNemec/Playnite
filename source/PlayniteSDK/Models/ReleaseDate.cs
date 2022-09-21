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
    /// Represents game release date.
    /// </summary>
    [Serializable]
    public struct ReleaseDate : IComparable, IComparable<ReleaseDate>, IEquatable<ReleaseDate>, ISerializable
    {
        private static readonly char[] serSplitter = new char[] { '-' };
        /// <summary>
        /// Gets DateTime representation of release date.
        /// </summary>
        public readonly DateTime Date;

        /// <summary>
        /// Gets empty representation of release date.
        /// </summary>
        public static readonly ReleaseDate Empty = new ReleaseDate(0);
        /// <summary>
        /// Gets release day.
        /// </summary>
        public int? Day { get; private set; }
        /// <summary>
        /// Gets release month.
        /// </summary>
        public int? Month { get; private set; }
        /// <summary>
        /// Gets release year.
        /// </summary>
        public int Year { get; private set; }

        /// <summary>
        /// Creates new instance of <see cref="ReleaseDate"/>.
        /// </summary>
        /// <param name="year"></param>
        public ReleaseDate(int year)
        {
            if (year == default(int))
            {
                Year = default(int);
                Month = default(int?);
                Day = default(int?);
                Date = default(DateTime);
            }
            else
            {
                Year = year;
                Day = null;
                Month = null;
                Date = new DateTime(year, 1, 1);
            }
        }

        /// <summary>
        /// Creates new instance of <see cref="ReleaseDate"/>.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        public ReleaseDate(int year, int month)
        {
            Year = year;
            Month = month;
            Day = null;
            Date = new DateTime(year, month, 1);
        }

        /// <summary>
        /// Creates new instance of <see cref="ReleaseDate"/>.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        public ReleaseDate(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
            Date = new DateTime(year, month, day);
        }

        /// <summary>
        /// Creates new instance of <see cref="ReleaseDate"/>.
        /// </summary>
        /// <param name="dateTime"></param>
        public ReleaseDate(DateTime dateTime) : this(dateTime.Year, dateTime.Month, dateTime.Day)
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="ReleaseDate"/>.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public ReleaseDate(SerializationInfo info, StreamingContext context)
        {
            var serDate = Deserialize(info.GetString(nameof(ReleaseDate)));
            Year = serDate.Year;
            Month = serDate.Month;
            Day = serDate.Day;
            if (Year == default(int))
            {
                Date = default(DateTime);
            }
            else
            {
                Date = new DateTime(Year, Month ?? 1, Day ?? 1);
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public int CompareTo(ReleaseDate other)
        {
            return Date.CompareTo(other.Date);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public bool Equals(ReleaseDate other)
        {
            return Day == other.Day &&
                Month == other.Month &&
                Year == other.Year;
        }

        /// <inheritdoc/>
        public static bool operator ==(ReleaseDate obj1, ReleaseDate obj2)
        {
            return obj1.Equals(obj2);
        }

        /// <inheritdoc/>
        public static bool operator !=(ReleaseDate obj1, ReleaseDate obj2)
        {
            return !obj1.Equals(obj2);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Year.GetHashCode() ^
                (Month ?? 0).GetHashCode() ^
                (Day ?? 0).GetHashCode();
        }

        /// <summary>
        /// Gets release date serialized to a string.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Try to deserialize string to a release date.
        /// </summary>
        /// <param name="stringDate"></param>
        /// <param name="date"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Deserialize string to release date.
        /// </summary>
        /// <param name="stringDate"></param>
        /// <returns></returns>
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

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Day != null)
            {
                return Date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
            }
            else if (Month != null)
            {
                return Date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.YearMonthPattern);
            }
            else
            {
                return Year.ToString();
            }
        }

        /// <inheritdoc/>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ReleaseDate), Serialize());
        }
    }
}
