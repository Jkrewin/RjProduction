
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
        [Ignore] public string TabelName { get => this.GetType().Name; }
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

       
    }
}
