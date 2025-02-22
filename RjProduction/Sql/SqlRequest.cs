
using RjProduction.XML;
using System.Data;
using System.Reflection;
using static RjProduction.Sql.ISqlProfile;

namespace RjProduction.Sql
{
    public static class SqlRequest
    {
        public static void CreateTabel<T>() where T : SqlParam
        {
            if (MDL.SqlProfile is null) throw new Exception("Профиль подключения не выбран " + nameof(MDL.SqlProfile));
          
            MDL.SqlProfile.Conection();
            try
            {
                ForceCreateTabel<T>();
            }
            catch
            {
                throw;
            }
            finally
            {
                MDL.SqlProfile.Disconnect();
            }
        }

        /// <summary>
        /// Проверяет БД на совметимость версии программы
        /// </summary>
        /// <returns>true - совместима версия</returns>
        [DependentCode]
        public static bool TestingDB() {
            ISqlProfile sqlProfile = MDL.SqlProfile!;
            if (MDL.SqlProfile is null) throw new Exception("Профиль подключения не выбран " + nameof(MDL.SqlProfile));

            bool actor<T>()
            {
                T obj = Activator.CreateInstance<T>();
                if (obj is SqlParam sql)
                {
                    if (sqlProfile.ExistTabel(sql.TabelName))
                    {
                        List<(string, string)> direct = sqlProfile.TitleDB(sql);
                        foreach (var d in sqlProfile.GetFieldSql("ID>0", sql.TabelName))
                        {
                            if (direct.Any(x => x.Item1 == d.NameField) == false)
                            {
                                return false;
                            }
                        }
                    }
                    else return false;
                }
                else return false;

                return true;
            }

            sqlProfile.Conection();

            try
            {
                if (actor<DocArrival>() == false) return false;
                if (actor<DocMoving>() == false) return false;
                if (actor<DocShipments>() == false) return false;
                if (actor<Model.Products>() == false) return false;
                if (actor<Model.WarehouseClass>() == false) return false;
                if (actor<Model.DocRow>() == false) return false;
                if (actor<DocWritedowns>() == false) return false;
            }
            catch
            {
                throw;
            }
            finally
            {
                MDL.SqlProfile.Disconnect();
            }
            return true;
        }

        /// <summary>
        /// Структурный таблици, необходимые при старте программы 
        /// </summary>
        [DependentCode]
        public static void CreateStartBaseTabel()
        {           
            if (MDL.SqlProfile is null) throw new Exception("Профиль подключения не выбран " + nameof(MDL.SqlProfile));
            
            MDL.SqlProfile.Conection();

           
            ForceCreateTabel< DocArrival > ();
            try
            {
                ForceCreateTabel<DocWritedowns>();
                ForceCreateTabel<DocArrival>();
                ForceCreateTabel<DocMoving>();
                ForceCreateTabel<DocShipments>();
                ForceCreateTabel<Model.Products>();
                ForceCreateTabel<Model.WarehouseClass>();
                ForceCreateTabel<Model.DocRow>();
            }
            catch
            {
                throw;
            }
            finally
            {
                MDL.SqlProfile.Disconnect();
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
        /// <summary>
        /// Создать коллекцию данных
        /// </summary>
        /// <typeparam name="T">SqlParam только для</typeparam>
        /// <param name="tabelName">Название таблици</param>
        /// <param name="where">критерии поиска</param>
        /// <param name="_lock">блокировка (осторожно: можно заблокировать много записей !)</param>
        /// <returns></returns>
        public static List<T>? ReadСollection<T>(string tabelName, string where, bool _lock = false) where T : SqlParam
        {
            ISqlProfile sqlProfile = MDL.SqlProfile!;
            if (MDL.SqlProfile is null) throw new Exception("Профиль подключения не выбран " + nameof(MDL.SqlProfile));
            List<T> list = [];
            sqlProfile.Conection();
            try
            {
                var ls = sqlProfile.GetFieldSql(where, tabelName, "*");
                foreach (var item in ls)
                {
                    list.Add((T)Integrator(Activator.CreateInstance<T>()!, item, _lock));
                }
            }
            catch
            {
                throw;
            }
            finally { sqlProfile.Disconnect(); }

            return list;
        }
        /// <summary>
        /// Чтение данных
        /// </summary>
        /// <typeparam name="T">для SqlParam</typeparam>
        /// <param name="id">ID записи</param>
        /// <param name="_lock">Блокировка записи</param>
        /// <returns></returns>
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

            sqlProfile.Conection(true);
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
                        if (Prv_IsLock(_obj.ID,_obj.TabelName))
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
                sqlProfile.Transaction(TypeTransaction.roolback);
                throw;
            }
            finally
            {
                sqlProfile.Transaction(TypeTransaction.commit);
                sqlProfile.Disconnect();
            }
            return IDField_lite;
        }
        /// <summary>
        /// Получение данных виде DataTable
        /// </summary>
        public static DataTable? GetDataTable(string tabel_name, string select_sql = "*", string where_sql = "") {
            ISqlProfile sqlProfile = MDL.SqlProfile!;
            if (MDL.SqlProfile is null) throw new Exception("Профиль подключения не выбран " + nameof(MDL.SqlProfile));
            DataTable? dt;
            try
            {
                sqlProfile.Conection();
                dt = sqlProfile.GetDataTable(tabel_name, select_sql, where_sql);
            }
            catch 
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
        /// Удалит объект
        /// </summary>      
        public static void Delete(SqlParam obj) {
            ISqlProfile sqlProfile = MDL.SqlProfile!;
            if (MDL.SqlProfile is null) throw new Exception("Профиль подключения не выбран " + nameof(MDL.SqlProfile));

            if (obj.ID == -1) return;

            sqlProfile.Conection();
            try
            {
                sqlProfile.Delete(obj.TabelName, nameof(obj.ID)+ "="+ obj.ID.ToString());
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
        /// Проверка запись блокирована или нет 
        /// </summary>
        public static bool IsLock(long id, string tabelname)
        {
            ISqlProfile sqlProfile = MDL.SqlProfile!;
            if (MDL.SqlProfile is null) throw new Exception("Профиль подключения не выбран " + nameof(MDL.SqlProfile));

            bool k = false;
            sqlProfile.Conection();
            try
            {
                k = Prv_IsLock(id, tabelname);
            }
            catch
            {
                throw;
            }
            finally
            {
                sqlProfile.Disconnect();
            }
            return k;
        }
        /// <summary>
        /// Пометить на удаление
        /// </summary>
        public static void Mark_ActiveObjIsDelete(long id, string tabelname, bool del = true ) {
            ISqlProfile sqlProfile = MDL.SqlProfile!;
            if (MDL.SqlProfile is null) throw new Exception("Профиль подключения не выбран " + nameof(MDL.SqlProfile));
            if (id == -1) return;

            sqlProfile.Conection();
            try 
            {
                sqlProfile.SqlCommand($"UPDATE {sqlProfile.QuotSql(tabelname)} SET {nameof(SqlParam.ActiveObjIsDelete)}={(del==true ? "1":"0")} WHERE ID = {id} ");
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

        private static void ForceCreateTabel<T>() where T : SqlParam
        {
            ISqlProfile sqlProfile = MDL.SqlProfile!;
            T obj = Activator.CreateInstance<T>();

            string str = "CREATE TABLE " + sqlProfile.QuotSql(obj.TabelName) + " (";
            foreach (var item in sqlProfile.TitleDB(obj)) str += $"{sqlProfile.QuotSql(item.Item1)} {item.Item2}, ";
            str = str[0..^2] + ") ";

            if (sqlProfile.ExistTabel(obj.TabelName) == false) sqlProfile.SqlCommand(str);
        }

        private static bool Prv_IsLock(long id, string tabelname)
        {
            var kk = MDL.SqlProfile!.GetFieldSql($"ID={id}", tabelname, "LockInfo, datetime('now') as now").First().ToArray();
           // var k = MDL.SqlProfile!.GetDataFieldSql(tabelname, $"ID={id}", "LockInfo, datetime('now') as now").ToArray();
            DateTime start = DateTime.Parse(ISqlProfile.FieldSql.ValueFieldSql(kk, "LockInfo"));
            DateTime now = DateTime.Parse(ISqlProfile.FieldSql.ValueFieldSql(kk, "now")); // дата и время на сервере может быть другая, получаем от туда а не с текущего сервера  
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
            else if (info.PropertyType.Name == "DateOnly") {
                DateOnly t = (DateOnly)obj;
                value = $"{t.Year}-{t.Month}-{t.Day}"; 
            }
            else if (info.PropertyType.Name == "DateTime")
            {
                DateTime t = (DateTime)obj;
                value = $"{t.Year}-{t.Month}-{t.Day}";
            }
            else { value = obj.ToString() ?? ""; }
            value = value.Replace("'", @"\'");

            return value;
        }

        private static object Integrator(object obj, long id, bool _lock) {
            ISqlProfile sqlProfile = MDL.SqlProfile!;
            var tmp = sqlProfile.GetFieldSql(id, ((SqlParam)obj).TabelName);
            return Integrator(obj, tmp, _lock);
        }

        private static object Integrator(object obj, FieldSql[] tmp, bool _lock)
        {           
            if (tmp.Length != 0)
            {
                // Length = 0 будет означать что не найдена строка но она будет как новая rebild 
                // Назначение ID
                var fi = obj.GetType().GetField("IDField", BindingFlags.NonPublic | BindingFlags.Instance);
                fi?.SetValue(obj, Int32.Parse(tmp.First(x => x.NameField == "ID").Value));
            }
            else if (tmp.Length == 0) return obj;
            //var test = (obj.GetType().GetProperties().Where(x => x.CanWrite)).ToList();
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
                        else if (item.PropertyType.Name == "UInt32") { item.SetValue(obj, UInt32.Parse(str)); }
                        else if (item.PropertyType.Name == "String") { item.SetValue(obj, str); }
                        else if (item.PropertyType.Name == "Double") { item.SetValue(obj, double.Parse(str==string.Empty ? "0" : str )); }
                        else if (item.PropertyType.Name == "Decimal") { item.SetValue(obj, decimal.Parse(str == string.Empty ? "0" : str)); }
                        else if (item.PropertyType.Name == "DateOnly") { item.SetValue(obj, DateOnly.Parse(str)); }
                        else if (item.PropertyType.Name == "DateTime") { item.SetValue(obj, DateTime.Parse(str)); }
                        //else throw new NotImplementedException("Такой тип данных '"+ item.PropertyType.Name + "' отсутствует  ");
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
            vsPlus, vsMunis, vsMutation, vsNone
        }

    }
}
