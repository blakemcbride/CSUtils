
using System;
using System.Text;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;


/// Author: Blake McBride

namespace Utils {

	public enum ConnectionType {MicrosoftServer, PostgreSQL};

	class Connection {
		private static ADOFactory pgFactory = new PGFactory();
		private static ADOFactory msFactory = new MSFactory();
		private ADOFactory factory;
		internal DbConnection rconn;
		internal DbConnection wconn;
		private ConnectionType type;
		private DbTransaction trans;
		private bool inTrans = false;
		private string connectionString;
		private DbCommand cmd;

		private ADOFactory calcFactory() {
			switch (type) {
			case ConnectionType.MicrosoftServer:
				return msFactory;
			case ConnectionType.PostgreSQL:
				return pgFactory;
			default:
				return null;
			}
		}

		internal ADOFactory getFactory() {
			return factory;
		}

		public DbConnection getDbConnection() {
			return rconn;
		}

		private static string buildConnectionString(ConnectionType type, string host, string dbname, string user, string pw) {
			string cs;
			switch (type) {
			case ConnectionType.PostgreSQL:
				cs = "Host=" + host + ";DataBase=" + dbname + ";UserName=" + user + ";Password=" + pw;
				break;
			case ConnectionType.MicrosoftServer:
				if (user == null || user.Length == 0)
					cs = "Data Source=" + host + ";Initial Catalog=" + dbname + ";Integrated Security=true";
				else
					cs = "Data Source=" + host + ";Initial Catalog=" + dbname + ";User Id=" + user + ";Password=" + pw;
				break;
			default:
				throw new System.ArgumentException();
			}
			return cs;
		}

		public Connection(ConnectionType type, string cs) {
			this.type = type;
			factory = calcFactory();
			connectionString = cs;
			rconn = factory.CreateConnection(cs);
		}

		public Connection(ConnectionType type, string host, string dbname, string user, string pw) :
		this(type, buildConnectionString(type, host, dbname, user, pw)) {}

		public Connection(ConnectionType type, string host, string dbname) :
		this(type, buildConnectionString(type, host, dbname, null, null)) {}

		internal DbConnection getWconn() {
			if (wconn == null)
				wconn = factory.CreateConnection(connectionString);
			return wconn;
		}

		public void BeginTransaction() {
			if (!inTrans) {
				trans = getWconn().BeginTransaction();
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

		public int Execute(string sql) {
			if (cmd == null)
				cmd = factory.CreateCommand(rconn);
			cmd.CommandText = sql;
			int r = cmd.ExecuteNonQuery();
			return r;
		}

		public void Close() {
			if (inTrans) {
				trans.Rollback();
				inTrans = false;
			}
			if (cmd != null) {
				cmd.Dispose();
				cmd = null;
			}
			if (rconn != null) {
				rconn.Close();
				rconn = null;
			}
			if (wconn != null) {
				wconn.Close();
				wconn = null;
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

	class Command {
		internal Connection conn;
		internal DbCommand cmd;
		internal bool isSelect;
		internal string tname;
		internal Type tableinfo;  // should be here because may be shared among multiple cursors

		internal Command(Connection c) {
			conn = c;
			cmd = conn.getFactory().CreateCommand(c.rconn);
		}

		public DbCommand GetDbCommand() {
			return cmd;
		}

		public int ExecuteNonQuery() {
			return cmd.ExecuteNonQuery();
		}

		public int ExecuteNonQuery(string sql) {
			cmd.CommandText = sql;
			return cmd.ExecuteNonQuery();
		}

		public Cursor Execute(Type tableinfo, string sql) {
			sql = sql.TrimStart();
			if (cmd == null)
				cmd = conn.getFactory().CreateCommand(conn.rconn);
			cmd.CommandText = sql;
			this.tableinfo = tableinfo;
			isSelect = false;
			tname = null;
			if (sql.Length > 7 && sql.Substring(0, 7).ToLower().Equals("select ")) {
				string sql2 = sql.ToLower();
				int i = sql2.IndexOf(" from ");
				if (i > 8) {
					StringBuilder sbname = new StringBuilder();
					for (i += 6; i < sql2.Length; i++) {
						if (sql2[i] == ' ')
							break;
						sbname.Append(sql2[i]);
					}
					tname = sbname.ToString();
					isSelect = true;
				}
			}
			return new Cursor(this);
		}

		public Cursor Execute() {
			return new Cursor(this);
		}

		public Cursor Execute(string sql) {
			return Execute(null, sql);
		}

		public DbParameter AddParameter(string name, object val) {
			return conn.getFactory().AddParameter(cmd, name, val);
		}

		public void ClearParameters() {
			cmd.Parameters.Clear();
		}

		public void Close() {
			if (cmd != null) {
				cmd.Dispose();
				cmd = null;
			}
		}
	}

	class Cursor {
		private Command cmd;
		private DbCommand ucmd;
		private DbDataReader rec;
		private Dictionary<string,object> cols = new Dictionary<string, object>();
		private Dictionary<string,object> ocols = new Dictionary<string, object>();
		private FileStream fs;
		private BinaryFormatter formatter;

		public Cursor(Command c) {
			cmd = c;
			rec = cmd.cmd.ExecuteReader();
		}

		private void bufferAll() {
			fs = new FileStream(System.IO.Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 10000, FileOptions.DeleteOnClose | FileOptions.SequentialScan);
			formatter = new BinaryFormatter();
			var dict = new Dictionary<string, object>();
			while (rec.Read()) {
				dict.Clear();
				for (int i = 0; i < rec.FieldCount; i++)
					dict[rec.GetName(i).ToLower()] = rec.GetValue(i);
				try {
					formatter.Serialize(fs, dict);
				} catch (SerializationException e) {
					Console.WriteLine("Failed to serialize ADO records. Reason: " + e.Message);
					throw e;
				}
			}
			internalClose();
			fs.Position = 0;
		}

		/// <summary>
		/// Gets the next record.  Buffers all records in a local temp file to avoid leaving the internal cursor open.
		/// In other words, you can use nested queries.  However, this is less effecient than NextUnbuffered().
		/// </summary>
		/// <returns><c>true</c>, if there is another record, <c>false</c> if no more records.</returns>
		public bool Next() {
			if (fs == null)
				bufferAll();
			try {
				ocols = (Dictionary<string, object>) formatter.Deserialize(fs);
				cols = new Dictionary<string, object>(ocols);
			} catch {
				ocols.Clear();
				cols.Clear();
				Close();
				return false;
			}
			return true;
		}

		/// <summary>
		/// Gets the next record.  Records are not buffered so nested queries are not allowed.  However, this is more effecient than Next().
		/// </summary>
		/// <returns><c>true</c>, if there is another record, <c>false</c> if no more records.</returns>
		public bool NextUnbuffered() {
			bool r = rec.Read();
			cols.Clear();
			ocols.Clear();
			if (r)
				for (int i = 0; i < rec.FieldCount; i++) {
					string name = rec.GetName(i).ToLower();
					object val = rec.GetValue(i);
					cols[name] = val;
					ocols[name] = val;
				}
			else
				internalClose();
			return r;
		}

		public Dictionary<string,object> GetAllColumns() {
			return cols;
		}

		public Dictionary<string,object> FetchOne() {
			Dictionary<string,object> r = Next() ? cols : null;
			internalClose();
			return r;
		}

		public List<Dictionary<string,object>> FetchAll() {
			var r = new List<Dictionary<string,object>>();
			while (Next())
				r.Add(cols);
			internalClose();
			return r;
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

		private void internalClose() {
			if (rec != null) {
				rec.Close();
				rec = null;
			}
			if (cmd != null  &&  cmd.cmd != null) {
				cmd.cmd.Dispose();
				cmd.cmd = null;
			}
		}

		public void Close() {
			internalClose();
			if (fs != null) {
				fs.Close();
				fs = null;
			}
		}

		public void Update() {
			if (!cmd.isSelect)
				throw new System.InvalidOperationException("Can't update record; not in select");
			if (cmd.tableinfo == null)
				throw new System.InvalidOperationException("Can't update record; no table info");
			if (cmd.tname == null)
				throw new System.InvalidOperationException("Can't update record; no table name");
			var cf = new List<KeyValuePair<string,object>>();
			foreach (var item in cols) {
				if (!ocols.ContainsKey(item.Key) || ocols[item.Key] != item.Value)
					cf.Add(new KeyValuePair<string, object>(item.Key, item.Value));
			}
			if (cf.Count != 0) {
				StringBuilder sql = new StringBuilder("update " + cmd.tname + " set ");
				bool needComma = false;
				foreach (var fld in cf) {
					if (needComma)
						sql.Append(", ");
					else
						needComma = true;
					if (fld.Value is string)
						sql.Append(fld.Key + "='" + fld.Value + "'");
					else
						sql.Append(fld.Key + "=" + fld.Value);
				}
				sql.Append(" where ");
				var fn = (List<string>) cmd.tableinfo.GetField("PRIMARY_KEY").GetValue(null);
				needComma = false;
				foreach (var x in fn) {
					if (needComma)
						sql.Append(", ");
					else
						needComma = true;	
					if (ocols[x] is string)
						sql.Append(x + "='" + ocols[x] + "'");
					else
						sql.Append(x + "=" + ocols[x]);
				}
				if (ucmd == null)
					ucmd = cmd.conn.getFactory().CreateCommand(cmd.conn.getWconn());
				ucmd.CommandText = sql.ToString();
				ucmd.ExecuteNonQuery();
			}
		}

		public void Delete() {
			if (!cmd.isSelect)
				throw new System.InvalidOperationException("Can't delete record; not in select");
			if (cmd.tableinfo == null)
				throw new System.InvalidOperationException("Can't delete record; no table info");
			if (cmd.tname == null)
				throw new System.InvalidOperationException("Can't delete record; no table name");
			var cf = new List<KeyValuePair<string,object>>();
			foreach (var item in cols) {
				if (!ocols.ContainsKey(item.Key) || ocols[item.Key] != item.Value)
					cf.Add(new KeyValuePair<string, object>(item.Key, item.Value));
			}
			if (cf.Count != 0) {
				StringBuilder sql = new StringBuilder("delete from " + cmd.tname + " where ");
				bool needComma = false;
				var fn = (List<string>) cmd.tableinfo.GetField("PRIMARY_KEY").GetValue(null);
				foreach (var x in fn) {
					if (needComma)
						sql.Append(", ");
					else
						needComma = true;	
					if (ocols[x] is string)
						sql.Append(x + "='" + ocols[x] + "'");
					else
						sql.Append(x + "=" + ocols[x]);
				}
				if (ucmd == null)
					ucmd = cmd.conn.getFactory().CreateCommand(cmd.conn.getWconn());
				ucmd.CommandText = sql.ToString();
				ucmd.ExecuteNonQuery();
			}
		}
	}

	class Record {
		private Dictionary<string,object> cols = new Dictionary<string, object>();
		private Connection conn;
		private string table;
		private DbCommand cmd;
//		private Type tableInfo;

		public Record(Connection c, string tbl) {
			conn = c;
			table = tbl;
		}

		public Record(Connection c, Type tableInfo) {
			conn = c;
//			this.tableInfo = tableInfo;
			table = (string) tableInfo.GetField("TABLE_NAME").GetValue(null);
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

		public Record Clear() {
			cols.Clear();
			return this;
		}

		public void AddRecord() {
			if (cols.Count != 0) {
				StringBuilder sql = new StringBuilder("insert into " + table + " (");
				bool needComma = false;
				foreach (var fld in cols) {
					if (needComma)
						sql.Append(", ");
					else
						needComma = true;
					sql.Append(fld.Key);
				}
				sql.Append(") values (");
				needComma = false;
				foreach (var x in cols) {
					if (needComma)
						sql.Append(", ");
					else
						needComma = true;	
					if (x.Value is string)
						sql.Append("'" + x.Value + "'");
					else
						sql.Append(x.Value);
				}
				sql.Append(")");

				if (cmd == null)
					cmd = conn.getFactory().CreateCommand(conn.getWconn());
				cmd.CommandText = sql.ToString();
				cmd.ExecuteNonQuery();
			}
		}

		public void Close() {
			if (cmd != null) {
				cmd.Dispose();
				cmd = null;
			}
		}
	
	}

}
