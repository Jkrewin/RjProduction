using RjProduction.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RjProduction.Model.Catalog
{
    /// <summary>
    /// Информация о договоре в будущем будет дополнено другими полями
    /// </summary>
    public class Contract : SqlParam
    {
        public Fgis.XML.forestUsageReport.headerClass.ContractType? FgisElement;


    }
}
