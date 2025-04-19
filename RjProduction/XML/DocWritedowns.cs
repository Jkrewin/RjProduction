using RjProduction.Model;
using RjProduction.Sql;
using RjProduction.Model.DocElement;
using System.Text;
using System.Windows;

namespace RjProduction.XML
{
    /// <summary>
    /// Списание продукции
    /// </summary>
    public class DocWriteDowns : XmlProtocol, IDocMain
    {
        [SqlIgnore]
        public string Doc_Code
        {
            get => DocCode.Списание_Продукции;
        }
        /// <summary>
        /// Необходимый индификатор для общей бд бля быстрого поиска этого документа
        /// </summary>
        public string ID_Doc
        {
            get => Convert.ToHexString(System.Security.Cryptography.MD5.HashData(Encoding.ASCII.GetBytes($"{Doc_Code}{DataCreate}+{Number}")));
        }
        /// <summary>
        /// Заголовок документа
        /// </summary>
        public string DocTitle { get; set; } = "Списание продукции";
        /// <summary>
        /// Склад где списание
        /// </summary>
        public WarehouseClass Warehouse { get; set; }
        /// <summary>
        /// Причина списание
        /// </summary>
        public string Reason { get; set; }

        public double AllCubs
        {
            get
            {
                double d = 0;
                foreach (var obj in MainTabel)
                {
                    foreach (var tv in obj.Tabels)
                    {
                        if (tv is MaterialObj material) d += material.CubatureAll;
                        else if (tv is Tabel_Timbers timber) d += timber.CubatureAll;
                    }
                }
                return d;
            }
        }
        public decimal AllSum { get => MainTabel.Sum(x => x.Amount); }

        public DocWriteDowns()
        {
            TypeDoc = Doc_Code + DocTitle;
            Warehouse = new WarehouseClass() { NameWarehouse = "n/a" };
            Reason = "Уничтожение товара";
        }

        /// <summary>
        /// Провести документ 
        /// </summary>
        public void CarryOut()
        {
            if (Status != StatusEnum.Не_Проведен) {
                MessageBox.Show("Текущий статус документа: " + Status.ToString());
                return;
            } 
            if (MDL.SqlProfile == null)
            {
                MessageBox.Show("Нет активного подключения к БД, создайте новое подключение к БД.");
                return;
            }
            var id_doc = SqlRequest.ExistRecord<DocWriteDowns>(new ISqlProfile.FieldSql("ID_Doc", ID_Doc));
            if (id_doc != -1)
            {
                MessageBox.Show("Этот документ был ранее проведен или произошла ошибка. С такой датой и номером уже ранее сохранен в БД. ");
                Status = StatusEnum.Проведен;
                goto final;
            }

            Warehouse.SyncClass();



        final: XmlProtocol.SaveDocXml<DocArrival>(this);
        }
    }
}
