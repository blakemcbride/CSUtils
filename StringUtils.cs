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

using System.Text;

// Author: Blake McBride

namespace CSUtils {
	
	public static class StringUtils {

		/// <summary>
		/// APL-like Take for strings.
		/// </summary>
		/// <param name="s">the string</param>
		/// <param name="n">number of characters to take (negative means take from back)</param>
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
		/// <param name="n">number of characters to drop (negative means from back)</param>
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

