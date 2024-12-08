using System;
using System.Data.Common;
using System.Data.SQLite;
using static RjProduction.Sql.ISqlProfile;

namespace RjProduction.Sql
{
    public class SqliteProfile : ISqlProfile
    {
        private readonly SQLiteConnection SQLite = new();
        ISqlProfile.TypeSqlConnection ISqlProfile.SqlType => ISqlProfile.TypeSqlConnection.Sqlite;

        public string QuotSql(string str) => $"[{str}]";

        /// <summary>
        /// Локальное размещение папки для базы данных
        /// </summary>
        public string LocalDir { get; set; } = AppContext.BaseDirectory + "Data\\";
        /// <summary>
        /// База данных по умолчанию
        /// </summary>
        public string DataBase { get; set; } = "DBsqlite";

        public bool ConnectIs => SQLite.State == System.Data.ConnectionState.Open;

        public ISqlProfile.FieldSql[] GetDate(int ID, string TabelName)
        {
            if (ID == -1) { throw new Exception("ID = -1 not exist id"); }            

            List<FieldSql> ls = [];
            SQLiteCommand command = new($"SELECT * FROM [{TabelName}] WHERE ID={ID}", SQLite);
            SQLiteDataReader sqReader = command.ExecuteReader();
            if (sqReader.HasRows)
            {
                using (sqReader)
                {
                    sqReader.Read();
                    for (int i = 0; i < sqReader.FieldCount; i++)
                    {
                        ls.Add(new ISqlProfile.FieldSql(sqReader.GetName(i), sqReader.GetFieldType(i).Name, sqReader.GetValue(i).ToString() ?? string.Empty));
                    }
                }
            }
            return [.. ls];
        }

        public bool ExistTabel(string tabelName)
        {
            bool d;
            if (ConnectIs == false) throw new Exception("Не выполнено подключение к бд");
            SQLiteCommand command = new("SELECT name FROM sqlite_master WHERE type='table' AND name='" + tabelName + "'", SQLite);
            using (SQLiteDataReader sqReader = command.ExecuteReader())
            {
                sqReader.Read();
                d = sqReader.HasRows;
                sqReader.Close();
            }
            return d;
        }

        public string TypeSQL(string info) => info switch
        {
            "Decimal" => "NUMERIC",
            "Int16" => "INTEGER",
            "Int32" => "INTEGER",
            "Short" => "INTEGER",
            "UInt32"=> "REAL",
            "Int64" => "REAL",
            "Fload" => "REAL",
            "Double" => "REAL",
            "KEY_ID" => "INTEGER primary key AUTOINCREMENT",
            _ => "TEXT"
        };

        public long SqlCommand(string sql)
        {
            if (ConnectIs == false) throw new Exception("Не выполнено подключение к бд");
            SQLiteCommand command = SQLite.CreateCommand();
            command.CommandText = sql + "; SELECT last_insert_rowid();";
#if DEBUG
            System.Diagnostics.Debug.WriteLine("SqlCommand:" + DateTime.Now.ToString() + " " + sql);
#endif            
            return (long)command.ExecuteScalar();
        }

        public void Disconnect() => SQLite.Close();

        public void Conection()
        {
            string sFile = LocalDir + DataBase + ".db";
            SQLiteConnectionStringBuilder stringBuilder = new() { DataSource = sFile + "" };
            SQLite.ConnectionString = stringBuilder.ToString();
            try
            {
                SQLite.Open();
            }
            catch
            {
                throw;
            }
        }



    }
}
