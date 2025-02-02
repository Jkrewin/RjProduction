
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using static RjProduction.Sql.ISqlProfile;

namespace RjProduction.Sql
{
    /// <summary>
    /// Sqlite профиль подключения работает с System.Data.SQLite.Core 1.0.119
    /// The official SQLite database engine for both x86 and x64 along with the ADO.NET provider.
    /// </summary>
    public class SqliteProfile : ISqlProfile
    {
        private SQLiteTransaction? SqlTransaction;
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
        // <summary>
        /// Строка последнего запроса для поиска ошибок
        /// </summary>
        public string SqlLogString { get; set; } = string.Empty;


        public string QuotSql(string str) => $"[{str}]";

        public void Transaction(TypeTransaction transaction)
        {
            if (transaction == TypeTransaction.commit) SqlTransaction!.Commit();
            else if (transaction == TypeTransaction.roolback) SqlTransaction!.Rollback();
        }

        public ISqlProfile.FieldSql[] GetFieldSql(string where, string TabelName)
        {

            List<FieldSql> ls = [];
            SqlLogString = $"SELECT * FROM [{TabelName}] WHERE {where} LIMIT 1";
            SQLiteCommand command = new(SqlLogString, SQLite);
            if (SqlTransaction is not null) command.Transaction = SqlTransaction;
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
        public ISqlProfile.FieldSql[] GetFieldSql(long ID, string TabelName)
        {
            return GetFieldSql($"ID = {ID}", TabelName);
        }
        public List<FieldSql[]> GetFieldSql(string where, string tabelName, string select = "*")
        {
            List<FieldSql[]> ls = [];
            SqlLogString = $"SELECT {select} FROM [{tabelName}] WHERE {where}";
            SQLiteCommand command = new(SqlLogString, SQLite);
            if (SqlTransaction is not null) command.Transaction = SqlTransaction;
            using (SQLiteDataReader sqReader = command.ExecuteReader())
            {
                while (sqReader.Read())
                {
                    FieldSql[] obj = new FieldSql[sqReader.FieldCount];
                    for (global::System.Int32 i = sqReader.FieldCount - 1; i >= 0; i--)
                    {
                        obj[i] = new ISqlProfile.FieldSql(sqReader.GetName(i), sqReader.GetFieldType(i).Name, sqReader.GetValue(i).ToString() ?? string.Empty);
                    }
                    ls.Add(obj);
                }
            }
            return ls;
        }


        public bool ExistTabel(string tabelName)
        {
            bool d;
            if (ConnectIs == false) throw new Exception("Не выполнено подключение к бд");
            SqlLogString = "SELECT name FROM sqlite_master WHERE type='table' AND name='" + tabelName + "'";
            SQLiteCommand command = new(SqlLogString, SQLite);
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
            "UInt32" => "REAL",
            "Int64" => "REAL",
            "Float" => "REAL",
            "Double" => "TEXT",
            "KEY_ID" => "INTEGER primary key AUTOINCREMENT",
            _ => "TEXT"
        };
        public long SqlCommand(string sql)
        {
            SQLiteCommand command = SQLite.CreateCommand();
            if (SqlTransaction is not null) command.Transaction = SqlTransaction;
            command.CommandText = sql + "; SELECT last_insert_rowid();";
            SqlLogString = "SqlCommand:" + DateTime.Now.ToString() + " " + sql;
            return (long)command.ExecuteScalar();
        }

        public object? AdapterSql(string tabelName, string nameField, string where = "")
        {
            string pol;
            if (where != "")
            {
                pol = $"SELECT {nameField} FROM [{tabelName}] WHERE " + where + " LIMIT 1";
            }
            else
            {
                pol = $"SELECT {nameField} FROM [{tabelName}]" + " LIMIT 1";
            }

            SqlLogString = pol;
            using SQLiteCommand command = new(pol, SQLite);
            if (SqlTransaction is not null) command.Transaction = SqlTransaction;
            return command.ExecuteScalar();
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
            SqlLogString = pol;
            List<object[]> ls = [];
            SQLiteCommand command = new(pol, SQLite);
            if (SqlTransaction is not null) command.Transaction = SqlTransaction;
            using (SQLiteDataReader sqReader = command.ExecuteReader())
            {
                while (sqReader.Read())
                {
                    object[] obj = new object[sqReader.FieldCount];
                    for (global::System.Int32 i = sqReader.FieldCount - 1; i >= 0; i--)
                    {
                        if (sqReader.GetName(i) == "ID") id = (long)sqReader.GetValue(i);
                        obj[i] = sqReader.GetValue(i);
                    }
                    ls.Add(obj);
                }
            }
            return ls;
        }
        public DataTable GetDataTable(string tabelName, string select_sql = "*", string where_sql = "")
        {
            DataTable dataTable = new();
            try
            {
                SQLiteCommand cmd = SQLite.CreateCommand();
                if (where_sql != "") where_sql = " WHERE " + where_sql;
                cmd.CommandText = $"SELECT {select_sql} FROM {tabelName}{where_sql}";
                SqlLogString = cmd.CommandText;

                using SQLiteDataAdapter dataAdapter = new(cmd.CommandText, SQLite);
                dataAdapter.Fill(dataTable);
            }
            catch
            {
                throw;
            }
            return dataTable;
        }

        public void Disconnect()
        {
            SqlTransaction?.Dispose();
            SQLite.Close();
        }
        public void Conection(bool startTransaction = false)
        {
            string sFile = LocalDir + DataBaseFile + ".db";
            SQLiteConnectionStringBuilder stringBuilder = new() { DataSource = sFile + "" };
            SQLite.ConnectionString = stringBuilder.ToString();

            if (File.Exists(sFile) == false)
            {
                MessageBox.Show("Необходимо перезапусить профиль подключения. Так как путь к базе данных не найден.");
                MDL.LogError("БД не не найдена.", "Указанный путь " + sFile);
            }

            try
            {
                SQLite.Open();
                if (startTransaction) SqlTransaction = SQLite.BeginTransaction();
                else SqlTransaction = null;
            }
            catch
            {
                throw;
            }
        }


#pragma warning disable CA1816
        public void Dispose()
        {
            // В исходном коде Sqlite уже есть проверка на Dispose повторного вызова
            SQLite.Dispose();
#pragma warning restore CA1816
        }

    }
}
