
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using Utils;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
namespace ADOTest
{
	class Hospital {
		public const string TABLE_NAME = "hospital";
		public static List<string> PRIMARY_KEY = new List<string> { EHR_ID };
		public const string EHR_ID = "ehr_id";
		public const string HOSPITAL_NAME = "hospital_name";
	}
	class Secr_Lookup_ChasisType {
		public const string TABLE_NAME = "Secr_Lookup_ChasisType";
		public static List<string> PRIMARY_KEY = new List<string> { ID };
		public const string ID = "id";
		public const string CHASIS_DESCRIPTION = "chasis_description";
	}
	class Program {
		static void run_postgresql2() {
			Connection conn = new Connection(ConnectionType.PostgreSQL, "localhost", "mura", "postgres", "vryn#7796");
			try {
				Command cmd = conn.NewCommand();
				Cursor cursor = cmd.Execute(typeof(Hospital), "select * from hospital where ehr_id=@ehr_id");
				cmd.AddParameter("ehr_id", 1467);
				conn.BeginTransaction();
				while (cursor.Next()) {
					Console.WriteLine(cursor[Hospital.EHR_ID] + " " + cursor[Hospital.HOSPITAL_NAME]);
					cursor.Update();
					cursor[Hospital.HOSPITAL_NAME] = "abcd";
					cursor.Update();
					//cursor.Delete();
					Console.WriteLine(cursor[Hospital.EHR_ID] + " " + cursor[Hospital.HOSPITAL_NAME]);
				}
				conn.Commit();
				Record rec = new Record(conn, Hospital.TABLE_NAME);
				rec[Hospital.EHR_ID] = 44;
//				rec.AddRecord();
					
				///////////////////////////////////////////////////////////////////////////////////
				cmd.ClearParameters();
				cmd.AddParameter("ehr_id", 1425);
				cursor = cmd.Execute();
				while (cursor.Next())
					Console.WriteLine(cursor[Hospital.EHR_ID] + " " + cursor[Hospital.HOSPITAL_NAME]);
				
			} finally {
				if (conn != null)
					conn.Close();
			}
		}
		static void run_postgresql() {
			Connection conn = new Connection(ConnectionType.PostgreSQL, "localhost", "mura", "postgres", "vryn#7796");
			try {
				Command cmd = conn.NewCommand();
				Cursor cursor = cmd.Execute(typeof(Hospital), "select * from hospital");
				while (cursor.NextUnbuffered()) {
					Console.WriteLine(cursor[Hospital.EHR_ID] + " " + cursor[Hospital.HOSPITAL_NAME]);
				}
				cursor.Close();
			} finally {
				if (conn != null)
					conn.Close();
			}
		}
		static void run_microsoft() {
			Connection conn = new Connection(ConnectionType.MicrosoftServer, "AL001WSQLPdev01.us.chs.net", "CHSPortal", "compliance_portal_user", "SEcC0mp4$$");
			try {
				Command cmd = conn.NewCommand();
				Cursor cursor = cmd.Execute(typeof(Secr_Lookup_ChasisType), "select * from secr_lookup_chasistype");
				while (cursor.NextUnbuffered()) {
					Console.WriteLine(cursor[Secr_Lookup_ChasisType.ID] + " " + cursor[Secr_Lookup_ChasisType.CHASIS_DESCRIPTION]);
				}
				cursor.Close();
			} finally {
				if (conn != null)
					conn.Close();
			}
		}
		static void Main2(string [] args) {
//			run_microsoft();
			run_postgresql();
//			run_postgresql2();
		}
			
	}
}