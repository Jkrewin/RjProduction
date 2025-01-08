
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using static RjProduction.Sql.ISqlProfile;

namespace RjProduction.Sql
{
    public static class SqlRequest
    {
        public static void CreateTabel<T>() where T : SqlParam
        {
            ISqlProfile sqlProfile = MDL.SqlProfile!;
            if (MDL.SqlProfile is null) throw new Exception("Профиль подключения не выбран " + nameof (MDL.SqlProfile));
            T obj = Activator.CreateInstance<T>();

            string str = "CREATE TABLE " + sqlProfile.QuotSql(obj.TabelName) + " (";
            foreach (var item in sqlProfile.TitleDB(obj)) str += $"{sqlProfile.QuotSql(item.Item1)} {item.Item2}, ";
            str = str[0..^2] + ") ";

            sqlProfile.Conection();            
            try
            {               
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
        /// <summary>
        /// Структурный таблици, необходимые при старте программы 
        /// </summary>
        public static void CreateStartBaseTabel() 
        {
            ISqlProfile sqlProfile = MDL.SqlProfile!;
            if (MDL.SqlProfile is null) throw new Exception("Профиль подключения не выбран " + nameof(MDL.SqlProfile));
                     
            sqlProfile.Conection();
            try
            {
                //необходимо для блокировки записей
                string sql = "CREATE TABLE IF NOT EXISTS Premision (" +
                                "id_user INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
                                "user CHAR(100) NOT NULL," +
                                "timeout TEXT NOT NULL," +
                                "tabel_name STRING NOT NULL," +
                                "index_id INTEGER NOT NULL)";
                MDL.SqlProfile!.SqlCommand(sql);
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

        /// <summary>
        /// Проверка на существание записи. Тип необязательно указывать тут
        /// </summary>
        /// <typeparam name="T">SqlParam только</typeparam>
        /// <param name="field">не обязательно указывать тип FieldSql только название и значение</param>
        /// <returns>-1 - не существует </returns>
        public static long ExistRecord<T>(FieldSql field ) where T : SqlParam {
            ISqlProfile sqlProfile = MDL.SqlProfile!;
            
            if (MDL.SqlProfile is null) throw new Exception("Профиль подключения не выбран " + nameof(MDL.SqlProfile));
            T obj = Activator.CreateInstance<T>();
            sqlProfile.Conection();
            try
            {
                var ls = sqlProfile.AdapterSql(obj.TabelName, out long id, $"{field.NameField}='{field.Value}'");
                if (ls.Count == 0) return -1;
                else return id;
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

        public static T ReadData<T>(long id, bool _lock = false) where T : SqlParam
        {
            ISqlProfile sqlProfile = MDL.SqlProfile!;
            if (MDL.SqlProfile is null) throw new Exception("Профиль подключения не выбран " + nameof(MDL.SqlProfile));
            if (id == -1) throw new Exception("Неверный id для поиска строки");
            T obj;

            sqlProfile.Conection();
            try
            {
                obj = (T)Integrator(Activator.CreateInstance<T>(), id, _lock);
            }
            catch
            {
                throw;
            }
            finally
            {
                sqlProfile.Disconnect();
            }

            return obj;
        }

        public static T? ReadData<T>(FieldSql field, bool _lock = false) where T : SqlParam => ReadData<T>([field], _lock);

        public static T? ReadData<T>(FieldSql[] field, bool _lock = false) where T : SqlParam
        {
            ISqlProfile sqlProfile = MDL.SqlProfile!;
            if (MDL.SqlProfile is null) throw new Exception("Профиль подключения не выбран " + nameof(MDL.SqlProfile));
                        
            T obj= Activator.CreateInstance<T>();
            sqlProfile.Conection();
            try
            {
                string deep = "";
                foreach (FieldSql fieldSql in field) deep += $"{fieldSql.NameField}='{fieldSql.Value}' AND ";
                var ls = sqlProfile.AdapterSql(obj.TabelName, out long id, deep[..^5]);
                if (ls.Count == 0) return null;
                obj = (T)Integrator(obj, id, _lock);
            }
            catch 
            {
                throw;
            }
            finally
            {
                sqlProfile.Disconnect();
            }
           
            return obj;
        }

        /// <summary>
        /// Создает или изменяет данные в БД. <b>objA - внем будет обновлен ID если он -1 создан в БД</b>
        /// </summary>
        /// <param name="objA">Класс наследник SqlParam. ID будет обновленое если оно равно -1 и будет равно текущему в БД</param>
        /// <returns>id созданной или обновленной строки</returns>
        public static long SetData(SqlParam objA) => SetData([objA]);

        public static long SetData( SqlParam[] objA) 
        {
            ISqlProfile sqlProfile = MDL.SqlProfile!;
            if (MDL.SqlProfile is null) throw new Exception("Профиль подключения не выбран " + nameof(MDL.SqlProfile));

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
                        if (item.Item1 == nameof(_obj.LockInfo)) continue;  // Управление осущ. извне
                        str += sqlProfile.QuotSql(item.Item1) + ", ";
                        PropertyInfo? refl = _obj.GetType().GetProperty(item.Item1);                       
                        if (refl == null) continue;
                            if (refl.GetCustomAttributes(true).Any(x => x is SqlIgnore)) continue;

                        if (refl.PropertyType.BaseType?.Name != nameof(SqlParam)) {
                            values += "'" + CastType(refl.GetValue(_obj) ?? "", refl) + "', "; 
                        }
                        else
                        {
                            var ttt = refl.GetValue(_obj);
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
                    if (_obj.LockInfo)
                    {
                        if (IsLock(_obj))
                                throw new Exception("Запись " + _obj.TabelName  + " id:" + _obj.ID + " заблокированна другим пользователем id:"+ _obj.LockInfo);
                    }

                    string unityValue = "";

                    foreach (var item in sqlProfile.TitleDB(_obj))
                    {
                        if (item.Item1 == "ID") continue;       // Создаеться автоматически id
                        if (item.Item1 == nameof(_obj.LockInfo)) continue;  // Управление осущ. извне
                        PropertyInfo? refl = _obj.GetType().GetProperty(item.Item1);
                        if (refl == null) continue;                       
                        if (refl.GetCustomAttributes(true).Any(x => x is SqlIgnore)) continue; 
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
                foreach (var item in objA)
                {
                    if (item is null) continue;
                    IDField_lite = cROW(item);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                sqlProfile.Disconnect();
            }
            return IDField_lite;
        }

        public static DataTable? GetDataTable(string tabel_name, string select_sql = "*", string where_sql = "") {
            ISqlProfile sqlProfile = MDL.SqlProfile!;
            if (MDL.SqlProfile is null) throw new Exception("Профиль подключения не выбран " + nameof(MDL.SqlProfile));
            DataTable? dt;
            try
            {
                sqlProfile.Conection();
                dt = sqlProfile.GetDataTable(tabel_name, select_sql, where_sql);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                sqlProfile.Disconnect();
            }
            return dt;
        }

        /// <summary>
        /// Запрос на плюс или минус значение, к общим остаткам. Помогоает избежать ошибку конкурентного доступа без монопольго режима 
        /// </summary>
        /// <param name="obj">Класс который нужно проверить</param>
        /// /// <param name="changes_field">Какое поле нужно изменить и значение </param>
        /// <returns>true - успешный запрос. False - такой строки не существует, id = -1. Число отрицательное нулю, changes_field или значение в бд не соответствует  OperatonEnum</returns>
        public static bool ConcurrentReqest(SqlParam obj, FieldSql changes_field, OperatonEnum operaton ) 
        {
          return  ConcurrentReqest(obj.TabelName, obj.ID, changes_field, operaton);
        }

        /// <summary>
        /// Запрос на плюс или минус значение, к общим остаткам. Помогоает избежать ошибку конкурентного доступа без монопольго режима 
        /// </summary>
        /// <param name="tabelName">Название таблици</param>
        /// <param name="id">id строки</param>
        /// /// <param name="changes_field">Какое поле нужно изменить и значение </param>
        /// <returns>true - успешный запрос. False - такой строки не существует, id = -1. Число отрицательное нулю, changes_field или значение в бд не соответствует  OperatonEnum</returns>
        public static bool ConcurrentReqest(string tabelName, long id, FieldSql changes_field, OperatonEnum operaton)
        {
            ISqlProfile sqlProfile = MDL.SqlProfile!;
            if (MDL.SqlProfile is null) throw new Exception("Профиль подключения не выбран " + nameof(MDL.SqlProfile));
            if (id == -1) return false;

            sqlProfile.Conection();
            try
            {
                var po = sqlProfile.AdapterSql(tabelName, changes_field, $"ID = {id}");
                if (po == null) return false;
                // проверка полей
                bool op = double.TryParse(po.ToString(), out double d_db);
                bool opt = double.TryParse(changes_field.Value, out double d_val);
                if (op == false || opt == false) return false;
                // совершение операции
                string result = "";
                switch (operaton)
                {
                    case OperatonEnum.vsPlus:
                        result = (d_db + d_val).ToString();
                        break;
                    case OperatonEnum.vsMunis:
                        var tt = d_db - d_val;
                        if (tt < 0) return false;
                        result = tt.ToString();
                        break;
                    case OperatonEnum.vsMutation:
                        result = changes_field.Value;
                        break;
                }
                sqlProfile.SqlCommand($"UPDATE {sqlProfile.QuotSql(tabelName)} SET {changes_field.NameField} = '{result}' WHERE ID = {id} ");
            }
            catch
            {
                throw;
            }
            finally
            {
                sqlProfile.Disconnect();
            }
            return false;
        }


        private static bool IsLock(SqlParam obj)
        {
            var k = MDL.SqlProfile!.GetDataFieldSql(obj.TabelName, $"ID={obj.ID}", nameof(obj.LockInfo) + ", datetime('now') as now").ToArray();
            DateTime start = DateTime.Parse(ISqlProfile.FieldSql.ValueFieldSql(k, "LockInfo"));
            DateTime now = DateTime.Parse(ISqlProfile.FieldSql.ValueFieldSql(k, "now")); // дата и время на сервере может быть другая, получаем от туда а не с текущего сервера  
            start = start.AddMinutes(SqlParam.TIMER_COUNT);
            if (now < start) return true;
            return false;
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

        private static object Integrator(object obj, long id, bool _lock)
        {
            ISqlProfile sqlProfile = MDL.SqlProfile!;
            var tmp = sqlProfile.GetDate(id, ((SqlParam)obj).TabelName);

            if (tmp.Length != 0)
            {
                // Length = 0 будет означать что не найдена строка но она будет как новая rebild 
                // Назначение ID
                var fi = obj.GetType().GetField("IDField", BindingFlags.NonPublic | BindingFlags.Instance);
                fi?.SetValue(obj, Int32.Parse(tmp.First(x => x.NameField == "ID").Value));
            }
            else if (tmp.Length == 0) return obj;

            foreach (var item in obj.GetType().GetProperties().Where(x => x.CanWrite))
            {
                //if (item is null) continue;
                if (item.GetCustomAttributes(true).Any(x => x is SqlIgnore)) continue;
                SqlParam? param = item.GetValue(obj) as SqlParam;
                               
                if (param is not null)
                {
                    // добавляем класс 
                    string str = tmp.First(x => x.NameField == item.Name ).Value;
                    object? dbClass = Activator.CreateInstance(item.PropertyType);
                    if (dbClass != null) item.SetValue(obj, Integrator(dbClass, int.Parse(str), false)); 
                }
                else
                {
                    string str = ISqlProfile.FieldSql.ValueFieldSql(tmp, item.Name);
                    string str2 = ISqlProfile.FieldSql.ValueFieldSql(tmp, nameof(param.LockInfo));
                    if (item.Name == nameof(param.LockInfo) & !string.IsNullOrEmpty(str2))
                    {
                        // заполнить информацию о блокировке ранее
                        var fi = obj.GetType().GetField("_LockInfo", BindingFlags.NonPublic | BindingFlags.Instance);
                        fi?.SetValue(obj, true);
                        //item.SetValue(obj, true);
                    }                   
                    else if (item.Name != nameof(param.LockInfo)) {
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
                    else if (item.Name == nameof(param.LockInfo) & string.IsNullOrEmpty(str2) & _lock == true)
                    {
                        // заблокировать сейчас
                        var fi = obj.GetType().GetField("_locker", BindingFlags.NonPublic | BindingFlags.Instance);
                        fi?.SetValue(obj, true);
                        item.SetValue(obj, true);
                    }
                }
            }
            return obj;

        }



        /// <summary>
        /// Операции
        /// </summary>
        public enum OperatonEnum
        {
            vsPlus, vsMunis, vsMutation
        }

    }
}
