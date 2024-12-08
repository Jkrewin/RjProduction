
namespace RjProduction.Sql
{
    public interface ISqlProfile
    {
        /// <summary>
        /// База Данных
        /// </summary>
        public string DataBase { get; set; }
        /// <summary>
        /// Тип подключения к данных
        /// </summary>
        public TypeSqlConnection SqlType { get; }
        /// <summary>
        /// Текущее состояние БД
        /// </summary>
        public bool ConnectIs { get; }

        /// <summary>
        /// Объект помещает в кавычки для некоторых версий sql кавычки бывают разный
        /// </summary>
        /// <param name="str">Текст</param>
        /// <returns>преобразованный текст в кавычках</returns>
        public string QuotSql(string str);




        /// <summary>
        /// Указывает какой тип подключения сейчас
        /// </summary>
        public enum TypeSqlConnection
        {
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
        /// <returns>Возврат Int добавленого id или измененного</returns>
        public long SqlCommand(string sql);
        /// <summary>
        /// Проверка на существоание таблици
        /// </summary>
        public bool ExistTabel(string tabelName);
        public FieldSql[] GetDate(int ID, string TabelName);

        /// <summary>
        /// Подключиться к базе данных
        /// </summary>
        public void Conection();
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
                var atr = field.GetCustomAttributes(true).Any(x => x is Ignore);
                if (atr != false) continue;

                string value = field.GetValue(obj)!.ToString() ?? string.Empty;
                SqlParam? param = field.GetValue(obj) as SqlParam;

                if (param is not null) ls.Add(new(field.Name, TypeSQL("Int32")));
                else if (field.Name == "ID") ls.Insert(0, new("ID", TypeSQL("KEY_ID")));
                else ls.Add(new(field.Name, TypeSQL(field.PropertyType.Name)));
            }
            return ls;
        }

        public readonly struct FieldSql(string name, string type, string value)
        {
            public readonly string NameField = name;
            public readonly string TypeField = type;
            public readonly string Value = value;

            public override readonly string ToString() => $"{NameField}; {TypeField} {Value}";

        }
    }
}
