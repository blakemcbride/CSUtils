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


#define USE_POSTGRESQL     //  define if using PostgreSQL
//#define MYSQL_EXTERNAL     //  define if MySQL connection is externally supplied

using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

#if USE_POSTGRESQL
using Npgsql;
#endif

// Author: Blake McBride

// This code does largely the same thing as DbProviderFactory but doesn't require foreign data provider registration.
namespace CSUtils {
    
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
            return conn.CreateCommand();
        }

        public override DbParameter AddParameter(DbCommand cmd, string name, object val) {
            DbParameter r = new SqlParameter();
            r.ParameterName = "@" + name;
            r.Direction = ParameterDirection.Input;
            r.Value = val;
            if (cmd.Parameters.Contains(r.ParameterName))
                cmd.Parameters.RemoveAt(r.ParameterName);
            cmd.Parameters.Add(r);
            return r;
        }
    }

#if USE_POSTGRESQL
    	internal class PGFactory : ADOFactory {
    		public override DbConnection CreateConnection(string cs) {
    			DbConnection c = new NpgsqlConnection(cs);
    			c.Open();
    			return c;
    		}
    		public override DbCommand CreateCommand(DbConnection conn) {
              return conn.CreateCommand();
    		}
    		public override DbParameter AddParameter(DbCommand cmd, string name, object val) {
    			DbParameter r = new NpgsqlParameter();
    			r.ParameterName = "@" + name;
    			r.Direction = ParameterDirection.Input;
    			r.Value = val;
    		    if (cmd.Parameters.Contains(r.ParameterName))
    		        cmd.Parameters.RemoveAt(r.ParameterName);
    		    cmd.Parameters.Add(r);
    			return r;
    		}
    	}
#endif

    internal class MySQLFactory : ADOFactory
    {
        public override DbConnection CreateConnection(string cs) {
            /*
             * Only taken out when no convinient MySql reference.  This method is never called anyway when there is an externally supplied connection.
             */
#if !MYSQL_EXTERNAL
            DbConnection c = new MySql.Data.MySqlClient.MySqlConnection(cs);
            c.Open();
            return c;
#else
            return null;
#endif
        }

        public override DbCommand CreateCommand(DbConnection conn) {
            return conn.CreateCommand();
        }

        public override DbParameter AddParameter(DbCommand cmd, string name, object val) {
            DbParameter r = cmd.CreateParameter();
            r.ParameterName = "@" + name;
            r.Direction = ParameterDirection.Input;
            r.Value = val;
            if (cmd.Parameters.Contains(r.ParameterName))
                cmd.Parameters.RemoveAt(r.ParameterName);
            cmd.Parameters.Add(r);
            return r;
        }
    }
}

