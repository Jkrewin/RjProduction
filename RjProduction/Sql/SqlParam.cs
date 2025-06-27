
using System.Data;
using System.Xml.Serialization;


namespace RjProduction.Sql
{
    /// <summary>
    /// Шаблон страндартных полей в sql строке
    /// </summary>
    public abstract class SqlParam
    {
        private protected long IDField = -1; // название этой переменной менять нельзя используеться как текст ид 
        private bool _locker = false; // владелей блокировки этого класса  
        private bool _LockInfo = false;
        private Timer? LockTimer; // обновляет блокировку 
        private bool _wait = false;

        /// <summary>
        /// Время на которое можно заблокировать запись иначе обновлять таймер блокировки
        /// </summary>
        public const int TIMER_COUNT = 5;

        /// <summary>
        /// Название таблици
        /// </summary>
        [SqlIgnore] public string TabelName { get => this.GetType().Name; }
        /// <summary>
        /// Ид в бд
        /// </summary>
        public long ID { get => IDField; }
        /// <summary>
        /// Пометка на удаление
        /// </summary>
        public bool ActiveObjIsDelete { get; set; } = false;
        /// <summary>
        /// Информации блокировки этой записии пользователем.  
        /// <b>Нельзя заблокировать ранее заблокированные записи. Если не подключен профиль БД или эта запись новая и ее нет в БД</b>
        /// </summary>
        [XmlIgnore]
        public bool LockInfo
        {
            get => _LockInfo;
            set
            {
                if (IDField != -1)
                {
                    string sql;
                    if (MDL.SqlProfile is null) return;
                    bool connector = MDL.SqlProfile.ConnectIs; //момент входа из других методов которые могут изменить LockInfo
                    if (value == true & _LockInfo == false)
                    {
                        // Тут блокируем эту запись 
                        sql = $"UPDATE {TabelName} SET {nameof(LockInfo)}=datetime('now') WHERE ID={IDField}";


                        if (connector==false) MDL.SqlProfile.Conection();
                        try
                        {
                            // Проверим если кто то уже успел заблокировать 
                            //var pr = MDL.SqlProfile.GetDate(IDField, TabelName);
                            var str = ISqlProfile.FieldSql.ValueFieldSql(MDL.SqlProfile.GetFieldSql(IDField, TabelName), nameof(LockInfo));
                            if (string.IsNullOrEmpty(str) || str == "0")
                            {
                                MDL.SqlProfile.SqlCommand(sql);
                                _LockInfo = true;
                                _locker = true;
                                LockTimer = new Timer(new TimerCallback(CallBack), null, 0, 30000);
                            }
                        }
                        catch { }
                        finally
                        {
                            if (connector == false) MDL.SqlProfile.Disconnect();
                        }
                    }
                    else if (value == false & _locker == true)
                    {
                        // разблокируем если вы блокировали этот класс
                        sql = $"UPDATE {TabelName} SET {nameof(LockInfo)}='' WHERE ID={IDField}";

                        if (connector == false) MDL.SqlProfile.Conection();
                        try
                        {
                            MDL.SqlProfile.SqlCommand(sql);
                            _LockInfo = false;

                        }
                        catch { }
                        finally
                        {
                            if (connector == false) MDL.SqlProfile.Disconnect();
                        }
                    }
                    else if (value == false & _locker == false)
                    {
                        throw new Exception("Нельзя разблокировать запись, так как она была ранее заблокирована другим классом");
                    }
                }
            }
        }

        /// <summary>
        /// Заполнение из dataRow значений для этого класса
        /// </summary>
        private protected void FullSet(DataRow dataRow)
        {
            if (long.TryParse(dataRow[nameof(ID)].ToString(), out long l)) IDField = l;
            if (dataRow[nameof(ActiveObjIsDelete)] is bool b) ActiveObjIsDelete = b;
            if (bool.TryParse(dataRow[nameof(LockInfo)].ToString(), out bool bb)) LockInfo = bb;
        }
       
        private void CallBack(object? obj)
        {
            if (ID != -1) return;
            if (MDL.SqlProfile is null) return;
            // Первых вход в метод открывает прозвон возможности сделать запись 
            if (_wait == false)
            {
                LockTimer = new Timer(new TimerCallback(CallBack), null, 0, 150);
                _wait = true;
            }
            
            // если коннект свободен то будет выполнена запись
            if (MDL.SqlProfile.ConnectIs == true)
            {
                string sql = $"UPDATE {TabelName} SET {nameof(LockInfo)}=datetime('now') WHERE ID={IDField}";

                MDL.SqlProfile.Conection();
                try
                {
                    MDL.SqlProfile.SqlCommand(sql);
                }
                finally
                {                   
                        // объект успешно заблокирован
                        MDL.SqlProfile.Disconnect();
                        LockTimer = new Timer(new TimerCallback(CallBack), null, 0, TIMER_COUNT * 10000); //(!) ошибка 1 мин= 60 000мск
                        _wait = false;
                   
                }
            }
        }

        ~SqlParam()
        {
            // Немедленно разблокирует запись иначе она останеться зависшей 
            if (LockInfo == true & _locker == true) LockInfo = false;
        }

        /// <summary>
        /// Организатор загрузки  для внутренней бд из xml  указывает что справочник может загружен из файла а в общую БД пойдет название или ид, в дальнейшем поиск будет от сюда искать 
        /// </summary>
        public interface IClassifier
        {
            /// <summary>
            /// Строка для поиска в списке полученного из файла может содержать название 
            /// </summary>
            public string ID_Base { get; }

            /// <summary>
            /// Значение по умолчанию, либо не загруженны данные из класификатора
            /// </summary>
            public bool IsDefault { get; }
        }
    }
}
