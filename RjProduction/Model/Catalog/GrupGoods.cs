using RjProduction.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RjProduction.Model.Catalog
{
    public class GrupGoods : SqlParam
    {
        public string NameGrup { get; set; } = "Товар";
        /// <summary>
        /// Цветовая этикетка для товара
        /// </summary>
        public string? ColorLabel { get; set; }
    }
}
