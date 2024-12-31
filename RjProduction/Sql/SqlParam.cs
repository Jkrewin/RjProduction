
using RjProduction.Model;
using System.Data;

namespace RjProduction.Sql
{
    /// <summary>
    /// Шаблон страндартных полей в sql строке
    /// </summary>
    public abstract class SqlParam
    {
        private protected long IDField = -1; // название этой переменной менять нельзя используеться как текст ид 
        
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
        /// Информации блокировки этой записии пользователем
        /// </summary>
        public string LockInfo { get; set; } = "";

        /// <summary>
        /// Заполнение из dataRow значений для этого класса
        /// </summary>
        private protected void FullSet(DataRow dataRow)
        {
            if (long.TryParse(dataRow["ID"].ToString(), out long l)) IDField = l;
            if (dataRow["ActiveObjIsDelete"] is bool b) ActiveObjIsDelete = b;
            if (dataRow["LockInfo"] is string s) LockInfo = s;
        }
    }
}
