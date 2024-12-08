using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RjProduction.Sql
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class Ignore : System.Attribute
    {
        /// Игнорирует этот элемент и не добавляет его в БД 
    }
}
