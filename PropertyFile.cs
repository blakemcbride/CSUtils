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
using System.Collections.Generic;

namespace CSUtils {
    public class PropertyFile {
        private Dictionary<string, string> list;
        private string filename;

        public PropertyFile(string file) {
            Reload(file);
        }

        public string Get(string field, string defValue) {
            return Get(field) == null ? defValue : Get(field);
        }

        public string Get(string field) {
            return list.ContainsKey(field) ? list[field] : null;
        }

        public void Set(string field, object value) {
            if (!list.ContainsKey(field))
                list.Add(field, value.ToString());
            else
                list[field] = value.ToString();
        }

        public int GetInt(string field) {
            string sval = Get(field);
            if (sval == null)
                return 0;
            int res;
            Int32.TryParse(sval, out res);
            return res;
        }

        public int GetInt(string field, int dflt) {
            string sval = Get(field);
            if (sval == null)
                return dflt;
            int res;
            if (!Int32.TryParse(sval, out res))
                res = dflt;
            return res;
        }

        public DateTime? GetDateTime(string field) {
            string sval = Get(field);
            if (sval == null)
                return null;
            int date = DateUtils.Date(sval);
            if (date == 0)
                return null;
            return new DateTime(DateUtils.Year(date), DateUtils.Month(date), DateUtils.Day(date));
        }

        public void Save() {
            Save(filename);
        }

        public void Save(string fname) {
            filename = fname;

            if (!System.IO.File.Exists(fname))
                System.IO.File.Create(fname);

            System.IO.StreamWriter file = new System.IO.StreamWriter(fname);

            foreach(string prop in list.Keys)
                if (!string.IsNullOrWhiteSpace(list[prop]))
                    file.WriteLine(prop + "=" + list[prop]);

            file.Close();
        }

        public void Reload() {
            Reload(filename);
        }

        public void Reload(string fname) {
            filename = fname;
            list = new Dictionary<string, string>();

            if (System.IO.File.Exists(fname))
                LoadFromFile(fname);
            else
                System.IO.File.Create(fname);
        }

        private void LoadFromFile(string file) {
            foreach (string line in System.IO.File.ReadAllLines(file)) {
                string line2 = line.Trim();
                if (!string.IsNullOrEmpty(line2) &&
                        !line2.StartsWith(";") &&
                        !line2.StartsWith("#") &&
                        !line2.StartsWith("'") &&
                        line2.Contains("=")) {
                    int index = line2.IndexOf('=');
                    string key = line2.Substring(0, index).Trim();
                    string value = line2.Substring(index + 1).Trim();

                    if (value.StartsWith("\"") && value.EndsWith("\"") ||
                            value.StartsWith("'") && value.EndsWith("'"))
                        value = value.Substring(1, value.Length - 2);
                    list[key] = value;  //  take the last value assigned
                }
            }
        }
    }
}
