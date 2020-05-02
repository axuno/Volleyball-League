using System;

namespace TournamentManager
{
	public class DateTimePeriod
	{
		private DateTime? _start;
		private DateTime? _end;

        /// <summary>
        /// Creates a new <see cref="DateTimePeriod"/>
        /// When setting Start and End, it is ensured that always Start &lt;= End
        /// </summary>
		public DateTimePeriod()
		{}

        /// <summary>
        /// Creates a new <see cref="DateTimePeriod"/>.
        /// When setting Start and End, it is ensured that always Start &lt;= End
        /// </summary>
        /// <param name="start">Start <see cref="DateTime"/></param>
        /// <param name="end">End <see cref="DateTime"/></param>
		public DateTimePeriod(DateTime? start, DateTime? end)
		{
			Start = start;
			End = end;
		}

        /// <summary>
        /// Creates a new <see cref="DateTimePeriod"/>.
        /// When setting Start and End, it is ensured that always Start &lt;= End
        /// </summary>
        /// <param name="start">Start <see cref="DateTime"/></param>
        /// <param name="end">End <see cref="DateTime"/></param>
        public DateTimePeriod(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Creates a new <see cref="DateTimePeriod"/>
        /// When setting Start and End, it is ensured that always Start &lt;= End
        /// </summary>
        /// <param name="start">Start <see cref="DateTime"/></param>
        /// <param name="timeSpanToEnd">Timespan to end <see cref="TimeSpan"/></param>
        public DateTimePeriod(DateTime? start, TimeSpan? timeSpanToEnd)
        {
            Start = start;
            if (!(Start.HasValue && timeSpanToEnd.HasValue))
                End = null;
            else
                End = Start.Value.Add(timeSpanToEnd.Value);
        }

        /// <summary>
        /// Creates a new <see cref="DateTimePeriod"/>
        /// When setting Start and End, it is ensured that always Start &lt;= End
        /// </summary>
        /// <param name="start">Start <see cref="DateTime"/></param>
        /// <param name="timeSpanToEnd">Timespan to end <see cref="TimeSpan"/></param>
        public DateTimePeriod(DateTime start, TimeSpan timeSpanToEnd)
        {
            Start = start;
            End = Start.Value.Add(timeSpanToEnd);
        }

        /// <summary>
        /// Gets or sets the Start of the <see cref="DateTimePeriod"/>.
        /// When setting the property, the maximum time precision is seconds.
        /// When setting Start and End, it is ensured that always Start &lt;= End
        /// </summary>
		public DateTime? Start
		{
			get => _start;
            set
            {
                if (!value.HasValue)
                {
                    _start = null;
                }
                else
                {
                    var d = value.Value;
                    _start= new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);
                }

                SwapIfNeeded();
            }
		}

        /// <summary>
        /// Gets or sets the End of the <see cref="DateTimePeriod"/>.
        /// When setting the property, the maximum time precision is seconds.
        /// When setting Start and End, it is ensured that always Start &lt;= End
        /// </summary>
        public DateTime? End
		{
			get => _end;
            set
            {
                if (!value.HasValue)
                {
                    _end = null;
                }
                else
                {
                    var d = value.Value;
                    _end = new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);
                }

                SwapIfNeeded();
            }
        }

        private void SwapIfNeeded()
        {
            // Swap start and end by using a tuple
            if ((_start ?? DateTime.MinValue) > (_end ?? DateTime.MaxValue)) (_start, _end) = (_end, _start);
        }

        /// <summary>
        /// Checks whether the <see cref="DateTimePeriod"/> contains a <see cref="DateTime"/>
        /// If one of the date values is null, false is returned.
        /// </summary>
        /// <param name="testDateTime"></param>
        /// <returns>Returns true, if the <see cref="DateTimePeriod"/> contains the <see cref="DateTime"/>. If one of the date values is null, false is returned.</returns>
        public bool Contains(DateTime? testDateTime)
        {
            if (!(_start.HasValue && _end.HasValue && testDateTime.HasValue)) return false;
            if (testDateTime >= _start && testDateTime <= _end) return true;
            return false;
        }

        /// <summary>
        /// Checks whether the <see cref="DateTimePeriod"/> contains a <see cref="DateTime"/>
        /// If one of the date values is null, false is returned.
        /// </summary>
        /// <param name="testDateTime"></param>
        /// <returns>Returns true, if the <see cref="DateTimePeriod"/> contains the <see cref="DateTime"/>. If one of the date values is null, false is returned.</returns>
        public bool Contains(DateTime testDateTime)
        {
            if (!(_start.HasValue && _end.HasValue)) return false;
            if (testDateTime >= _start && testDateTime <= _end) return true;
            return false;
        }

        /// <summary>
        /// Checks whether the <see cref="DateTimePeriod"/> overlaps with another <see cref="DateTimePeriod"/>.
        /// If one of the start or end values is null, false is returned.
        /// </summary>
        /// <param name="testPeriod"></param>
        /// <returns>Returns true, if the <see cref="DateTimePeriod"/> overlaps with another <see cref="DateTimePeriod"/>. If one of the start or end values is null, false is returned.</returns>
        public bool Overlaps(DateTimePeriod testPeriod)
        {
            if (!(_start.HasValue && _end.HasValue && testPeriod.Start.HasValue && testPeriod.End.HasValue)) return false;
            // https://stackoverflow.com/questions/325933/determine-whether-two-date-ranges-overlap
            return _start <= testPeriod.End && testPeriod.Start <= _end;
        }

        /// <summary>
        /// Gets the duration as the <see cref="TimeSpan"/> of End minus Date.
        /// Returns null, if Start and/or End are null.
        /// </summary>
        /// <returns>Gets the duration as the <see cref="TimeSpan"/> of End minus Date. Returns null, if Start and/or End are null.</returns>
        public TimeSpan? Duration(bool nullable)
        {
            if (!(Start.HasValue && End.HasValue && nullable)) return null;

            return Duration();
        }

        /// <summary>
        /// Gets the duration as the <see cref="TimeSpan"/> of End minus Date.
        /// </summary>
        /// <returns>Gets the duration as the <see cref="TimeSpan"/> of End minus Date.</returns>
        /// <exception cref="NullReferenceException">Throws an exception if Start or End date are null.</exception>
        public TimeSpan Duration()
        {
            return End.Value - Start.Value;
        }
    }
}