using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RjProduction.Model
{
    /// <summary>
    /// Оперативные внешние задачи 
    /// </summary>
    public class TaskApp
    {
        public required object SenderDoc;
        public string ErrorText = "";
        public StatusEnum Status = StatusEnum.Пауза;



        public enum StatusEnum { 
            Процессе=0, Ошибка=1, Выполнено=2, Пауза =3
        }
    }
}
