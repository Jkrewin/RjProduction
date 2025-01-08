
using System.Data;
using System.Data.SQLite;
using static RjProduction.Sql.ISqlProfile;

namespace RjProduction.Sql
{
    public class SqliteProfile : ISqlProfile
    {
        private readonly SQLiteConnection SQLite = new();
        public TypeSqlConnection SqlType => ISqlProfile.TypeSqlConnection.Sqlite;

        /// <summary>
        /// Локальное размещение папки для базы данных
        /// </summary>
        public string LocalDir { get; set; } = AppContext.BaseDirectory + "Data\\";
        /// <summary>
        /// База данных по умолчанию
        /// </summary>
        public string DataBaseFile { get; set; } = "DBsqlite";
        /// <summary>
        /// Есть коннект или нет
        /// </summary>
        public bool ConnectIs => SQLite.State == System.Data.ConnectionState.Open;

        public string QuotSql(string str) => $"[{str}]";

        public ISqlProfile.FieldSql[] GetDate(string where, string TabelName) {
           
            List<FieldSql> ls = [];
            SQLiteCommand command = new($"SELECT * FROM [{TabelName}] WHERE {where}", SQLite);
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
        public ISqlProfile.FieldSql[] GetDate(long ID, string TabelName)      
        {
            return GetDate($"ID = { ID }", TabelName);
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
            "Decimal" => "TEXT",
            "Int16" => "INTEGER",
            "Int32" => "INTEGER",
            "Short" => "INTEGER",
            "UInt32"=> "REAL",
            "Int64" => "REAL",
            "Float" => "REAL",
            "Double" => "TEXT",
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

        public List<FieldSql> GetDataFieldSql(string tabelName, string where, string _select ="*") {
            if (ConnectIs == false) throw new Exception("Не выполнено подключение к бд");
            string pol = $"SELECT {_select} FROM [{tabelName}] WHERE {where} LIMIT 1";
            List<FieldSql> ls = [];

            SQLiteCommand command = new(pol, SQLite);
            using (SQLiteDataReader sqReader = command.ExecuteReader())
            {
                while (sqReader.Read())
                {
                    for (global::System.Int32 i = sqReader.FieldCount - 1; i >= 0; i--)
                    {                       
                        ls.Add(new FieldSql(sqReader.GetName(i), sqReader.GetDataTypeName(i), sqReader.GetValue(i).ToString()?? string.Empty));
                    }
                }
            }
            return ls;
        }

        public object? AdapterSql(string tabelName, FieldSql selectField, string where = "") {
            if (ConnectIs == false) throw new Exception("Не выполнено подключение к бд");
            string pol;
            if (where != "")
            {
                pol = $"SELECT * FROM [{tabelName}] WHERE " + where;
            }
            else
            {
                pol = $"SELECT * FROM [{tabelName}]";
            }

            SQLiteCommand command = new(pol, SQLite);
            using (SQLiteDataReader sqReader = command.ExecuteReader())
            {
                while (sqReader.Read())
                {                    
                    for (global::System.Int32 i = sqReader.FieldCount - 1; i >= 0; i--)
                    {
                        if (sqReader.GetName(i) == selectField.NameField) return sqReader.GetValue(i);                        
                    }                   
                }
            }
            return null;
        }

        public List<object[]> AdapterSql(string tabelName, out long id, string where = "")
        {
            id = -1;
            string pol;
            if (where != "")
            {
                pol = $"SELECT * FROM [{tabelName}] WHERE " + where;
            }
            else
            {
                pol = $"SELECT * FROM [{tabelName}]";
            }
            List<object[]> ls = [];
            SQLiteCommand command = new(pol, SQLite);
            using (SQLiteDataReader sqReader = command.ExecuteReader())
            {
                while (sqReader.Read())
                {
                    object[] obj = new object[sqReader.FieldCount];
                    for (global::System.Int32 i = sqReader.FieldCount-1; i >= 0; i--)
                    {
                        if (sqReader.GetName(i)=="ID") id = (long)sqReader.GetValue(i);
                         obj[i] = sqReader.GetValue(i);
                    }
                    ls.Add(obj);
                }
            }
            return ls;
        }
        public DataTable GetDataTable(string tabelName, string select_sql="*", string where_sql="") {
            SQLiteCommand cmd = SQLite.CreateCommand();
            if (where_sql != "") where_sql = " WHERE " + where_sql;
            cmd.CommandText = $"SELECT {select_sql} FROM {tabelName}{where_sql}";

            DataTable dataTable = new ();
            using (SQLiteDataAdapter dataAdapter = new(cmd.CommandText, SQLite)) dataAdapter.Fill(dataTable);
            return dataTable;
        }       

        public void Disconnect() => SQLite.Close();
        public void Conection()
        {
            string sFile = LocalDir + DataBaseFile + ".db";
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
