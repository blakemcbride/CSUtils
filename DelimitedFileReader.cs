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
using System.Collections;
using System.IO;
using System.Text;

namespace CSUtils {
    public class DelimitedFileReader : IDisposable {
        private char delimiter;
        private string delimeterString;
        private char quote;
        private ArrayList lineValues = new ArrayList();
        private int fieldPos = 0;
        private int fieldCountCheck = -1;
        private StreamReader fr;
        private FileStream fyle;
        private string originalRow;
        private bool disposed = false;

        public DelimitedFileReader(FileStream f, char delimiter, char quote) {
            fyle = f;
            fr = new StreamReader(new BufferedStream(f));
            this.delimiter = delimiter;
            delimeterString = delimiter.ToString();
            this.quote = quote;
        }

        public DelimitedFileReader(FileStream f) :
            this(f, ',', '"') { }

        public DelimitedFileReader(String name) :
            this(new FileStream(name, FileMode.Open), ',', '"') {}


        public DelimitedFileReader(String name, char delimiter, char quote) :
            this(new FileStream(name, FileMode.Open), delimiter, quote) {}

        public void Close() {
            try {
                fr?.Close();
            } catch (IOException) {
            } finally {
                fr = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing) {
                Close();
            }
            disposed = true;
        }

        public double GetDouble(int i) {
            try {
                return double.Parse(GetString(i));
            } catch (Exception e) {
                throw e;
            }
        }

        public void MoveToStart() {
            try {
                lineValues.Clear();
                fr.Close();
                fr = new StreamReader(new BufferedStream(fyle));
            } catch (IOException ex) { //it was just there! so log it only
                throw ex;
            }
        }

        public void SkipLine() {
            fr.ReadLine();
            originalRow = null;
        }

        public string GetRow() {
            return originalRow;
        }

        public bool NextLine() {
            lineValues.Clear();
            originalRow = null;
            string line = fr.ReadLine();

            while (true) {
                if (line == null)
                    return false;
                if (!line.Trim().Equals("")) {
                    string line2 = line.Replace(delimeterString, String.Empty);
                    if (line2.Length > 0)
                        break;
                }
                line = fr.ReadLine();
            }

            originalRow = line;
            StringBuilder sb = new StringBuilder();
            bool inQuotes = false;
            do {
                if (inQuotes) {
                    // continuing a quoted section, reappend newline
                    sb.Append("\n");
                    line = fr.ReadLine();
                    if (line == null)
                        break;
                    originalRow += "\n" + line;
                }
                for (int i = 0 ; i < line.Length ; i++) {
                    char c = line[i];
                    if (c == quote)
                        // this gets complex... the quote may end a quoted block, or escape another quote.
                        // do a 1-char lookahead:
                        if (inQuotes // we are in quotes, therefore there can be escaped quotes in here.
                            && line.Length > (i + 1) // there is indeed another character to check.
                            && line[i + 1] == quote) { // ..and that char. is a quote also.
                            // we have two quote chars in a row == one quote char, so consume them both and
                            // put one on the token. we do *not* exit the quoted text.
                            sb.Append(line[i + 1]);
                            i++;
                        } else {
                            inQuotes = !inQuotes;
                            // the tricky case of an embedded quote in the middle: a,bc"d"ef,g
                            if (i > 2 //not on the beginning of the line
                                && line[i - 1] != delimiter //not at the beginning of an escape sequence
                                && line.Length > (i + 1)
                                && line[i + 1] != this.delimiter //not at the end of an escape sequence
                            )
                                sb.Append(c);
                        }
                    else if (c == delimiter && !inQuotes) {
                        lineValues.Add(sb.ToString());
                        sb = new StringBuilder(); // start work on next token
                    } else
                        sb.Append(c);
                }
            } while (inQuotes);
            lineValues.Add(sb.ToString());
            if (fieldCountCheck > 0 && fieldCountCheck != lineValues.Count)
                throw new Exception("Bad number of records read.  Expected " + fieldCountCheck + " got " + lineValues.Count);
            fieldPos = 0;
            return true;
        }

        public int Count() {
            return lineValues.Count;
        }

        public string NextString() {
            return GetString(fieldPos++);
        }

        public string GetString(int item) {
            if (item >= lineValues.Count)
                return "";
            return (string) lineValues[item];
        }

        public int NextInt() {
            try {
                return int.Parse(NextString());
            } catch (Exception) {
                return 0;
            }
        }

        public double NextDouble() {
            try {
                return double.Parse(NextString());
            } catch (Exception) {
                return 0;
            }
        }

        public int GetInt(int item) {
            try {
                return int.Parse(GetString(item));
            } catch (Exception) {
                return 0;
            }
        }

        public void SetFieldCountCheck(int check) {
            fieldCountCheck = check;
        }
    }
}