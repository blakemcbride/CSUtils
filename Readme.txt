
ConnectionType.PostgreSQL
ConnectionType.MicrosoftServer

Connection conn = new Connection(type, host, db, user, pw)
Connection conn = new Connection(type, host, db)
Connection conn = new Connection(type, connectionString)

conn.BeginTransaction()
conn.Commit()
conn.Rollback()

conn.Execute(sql)

Record rec = NewRecord(typeof(MyTable))
Record rec = NewRecord(TABLE.TABLE_NAME)

conn.Close()

----------------------------------------------------------------------

Command cmd = conn.NewCommand()

cmd.AddParameter(name, val)
cmd.ClearParameters()

Cursor cursor = cmd.Execute()  // re-execute previous sql with new parameters
Cursor cursor = cmd.Execute(typeof(MyTable), sql)
Cursor cursor = cmd.Execute(sql)
n = cmd.ExecuteNonQuery()
n = cmd.ExecuteNonQuery(sql)

cmd.Close()

----------------------------------------------------------------------

bool cursor.Next()
bool cursor.NextUnbuffered()

cursor[TABLE.FIELDNAME]
cursor[TABLE.FIELDNAME] = ...

Dictionary rec = cursor.FetchOne()
List<Dictionary>> recs = cursor.FetchAll()

cursor.Update()
cursor.Delete()

cursor.Close()

----------------------------------------------------------------------

rec = new Record(conn, typeof(MyTable))
rec = new Record(conn, TABLE.TABLE_NAME)

cursor[TABLE.FIELDNAME]
cursor[TABLE.FIELDNAME] = ...

rec.AddRecord()

rec.Clear()

rec.Close()

----------------------------------------------------------------------

