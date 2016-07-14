using System;
using System.Text;
using System.Text.RegularExpressions;

/// Author: Blake McBride

namespace ADOTest {
	
	public static class StringUtils {

		/// <summary>
		/// APL-like Take for strings.
		/// </summary>
		/// <param name="s">the string</param>
		/// <param name="n">number of characters to take (negative means take from back)</param></param>
		public static string Take(string s, int n) {
			int len = s.Length;
			if (len == n)
				return s;
			if (n >= 0) {
				if (n < len)
					return s.Substring(0, n);
				StringBuilder sb = new StringBuilder(s);
				for (n -= len; n-- > 0;)
					sb.Append(' ');
				return sb.ToString();
			} else {
				n = -n;
				if (n < len)
					return Drop(s, len - n);
				StringBuilder sb = new StringBuilder();
				for (n -= len; n-- > 0;)
					sb.Append(' ');
				sb.Append(s);
				return sb.ToString();
			}
		}

		/// <summary>
		/// APL-like Drop for strings.
		/// </summary>
		/// <param name="s">the string</param>
		/// <param name="n">number of characters to drop (egative means from back)</param>
		public static string Drop(string s, int n) {
			if (n == 0)
				return s;
			int len = s.Length;
			if (n >= len || -n >= len)
				return "";
			if (n > 0)
				return s.Substring(n);
			return s.Substring(0, len + n);
		}
			

	}
}

