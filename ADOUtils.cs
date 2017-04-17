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

// Author: Blake McBride

#define USE_POSTGRESQL   //  define if using PostgreSQL

namespace CSUtils {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using MySql.Data.MySqlClient;

    public enum ConnectionType {MicrosoftServer, PostgreSQL, MySQL};

    public class Connection : IDisposable {
#if USE_POSTGRESQL
        private static readonly ADOFactory pgFactory = new PGFactory();
#else
        private static readonly ADOFactory pgFactory = null;
#endif
        private static readonly ADOFactory msFactory = new MSFactory();
        private static readonly ADOFactory mysqlFactory = new MySQLFactory();
        private readonly ADOFactory factory;
        internal DbConnection rconn;
        internal readonly ConnectionType type;
        private DbTransaction trans;
        private bool inTrans;
        private readonly string connectionString;
        private readonly Dictionary<string, SinglyLinkedList<string>> primaryKeyColumns = new Dictionary<string, SinglyLinkedList<string>>();
        private readonly Dictionary<string, bool> TableExistanceCache = new Dictionary<string, bool>();

        private ADOFactory CalcFactory() {
            switch (type) {
            case ConnectionType.MicrosoftServer:
                return msFactory;
            case ConnectionType.PostgreSQL:
                    return pgFactory;
            case ConnectionType.MySQL:
                    return mysqlFactory;
            default:
                return null;
            }
        }

        internal ADOFactory GetFactory() {
            return factory;
        }

        public DbConnection GetDbConnection() {
            return rconn;
        }

        private static string BuildConnectionString(ConnectionType type, string host, string dbname, string user, string pw) {
            string cs;
            switch (type) {
            case ConnectionType.PostgreSQL:
                cs = "Host=" + host + ";DataBase=" + dbname + ";UserName=" + user + ";Password=\"" + pw + "\"";
                break;
            case ConnectionType.MicrosoftServer:
                if (string.IsNullOrEmpty(user))
                    cs = "Data Source=" + host + ";Initial Catalog=" + dbname + ";Integrated Security=true;Timeout=30";
                else
                    cs = "Data Source=" + host + ";Initial Catalog=" + dbname + ";User Id=" + user + ";Password=\"" + pw + "\";Timeout=30";
                break;
            case ConnectionType.MySQL:
                    cs = "Server=" + host + ";DataBase=" + dbname + ";uid=" + user + ";pwd=\"" + pw + "\"";
                    break;
            default:
                throw new System.ArgumentException();
            }
            return cs;
        }

        public Connection(ConnectionType type, string cs) {
            this.type = type;
            factory = CalcFactory();
            connectionString = cs;
            rconn = factory.CreateConnection(cs);
        }

        public Connection(ConnectionType type, DbConnection con) {
            this.type = type;
            factory = CalcFactory();
            rconn = con;
        }

        public Connection(ConnectionType type, string host, string dbname, string user, string pw) :
        this(type, BuildConnectionString(type, host, dbname, user, pw)) {}

        public Connection(ConnectionType type, string host, string dbname) :
        this(type, BuildConnectionString(type, host, dbname, null, null)) {}

        public void BeginTransaction() {
            if (!inTrans) {
                trans = rconn.BeginTransaction();
                inTrans = true;
            }
        }

        public void Commit() {
            if (inTrans) {
                trans.Commit();
                inTrans = false;
            }
        }

        public void Rollback() {
            if (inTrans) {
                trans.Rollback();
                inTrans = false;
            }
        }

        public int ExecuteNonQuery(string sql, params object [] argv) {
            DbCommand cmd = factory.CreateCommand(rconn);
            cmd.CommandText = Command.ProcessPositionalParameters(cmd, sql, argv);
            int r = cmd.ExecuteNonQuery();
            cmd.Dispose();
            return r;
        }

        public SinglyLinkedList<Record> FetchAll(string sql, params object[] argv) {
            Command cmd = NewCommand();
            SinglyLinkedList<Record> recs = cmd.InternalExecute(null, sql, argv).FetchAll();
            cmd.Dispose();
            return recs;
        }

        public Record FetchOne(string sql, params object[] argv) {
            Command cmd = NewCommand();
            Record rec = cmd.InternalExecute(null, sql, argv).FetchOne();
            cmd.Dispose();
            return rec;
        }

        public SinglyLinkedList<string> GetPrimaryKeyColumns(string table) {
            string schema = null;

            table = table.Replace("[", string.Empty);
            table = table.Replace("]", string.Empty);
            if (table.Contains(".")) {
                string [] parts = table.Split('.');
                schema = parts[parts.Length-2];
                table = parts[parts.Length-1];
            }

            if (primaryKeyColumns.ContainsKey(table))
                return primaryKeyColumns[table];

            using (DbCommand cmd = rconn.CreateCommand()) {
                Command.AddParameter(cmd, "table", table);

                if (schema != null)
                    Command.AddParameter(cmd, "schema", schema);

                cmd.CommandText = @"SELECT kcu.COLUMN_NAME
                                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS as tc
                                LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE as  kcu
                                    ON kcu.CONSTRAINT_CATALOG = tc.CONSTRAINT_CATALOG
                                    AND kcu.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA
                                    AND kcu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
                                    -- AND kcu.TABLE_CATALOG = tc.TABLE_CATALOG  doesn't work on MySQL
                                    AND kcu.TABLE_SCHEMA = tc.TABLE_SCHEMA
                                    AND kcu.TABLE_NAME = tc.TABLE_NAME
                                WHERE tc.CONSTRAINT_TYPE ='PRIMARY KEY' ";
                if (schema != null)
                    cmd.CommandText += "AND tc.TABLE_SCHEMA = @schema ";
                cmd.CommandText += "AND tc.TABLE_NAME = @table ORDER BY ORDINAL_POSITION";

                using (DbDataReader reader = cmd.ExecuteReader(CommandBehavior.KeyInfo)) {
                    SinglyLinkedList<string> res = new SinglyLinkedList<string>();
                    while (reader.Read()) {
                        var str = reader[0];
                        if (str != DBNull.Value)
                            res.Add((string) str);
                    }
                    primaryKeyColumns[table] = res;
                    return res;
                }
            }
        }

        public bool TableExists(string table)
        {
            string schema = null;

            table = table.Replace("[", string.Empty);
            table = table.Replace("]", string.Empty);
            if (table.Contains(".")) {
                string[] parts = table.Split('.');
                schema = parts[parts.Length-2];
                table = parts[parts.Length-1];
            }

            if (TableExistanceCache.ContainsKey(table))
                return TableExistanceCache[table];
            bool res = false;
            using (Command cmd = NewCommand()) {
                if (schema != null)
                    cmd.AddParameter("schema", schema);
                cmd.AddParameter("table", table);
                string sql = "select count(*) from information_schema.tables where table_name = @table";
                if (schema != null)
                    sql += " and table_schema = @schema";
                using (Cursor cursor = cmd.Execute(sql))
                    if (cursor.IsNextUnbuffered()) {
                        Record rec = cursor.GetRecord();
                        res = (long)rec["count(*)"] != 0;
                    }
            }
            return TableExistanceCache[table] = res;
        }

        public void Dispose() {
            if (inTrans) {
                trans.Rollback();
                inTrans = false;
            }
            if (rconn != null) {
                rconn.Close();
                rconn = null;
            }
        }

        public Command NewCommand() {
            return new Command(this);
        }

        public Record NewRecord(string tbl) {
            return new Record(this, tbl);
        }

        public Record NewRecord(Type tableInfo) {
            return new Record(this, tableInfo);
        }
    }

    public class Command : IDisposable {
        internal Connection conn;
        internal DbCommand cmd;
        internal bool isSelect;
        internal string tname;
        internal Type tableinfo;  // should be here because may be shared among multiple cursors
        private string lastSQL;

        internal Command(Connection c) {
            conn = c;
            cmd = conn.GetFactory().CreateCommand(c.rconn);
        }

        internal static void AddParameter(DbCommand cmd, string param, object val)
        {
            DbParameter p = cmd.CreateParameter();
            p.ParameterName = "@" + param;
            p.Value = val;
            p.Direction = ParameterDirection.Input;
            cmd.Parameters.Add(p);
        }

        public DbCommand GetDbCommand() {
            return cmd;
        }

        public Connection GetConnection() {
            return conn;
        }

        public int ExecuteNonQuery() {
            isSelect = false;
            return cmd.ExecuteNonQuery();
        }

        public int ExecuteNonQuery(string sql, params object [] argv) {
            isSelect = false;
            lastSQL = sql;
            cmd.CommandText = ProcessPositionalParameters(cmd, sql, argv);
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        ///  Code to convert positional ? parameters into ADO.NET named parameters
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="sql"></param>
        /// <param name="argv"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static string ProcessPositionalParameters(DbCommand cmd, string sql, object [] argv) {
            if (argv.Length != 0) {
                cmd.Parameters.Clear();
                int paramIdx = 0;
                bool inString = false;
                bool inEscape = false;
                StringBuilder sb = new StringBuilder();
                for (int i = 0 ; i < sql.Length ; i++) {
                    char c = sql[i];
                    if (inEscape)
                        inEscape = false;
                    else if (c == '\\')
                        inEscape = true;
                    else if (inString) {
                        if (c == '\'')
                            if (i + 1 < sql.Length && sql[i + 1] == '\'') {
                                sb.Append('\'');
                                i++;
                            } else
                                inString = false;
                    } else if (c == '\'')
                        inString = true;
                    else if (c == '?') {
                        if (paramIdx >= argv.Length)
                            throw new Exception("missing parameter value");
                        string pname = "param" + paramIdx;
                        sb.Append("@" + pname + " ");
                        AddParameter(cmd, pname, argv[paramIdx++]);
                        continue;
                    }
                    sb.Append(c);
                }
                return sb.ToString();
            }
            return sql;
        }

        public Cursor Execute(Type tblinfo, string sql, params object[] argv) {
            return InternalExecute(tblinfo, sql, argv);
        }

        internal Cursor InternalExecute(Type tblinfo, string sql, object [] argv) {
            if (tblinfo == tableinfo && sql == lastSQL)
                return Execute();
            lastSQL = sql;
            sql = sql.TrimStart();
            if (cmd == null)
                cmd = conn.GetFactory().CreateCommand(conn.rconn);
            tableinfo = tblinfo;
            isSelect = false;
            tname = null;
            if (sql.Length > 7 && sql.Substring(0, 7).ToLower().Equals("select ")) {
                string sql2 = sql.ToLower();
                int i = sql2.IndexOf(" from ");
                if (i >= 8) {
                    StringBuilder sbname = new StringBuilder();
                    for (i += 6; i < sql2.Length; i++) {
                        if (char.IsWhiteSpace(sql2[i]))
                            break;
                        sbname.Append(sql2[i]);
                    }
                    tname = sbname.ToString();
                    isSelect = true;
                }
            }
            cmd.CommandText = ProcessPositionalParameters(cmd, sql, argv);
            return new Cursor(this);
        }

        private object ExecuteAutoInc(Type tblinfo, string sql, params object [] argv) {
            if (tblinfo == tableinfo && sql == lastSQL)
                return Execute();
            lastSQL = sql;
            sql = sql.TrimStart();
            if (cmd == null)
                cmd = conn.GetFactory().CreateCommand(conn.rconn);
            tname = null;
            switch (conn.type) {
                case ConnectionType.MicrosoftServer:
                    sql += "; SELECT SCOPE_IDENTITY();";
                    break;
                case ConnectionType.PostgreSQL:
                    int idx = sql.ToLower().IndexOf(" into ");
                    if (idx < 6)
                        throw new Exception("No table found");
                    tname = sql.Substring(idx + 6).TrimStart();
                    idx = tname.IndexOf(' ');
                    if (idx < 1)
                        throw new Exception("No table found");
                    tname = tname.Substring(0, idx);

                    SinglyLinkedList<string> pcols = conn.GetPrimaryKeyColumns(tname);
                    if (pcols.Count != 1)
                        throw new Exception("Primary key for table " + tname + " must be a single, serial column.");

                    sql += "RETURNING " + pcols.First();
                    break;
                case ConnectionType.MySQL:
                    // nothing special
                    break;
                default:
                    throw new Exception("Unable to handle auto-inc columns with this database type.");
            }
            tableinfo = tblinfo;
            isSelect = false;
            cmd.CommandText = ProcessPositionalParameters(cmd, sql, argv);

            if (conn.type == ConnectionType.MySQL)
                return ((MySqlCommand) cmd).LastInsertedId;

            DbDataReader rdr = cmd.ExecuteReader();
            object val = null;
            if (rdr.Read())
                val = rdr.GetValue(0);
            rdr.Close();
            return val;
        }

        public Cursor Execute() {
            return new Cursor(this);
        }

        public Cursor Execute(string sql, params object [] argv)
        {
            if (sql == lastSQL)
                return Execute();
            return InternalExecute(null, sql, argv);
        }

        public SinglyLinkedList<Record> FetchAll(string sql, params object [] argv)
        {
            return InternalExecute(null, sql, argv).FetchAll();
        }
        public Record FetchOne(string sql, params object[] argv) {
            return InternalExecute(null, sql, argv).FetchOne();
        }

        public DbParameter AddParameter(string name, object val) {
            return conn.GetFactory().AddParameter(cmd, name, val);
        }

        public void ClearParameters() {
            cmd.Parameters.Clear();
        }

        public void Dispose() {
            if (cmd != null) {
                cmd.Dispose();
                cmd = null;
            }
        }
    }

    public class Cursor : IDisposable {
        private readonly Command cmd;
        internal DbDataReader reader;
        private FileStream fs;
        private BinaryFormatter formatter;
        private long recordsBuffered;
        private long recordsRead;
        private Record lastRec;

        public Cursor(Command c) {
            cmd = c;
            reader = cmd.cmd.ExecuteReader();
        }

        private void BufferAll() {
            fs = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 10000, FileOptions.DeleteOnClose | FileOptions.SequentialScan);
            formatter = new BinaryFormatter();
            var dict = new Dictionary<string, object>();
            while (reader.Read()) {
                dict.Clear();
                for (int i = 0; i < reader.FieldCount; i++)
                    dict[reader.GetName(i).ToLower()] = reader.GetValue(i);
                formatter.Serialize(fs, dict);
                recordsBuffered++;
            }
            CloseReader();
            fs.Position = 0;
        }

        /// <summary>
        /// Gets the next record.  Buffers all records in a local temp file to avoid leaving the internal cursor open.
        /// In other words, you can use nested queries.  However, this is less effecient than NextUnbuffered().
        /// </summary>
        /// <returns>the Record object or <c>null</c> if no more records.</returns>
        public Record Next() {
            lastRec = null;
            if (fs == null)
                BufferAll();
            if (fs.Length == 0 || recordsRead == recordsBuffered) {
                Dispose();
                return lastRec;
            }
            try {
                lastRec = new Record(cmd.conn, cmd.tname, cmd, (Dictionary<string, object>) formatter.Deserialize(fs));
                recordsRead++;
            } catch {
                Dispose();
            }
            return lastRec;
        }

        public bool IsNext() {
            return null != Next();
        }

        /// <summary>
        /// Gets the next record.  Records are not buffered so nested queries are not allowed.  However, this is more effecient than Next().
        /// </summary>
        /// <returns>the Record object or <c>null</c> if no more records.</returns>
        public Record NextUnbuffered() {
            if (reader.Read())
                return lastRec = new Record(cmd.conn, cmd.tname, cmd, reader);
            Dispose();
            return lastRec = null;
        }

        public bool IsNextUnbuffered() {
            return null != NextUnbuffered();
        }

        public Record FetchOne() {
            lastRec = NextUnbuffered();
            if (lastRec != null)
                Dispose();
            return lastRec;
        }

        public Record GetRecord() {
            return lastRec;
        }

        public SinglyLinkedList<Record> FetchAll() {
            var r = new SinglyLinkedList<Record>();
            Record rec;
            while (null != (rec=NextUnbuffered()))
                r.Add(rec);
            return r;
        }

        public SinglyLinkedList<object> FetchAllOneColumn() {
            var r = new SinglyLinkedList<object>();
            Record rec;
            while (null != (rec=NextUnbuffered()))
                r.Add(rec.GetAllColumns().First());
            return r;
        }

        private void CloseReader() {
            if (reader != null) {
                reader.Close();
                reader = null;
            }
        }

        public void Dispose() {
            CloseReader();
            if (fs != null) {
                fs.Close();
                fs = null;
            }
        }
    }

    public class Record : IDisposable {
        private readonly Dictionary<string,object> cols;
        private readonly Dictionary<string,object> ocols;
        private readonly Connection conn;
        private readonly Command cmd;
        private readonly string table;
        private DbCommand dbCmd;

        public Record(Connection c, string tbl) {
            conn = c;
            table = tbl;
            cols = new Dictionary<string, object>();
            ocols = new Dictionary<string, object>();
        }

        public Record(Connection c, string tbl, Command cmd, DbDataReader reader) {
            conn = c;
            this.cmd = cmd;
            table = tbl;
            cols = new Dictionary<string, object>();
            ocols = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++) {
                string name = reader.GetName(i).ToLower();
                object val = reader.GetValue(i);
                cols[name] = val;
                ocols[name] = val;
            }
        }

        public Record(Connection c, string tbl, Command cmd, Dictionary<string, object> columns) {
            conn = c;
            this.cmd = cmd;
            table = tbl;
            ocols = columns;
            cols = new Dictionary<string, object>(ocols);
        }

        public Record(Connection c, Type tableInfo) {
            conn = c;
//          this.tableInfo = tableInfo;
            table = (string) tableInfo.GetField("TABLE_NAME").GetValue(null);
            cols = new Dictionary<string, object>();
            ocols = new Dictionary<string, object>();
        }

        public object Get(string name) {
            object val;
            bool r = cols.TryGetValue(name.ToLower(), out val);
            return r ? val : null;
        }

        public object Set(string name, object val) {
            cols[name.ToLower()] = val;
            return val;
        }

        public object this[string fn] {
            get {
                return Get(fn);
            }
            set {
                Set(fn, value);
            }
        }

        public Dictionary<string, object> GetAllColumns() {
            return cols;
        }

        public Record Clear() {
            cols.Clear();
            return this;
        }

        public int Insert() {
            if (cols.Count == 0)
                return 0;
            StringBuilder sql = new StringBuilder("insert into " + table + " (");
            bool needComma = false;
            foreach (KeyValuePair<string, object> fld in cols) {
                if (needComma)
                    sql.Append(", ");
                else
                    needComma = true;
                sql.Append(fld.Key);
            }
            sql.Append(") values (");
            needComma = false;
            if (dbCmd == null)
                dbCmd = conn.GetFactory().CreateCommand(conn.rconn);
            else
                dbCmd.Parameters.Clear();
            foreach (KeyValuePair<string, object> keyColumn in cols) {
                if (needComma)
                    sql.Append(", ");
                else
                    needComma = true;
                sql.Append("@" + keyColumn.Key);
                Command.AddParameter(dbCmd, keyColumn.Key, keyColumn.Value);
            }
            sql.Append(")");

            dbCmd.CommandText = sql.ToString();
            return dbCmd.ExecuteNonQuery();
        }

        public int Update() {
            if (cmd == null)
                throw new InvalidOperationException("Can't update record; not from select");
            if (cmd.tableinfo == null && table == null)
                throw new InvalidOperationException("Can't update record; no table name");
            var changedColumns = new SinglyLinkedList<KeyValuePair<string, object>>();
            foreach (KeyValuePair<string, object> item in cols)
                if (!ocols.ContainsKey(item.Key) || ocols[item.Key] != item.Value)
                    changedColumns.Add(new KeyValuePair<string, object>(item.Key, item.Value));
            if (changedColumns.Count != 0) {
                if (dbCmd == null)
                    dbCmd = conn.rconn.CreateCommand();
                else
                    dbCmd.Parameters.Clear();
                StringBuilder sql = new StringBuilder("update " + table + " set ");
                bool needComma = false;
                foreach (KeyValuePair<string, object> fld in changedColumns) {
                    if (needComma)
                        sql.Append(", ");
                    else
                        needComma = true;
                    sql.Append(fld.Key + "=@" + fld.Key);
                    Command.AddParameter(dbCmd, fld.Key, fld.Value);
                }
                sql.Append(" where ");
                var primaryKeyColumns = cmd.conn.GetPrimaryKeyColumns(table);
                needComma = false;
                foreach (string keyColumn in primaryKeyColumns) {
                    if (needComma)
                        sql.Append(", ");
                    else
                        needComma = true;
                    sql.Append(keyColumn + "=@" + keyColumn);
                    Command.AddParameter(dbCmd, keyColumn, ocols[keyColumn.ToLower()]);
                }
                dbCmd.CommandText = sql.ToString();
                return dbCmd.ExecuteNonQuery();
            }
            return 0;
        }

        public int Delete() {
            if (cmd == null)
                throw new InvalidOperationException("Can't update record; not from select");
            if (cmd.tableinfo == null && table == null)
                throw new InvalidOperationException("Can't delete record; no table name");
            if (dbCmd == null)
                dbCmd = conn.rconn.CreateCommand();
            else
                dbCmd.Parameters.Clear();
            StringBuilder sql = new StringBuilder("delete from " + table + " where ");
            bool needComma = false;
            var primaryKeyColumns = cmd.conn.GetPrimaryKeyColumns(table);
            foreach (string keyColumn in primaryKeyColumns) {
                if (needComma)
                    sql.Append(", ");
                else
                    needComma = true;
                sql.Append(keyColumn + "=@" + keyColumn);
                Command.AddParameter(dbCmd, keyColumn, ocols[keyColumn.ToLower()]);
            }
            dbCmd.CommandText = sql.ToString();
            return dbCmd.ExecuteNonQuery();
    }

        public void Dispose() {
            if (dbCmd != null) {
                dbCmd.Dispose();
                dbCmd = null;
            }
        }
    }

}
