
using System.Data;

namespace RjProduction.Sql
{
    public interface ISqlProfile: IDisposable
    {
        /// <summary>
        /// База Данных
        /// </summary>
        public string DataBaseFile { get; set; }
        /// <summary>
        /// Тип подключения к данных
        /// </summary>
        public TypeSqlConnection SqlType { get; }
        /// <summary>
        /// Текущее состояние БД
        /// </summary>
        public bool ConnectIs { get; }
        /// <summary>
        /// Строка последнего запроса для поиска ошибок
        /// </summary>
        public string SqlLogString { get; set; }


        /// <summary>
        /// Объект помещает в кавычки для некоторых версий sql кавычки бывают разный
        /// </summary>
        /// <param name="str">Текст</param>
        /// <returns>преобразованный текст в кавычках</returns>
        public string QuotSql(string str);

        /// <summary>
        /// Управляет транзакцией
        /// </summary>
        public void Transaction(TypeTransaction transaction);
       
        /// <summary>
        /// Указывает какой тип подключения сейчас
        /// </summary>
        public enum TypeSqlConnection
        {
            none,
            /// <summary>
            /// <b>NuGet</b> >> Microsoft.Data.SqlClient
            /// </summary>
            MSSQL,
            /// <summary>
            /// <b>NuGet</b> >> System.Data.SQLite.Core 
            /// </summary>
            Sqlite,
            /// <summary>
            ///  NuGet >> MySql.Data 8.0.33
            /// </summary>
            MySQL
        }
        /// <summary>
        /// Приводит тип данных в соотвествии с этой БД
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public string TypeSQL(string info);
        /// <summary>
        /// Выполняет SQL запрос
        /// </summary>
        /// <param name="sql">Строка запроса</param>
        /// <returns>Возврат Int добавленого id </returns>
        public long SqlCommand(string sql);
        /// <summary>
        /// Проверка на существоание таблици
        /// </summary>
        public bool ExistTabel(string tabelName);

        /// <summary>
        /// Полученние данных по id
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="TabelName"></param>
        /// <returns></returns>
        public FieldSql[] GetFieldSql(long ID, string TabelName);
        public FieldSql[] GetFieldSql(string where, string TabelName);
        public List<FieldSql[]> GetFieldSql(string where, string tabelName, string select ="*");

        /// <summary>
        /// Получает List объектов по запросу или с параметрами поиска
        /// </summary>
        /// <param name="tabelName">Нзавание таблици</param>        
        /// <param name="where">Необязателен. Можно составить запрос после WHERE с поиском строк </param>
        public List<object[]> AdapterSql(string tabelName, out long id, string where = "");
        public object? AdapterSql(string tabelName, string nameField, string where = "");

        public void Delete(string tabelName, string where);

        /// <summary>
        /// Подключиться к базе данных
        /// </summary>
        public void Conection(bool startTransaction = false);
        /// <summary>
        /// Отключиться от БД
        /// </summary>
        public void Disconnect();
        /// <summary>
        /// Получает типы и заголовки из класса
        /// </summary>
        public List<(string, string)> TitleDB(SqlParam obj)
        {
            List<(string, string)> ls = [];
            foreach (var field in obj.GetType().GetProperties().Where(x => x.CanRead))
            {
                // Проверяем атрибуты
                var atr = field.GetCustomAttributes(true).Any(x => x is SqlIgnore);
                if (atr != false) continue;
                
                SqlParam? param = field.GetValue(obj) as SqlParam;

                // SqlFlat добовляет столбци в таблицу из класса
                if (field.GetValue(obj) is SqlFlat paramFlat) ls.AddRange(TitleDB_SqlFlat(paramFlat));
                // Проверяем что это касификатор и добав. тoлько название
                else if (field.GetValue(obj) is Sql.SqlParam.IClassifier cls) ls.Add(new(field.Name, TypeSQL("String")));
                // id класс
                else if (param is not null) ls.Add(new(field.Name, TypeSQL("Int32")));
                // это поле исключетельно для ID в бд
                else if (field.Name == nameof(SqlParam.ID)) ls.Insert(0, new("ID", TypeSQL("KEY_ID")));
                // стандартное поле
                else ls.Add(new(field.Name, TypeSQL(field.PropertyType.Name)));
            }
            return ls;
        }
        /// <summary>
        /// Для вложенных классов без использование доп таблици
        /// </summary>
        /// <param name="obj">SqlFlat</param>
        private List<(string, string)> TitleDB_SqlFlat(SqlFlat obj) {
            List<(string, string)> ls = [];
            foreach (var field in obj.GetType().GetProperties().Where(x => x.CanRead))
            {
                // Проверяем атрибуты
                var atr = field.GetCustomAttributes(true).Any(x => x is SqlIgnore);
                if (atr != false) continue;

                SqlParam? param = field.GetValue(obj) as SqlParam;

                if (param is not null) ls.Add(new(field.Name, TypeSQL("Int32")));
                else if (field.Name == nameof(SqlParam.ID)) ls.Insert(0, new("ID", TypeSQL("KEY_ID")));
                else ls.Add(new(field.Name, TypeSQL(field.PropertyType.Name)));
            }
            return ls;

        }

        /// <summary>
        /// Обеспечивает работу с DataTable
        /// </summary>
        /// <param name="tabelName">Название таблици</param>
        /// <param name="select_sql">Запрос по столбцам или все сразу </param>
        /// <param name="where_sql">where выборка если нужна  </param>
        /// <returns>DataTable</returns>
        public DataTable GetDataTable(string tabelName, string select_sql ="*", string where_sql="");
        /// <summary>
        /// Являеться структурой полей базы данный улучшая поиск по ним
        /// </summary>
        public readonly struct FieldSql
        {
            public readonly string NameField ;
            public readonly string TypeField ;
            public readonly string Value ;
            public readonly double ValueDbl;

            public FieldSql(string name, double value)
            {
                NameField = name;
                Value = value.ToString();
                ValueDbl = value;
                TypeField = string.Empty;
            }

            public FieldSql(string name, string value) {
                NameField = name;
                Value = value;
                TypeField = string.Empty;
            }

            public FieldSql(string name, string type, string value) {
                NameField = name;
                TypeField = type;
                Value = value;
            }

            public override readonly string ToString() => $"{NameField}; {TypeField} {Value}";

            /// <summary>
            /// посик значения в массиве по названию NameField
            /// </summary>
            public static string ValueFieldSql(FieldSql[] fields, string nameField) {
                for (int i = fields.Length-1; i >= 0; i--)
                {
                    if (fields[i].NameField.Equals(nameField)) { 
                        return fields[i].Value ;
                    }
                }
                return string.Empty ;   
            }
        }

        public enum TypeTransaction
        {
            commit, roolback        
        }
    }
}
