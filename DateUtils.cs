/*
*  Copyright (c) 2015 Blake McBride (blake@mcbride.name)
*  All rights reserved.
*
*  Permission is hereby granted, free of charge, to any person obtaining
*  a copy of this software and associated documentation files (the
*  "Software"), to deal in the Software without restriction, including
*  without limitation the rights to use, copy, modify, merge, publish,
*  distribute, sublicense, and/or sell copies of the Software, and to
*  permit persons to whom the Software is furnished to do so, subject to
*  the following conditions:
*
*  1. Redistributions of source code must retain the above copyright
*  notice, this list of conditions, and the following disclaimer.
*
*  2. Redistributions in binary form must reproduce the above copyright
*  notice, this list of conditions and the following disclaimer in the
*  documentation and/or other materials provided with the distribution.
*
*  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
*  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
*  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
*  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
*  HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
*  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
*  LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
*  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
*  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
*  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
*  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Globalization;
using System.Text.RegularExpressions;

// Author: Blake McBride


namespace CSUtils {

	/// <summary>
	/// Date utils.  Handles dates represented as an int in the form YYYYMMDD
	/// </summary>
	public static class DateUtils {

		private static readonly Regex DATE_FORMAT_MM_DD_YY = new Regex(@"(0?[1-9]|1[012])/(0?[1-9]|[12][0-9]|3[01])/\d\d");
		private static readonly Regex DATE_FORMAT_MM_DD_YYYY = new Regex(@"(0?[1-9]|1[012])/(0?[1-9]|[12][0-9]|3[01])/\d\d\d\d");

		
		public static int Today() {
			DateTime localDate = DateTime.Now;
			return localDate.Year * 10000 + localDate.Month * 100 + localDate.Day;
		}

		public static int Date(DateTime dt) {
			return dt.Year * 10000 + dt.Month * 100 + dt.Day;
		}

		public static DateTime Date(int dt) {
			return new DateTime(Year(dt), Month(dt), Day(dt));
		}

		public static int Date(int y, int m, int d) {
			y = GuessYear(y);
			return y * 10000 + m * 100 + d;
		}

		public static int Year(int dt) {
			return dt / 10000;
		}

		public static int Month(int dt) {
			return (dt % 10000) / 100;
		}

		public static int Day(int dt) {
			return dt % 100;
		}

		private static int GuessYear(int y) {
			if (y >= 100)
				return y;
			int currentYear = Year(Today());
			if (y + 2000 > currentYear + 10)
				return 1900 + y;
			else
				return 2000 + y;
		}

        /// <summary>Returns integer representation of day of week.</summary>
        /// <returns>1=Sun, 2=Mon, 3=Tue, 4=Wed, 5=Thu, 6=Fri, 7=Sat</returns>
        /// <param name="date">date in the form YYYYMMDD</param>
        public static int DayOfWeek(int date) {
			DateTime dt = new DateTime(Year(date), Month(date), Day(date));
			if (dt.DayOfWeek == System.DayOfWeek.Sunday)
				return 1;
			if (dt.DayOfWeek == System.DayOfWeek.Monday)
				return 2;
			if (dt.DayOfWeek == System.DayOfWeek.Tuesday)
				return 3;
			if (dt.DayOfWeek == System.DayOfWeek.Wednesday)
				return 4;
			if (dt.DayOfWeek == System.DayOfWeek.Thursday)
				return 5;
			if (dt.DayOfWeek == System.DayOfWeek.Friday)
				return 6;
			return 7;
		}

		/// <summary>
		/// Gets the day of week name abbreviated.
		/// </summary>
		/// <returns>The day of week abbreviated.</returns>
		/// <param name="date">Date.</param>
		public static string DayNameAbbreviated(int date) {
			return StringUtils.Take(DayName(date), 3);
		}

		/// <summary>
		/// Gets the day of week name.
		/// </summary>
		/// <returns>The day of week.</returns>
		/// <param name="date">Date.</param>
		public static string DayName(int date) {
			switch (DayOfWeek(date)) {
			case 1:
				return "Sunday";
			case 2:
				return "Monday";
			case 3:
				return "Tuesday";
			case 4:
				return "Wednesday";
			case 5:
				return "Thursday";
			case 6:
				return "Friday";
			case 7:
				return "Saturday";
			default:
				return "Err";
			}
		}

		private static string NormalizeDate(string date) {
			int length = date.Length;
			int year = GuessYear(Int32.Parse(date.Substring(length - 2)));
			return date.Substring(0, length - 2) + year;
		}
			
		/// <summary>
		/// Gets the date.
		/// </summary>
		/// <returns>The date.</returns>
		/// <param name="date">Date.</param>
		public static int Date(string date) {
			try {
				date = date.Replace("-", "/").Trim();
				date = date.Replace("\\.", "/");

				if (DATE_FORMAT_MM_DD_YYYY.IsMatch(date))
					return Date(DateTime.ParseExact(date, "M/d/yyyy", CultureInfo.CurrentCulture));
				else if (DATE_FORMAT_MM_DD_YY.IsMatch(date))
					return Date(DateTime.ParseExact(NormalizeDate(date), "M/d/yyyy", CultureInfo.CurrentCulture));
			} catch {
				return 0;
			}
			return 0;
		}

		public static string Format(string fmt, DateTime dt) {
			return dt.ToString(fmt);
		}

		public static string Format(string fmt, int dt) {
			return Date(dt).ToString(fmt);
		}
			
		public static string Format(Mask fmt, DateTime dt) {
			return dt.ToString(GetFmtStr(fmt));
		}

		public static string Format(Mask fmt, int dt) {
			return Date(dt).ToString(GetFmtStr(fmt));
		}

		public enum Mask { MMDDYY, MMDDYYYY, MONTH_DAY_YEAR, MON_DAY_YEAR, YYYYMMDD, YYYY_MM_DD };

		private static string GetFmtStr(Mask i) {
			switch (i) {
			default:
			case Mask.MMDDYY:
				return "M/d/yy";       // 6/8/59
			case Mask.MMDDYYYY:
				return "M/d/yyyy";     // 6/8/1959
			case Mask.MONTH_DAY_YEAR:
				return "MMMM d, yyyy"; // June 8, 1959
			case Mask.MON_DAY_YEAR:
				return "MMM d, yyyy";  // Jun 8, 1959
			case Mask.YYYYMMDD:
				return "yyyyMMdd";     //  19590608
			case Mask.YYYY_MM_DD:
				return "yyyy-MM-dd";   //  1959-06-08
			}
		}
        public static DateTime RoundUp(DateTime dt, TimeSpan d) {
            long delta = (d.Ticks - (dt.Ticks % d.Ticks)) % d.Ticks;
            return new DateTime(dt.Ticks + delta);
        }

        public static DateTime RoundDown(DateTime dt, TimeSpan d) {
            long delta = dt.Ticks % d.Ticks;
            return new DateTime(dt.Ticks - delta);
        }

        public static DateTime RoundToNearest(DateTime dt, TimeSpan d) {
            long delta = dt.Ticks % d.Ticks;
            bool roundUp = delta > d.Ticks / 2;
            return roundUp ? RoundUp(dt, d) : RoundDown(dt, d);
        }
    }
}

