
using System.Reflection;

namespace RjProduction.Sql
{
    public static class SqlRequest
    {
        public static void CreateTabel<T>(ISqlProfile sqlProfile) where T : SqlParam
        {
            T obj = Activator.CreateInstance<T>();

            string str = "CREATE TABLE " + sqlProfile.QuotSql(obj.TabelName) + " (";
            foreach (var item in sqlProfile.TitleDB(obj)) str += $"{sqlProfile.QuotSql(item.Item1)} {item.Item2}, ";
            str = str[0..^2] + ") ";

            sqlProfile.Conection();            
            try
            {
                if (sqlProfile.ExistTabel("Premision") == false)
                {
                    //необходимо для блокировки записей
                    string sql = "CREATE TABLE IF NOT EXISTS Premision (" +
                                    "id_user INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
                                    "user CHAR(100) NOT NULL," +
                                    "timeout REAL NOT NULL," +
                                    "tabel_name STRING NOT NULL," +
                                    "index_id INTEGER NOT NULL)";
                    sqlProfile.SqlCommand(sql);
                }
                if (sqlProfile.ExistTabel(obj.TabelName)==false) sqlProfile.SqlCommand(str);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                sqlProfile.Disconnect();
            }
        }

        public static T ReadData<T>(ISqlProfile sqlProfile, int id) where T : SqlParam
        {
            if (id == -1) throw new Exception("Неверный id для поиска строки");
            T obj;

            sqlProfile.Conection();
            try
            {
                obj = (T)Integrator(Activator.CreateInstance<T>(), id, sqlProfile);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                sqlProfile.Disconnect();
            }

            return obj;
        }

        public static void SetData(ISqlProfile sqlProfile, SqlParam objA) 
        {
            sqlProfile.Conection();
            long IDField_lite = -1;

            long cROW(SqlParam _obj)
            {
                long IDField;

                if (_obj.ID == -1)          //INSERT INTO
                {
                    string str = "";
                    string values = "";
                    foreach (var item in sqlProfile.TitleDB(_obj))
                    {
                        if (item.Item1 == "ID") continue;       // Создаеться автоматически id
                        str += sqlProfile.QuotSql(item.Item1) + ", ";
                        PropertyInfo? refl = _obj.GetType().GetProperty(item.Item1);
                        if (refl == null) continue;

                        if (refl.PropertyType.BaseType?.Name != nameof(SqlParam)) {
                            values += "'" + CastType(refl.GetValue(_obj) ?? "", refl) + "', "; 
                        }
                        else
                        {
                            if (refl.GetValue(_obj) is SqlParam sql)
                            {
                                if (sql.ID != -1) values += "'" + sql.ID + "', ";
                                else values += $"'{cROW(sql)}', ";
                            }
                        }
                    }

                    str = str[0..^2];
                    values = values[0..^2];

                    IDField = sqlProfile.SqlCommand($"INSERT INTO  {sqlProfile.QuotSql(_obj.TabelName)} ({str}) VALUES ({values})");
                    var fi = _obj.GetType().GetField("IDField", BindingFlags.NonPublic | BindingFlags.Instance);
                    fi?.SetValue(_obj, IDField);
                }
                else                    //Update
                {
                    if (!string.IsNullOrEmpty(_obj.LockInfo))
                    {
                        throw new Exception("заблокированна");
                    }

                    string unityValue = "";

                    foreach (var item in sqlProfile.TitleDB(_obj))
                    {
                        if (item.Item1 == "ID") continue;       // Создаеться автоматически id
                        PropertyInfo? refl = _obj.GetType().GetProperty(item.Item1);
                        if (refl == null) continue;

                        if (refl.PropertyType.BaseType?.Name != nameof(SqlParam))
                        {
                            unityValue += sqlProfile.QuotSql(item.Item1) + "='" + CastType(refl.GetValue(_obj) ?? "", refl) + "', ";
                        }
                        else            // here class
                        {
                            if (refl.GetValue(_obj) is SqlParam sql)
                            {
                                if (sql.ID != -1) unityValue += $"{sqlProfile.QuotSql(item.Item1)}='{sql.ID}', ";
                                else { unityValue += $"{sqlProfile.QuotSql(item.Item1)}='{cROW(sql)}', "; }
                            }
                        }
                    }

                    unityValue = unityValue[0..^2];
                    IDField = _obj.ID;
                    sqlProfile.SqlCommand($"UPDATE {sqlProfile.QuotSql(_obj.TabelName)} SET {unityValue} WHERE ID = {_obj.ID} ");
                }
                return IDField;
            }

            try
            {
                IDField_lite = cROW(objA);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                sqlProfile.Disconnect();
            }
           // return IDField_lite;
        }

        /// <summary>
        /// Converts to the desired type.
        /// </summary>
        /// <param name="obj">obtained from GetValue</param>
        /// <param name="info">this.GetType().GetProperty(item.Key)</param>
        /// <returns>string value ready for the database</returns>
        /// <remarks>For some special cases, you should specify a conversion to the string type</remarks>
        private static string CastType(object obj, System.Reflection.PropertyInfo info)
        {
            string value = "";
            if (obj == null) return value;

            if (obj.GetType().IsEnum) { value = ((int)obj).ToString(); }
            else if (info.PropertyType.Name == "Boolean") { value = obj.ToString() == "True" ? "1" : "0"; }
            else if (info.PropertyType.Name == "Color") { value = ((System.Drawing.Color)obj).Name; }
            else { value = obj.ToString() ?? ""; }
            value = value.Replace("'", @"\'");

            return value;
        }

        private static object Integrator(object obj, int id, ISqlProfile sqlProfile)
        {
            var tmp = sqlProfile.GetDate(id, ((SqlParam)obj).TabelName);

            // Назначение ID
            var fi = obj.GetType().GetField("IDField", BindingFlags.NonPublic | BindingFlags.Instance);
            fi?.SetValue(obj, Int32.Parse(tmp.First(x => x.NameField == "ID").Value));

            foreach (var item in obj.GetType().GetProperties().Where(x => x.CanWrite))
            {
                SqlParam? param = item.GetValue(obj) as SqlParam;

                if (param is not null)
                {
                    string str = tmp.First(x => x.NameField == item.Name ).Value;
                    object? dbClass = Activator.CreateInstance(item.PropertyType);
                    if (dbClass != null) item.SetValue(obj, Integrator(dbClass, int.Parse(str), sqlProfile));
                }
                else
                {
                    string str = tmp.First(x => x.NameField == item.Name).Value;
                    // in special cases you need to add data conversion manually
                    if (item.PropertyType.IsEnum) { item.SetValue(obj, Enum.Parse(item.PropertyType, str)); }
                    else if (item.PropertyType.Name == "Color") { item.SetValue(obj, System.Drawing.Color.FromName(str)); }
                    else if (item.PropertyType.Name == "Boolean") { item.SetValue(obj, str != "0"); }
                    else if (item.PropertyType.Name == "Int64") { item.SetValue(obj, Int64.Parse(str)); }
                    else if (item.PropertyType.Name == "Int16") { item.SetValue(obj, Int16.Parse(str)); }
                    else if (item.PropertyType.Name == "Int32") { item.SetValue(obj, Int32.Parse(str)); }
                    else if (item.PropertyType.Name == "String") { item.SetValue(obj, str); }
                    //else item.SetValue(obj, str);
                }
            }
            return obj;

        }

       



    }
}
