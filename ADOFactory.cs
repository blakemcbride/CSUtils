using System;
using System.Data;
using System.Data.Common;

using System.Data.SqlClient;
using Npgsql;

/// Author: Blake McBride

// This code does largely the same thing as DbProviderFactory but doesn't require foreign data provider registration.
namespace Utils {
	
	internal abstract class ADOFactory  {
		public abstract DbConnection CreateConnection(string cs);
		public abstract DbCommand  CreateCommand(DbConnection conn);
		public abstract DbParameter AddParameter(DbCommand cmd, string name, object val);
	}

	internal class MSFactory : ADOFactory {
		public override DbConnection CreateConnection(string cs) {
			DbConnection c = new SqlConnection(cs);
			c.Open();
			return c;
		}
		public override DbCommand CreateCommand(DbConnection conn) {
			DbCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			return cmd;
		}
		public override DbParameter AddParameter(DbCommand cmd, string name, object val) {
			DbParameter r = new SqlParameter();
			r.ParameterName = "@" + name;
			r.Direction = ParameterDirection.Input;
			r.Value = val;
			cmd.Parameters.Add(r);
			return r;
		}
	}

	internal class PGFactory : ADOFactory {
		public override DbConnection CreateConnection(string cs) {
			DbConnection c = new NpgsqlConnection(cs);
			c.Open();
			return c;
		}
		public override DbCommand CreateCommand(DbConnection conn) {
			DbCommand cmd = new NpgsqlCommand();
			cmd.Connection = conn;
			return cmd;
		}
		public override DbParameter AddParameter(DbCommand cmd, string name, object val) {
			System.Data.Common.DbParameter r = new NpgsqlParameter();
			r.ParameterName = "@" + name;
			r.Direction = ParameterDirection.Input;
			r.Value = val;
			cmd.Parameters.Add(r);
			return r;
		}
	}
}

