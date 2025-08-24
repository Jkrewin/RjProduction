using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RjProduction.Sql
{
    /// <summary>
    /// Устанваливает уникальное имя в БД независимо от название класса или структуры
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class | AttributeTargets.Struct)]
    public class SqlTabelName : System.Attribute
    {
        /// <summary>
        /// Уникальное имя 
        /// </summary>
        public string TabelName { get; set; } = "TabelName";

        public SqlTabelName(string name)
        {
            TabelName = name;
        }
    }
}
