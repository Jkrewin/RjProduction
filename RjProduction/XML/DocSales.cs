using RjProduction.Model;
using RjProduction.Model.Catalog;
using RjProduction.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RjProduction.XML
{
    /// <summary>
    /// Продажи
    /// </summary>
    public class DocSales : XmlProtocol, IDocMain
    {
        [SqlIgnore]
        public string Doc_Code { get => DocCode.Продажи; }

        public string ID_Doc => Convert.ToHexString(System.Security.Cryptography.MD5.HashData(System.Text.Encoding.ASCII.GetBytes($"{Doc_Code}{DataCreate}+{Number}")));

        public string DocTitle { get; set; } = "Продажа продукции";

        public required WarehouseClass Warehouse_From { get; set; }

        public Company? Buyer { get; set; } 

        public DocSales()
        {
            TypeDoc = $"{Doc_Code}:{DocTitle}";
        }

        public void CarryOut()
        {
            List<string> strs = [];

            if (Buyer is null) {
                strs.Add("Необходимо выбрать покупателя");

            }
            if (Warehouse_From is null)
            {
                strs.Add("Склад не выбран");

            }


            if (Status == StatusEnum.Проведен) return;
            if (MDL.SqlProfile == null)
            {
                MessageBox.Show("Нет активного подключения к БД, создайте новое подключение к БД.");
                return;
            }

            var id_doc = SqlRequest.ExistRecord<DocSales>(new ISqlProfile.FieldSql(nameof(ID_Doc), ID_Doc));
            if (id_doc != -1)
            {
                MessageBox.Show("Этот документ был ранее проведен или произошла ошибка. С такой датой и номером. Уже зафиксированы в БД изменения, если вам нужно внести изменения, то нужно выполнить корректировку остатков. Создав документ по корректировки остатков на складе. ");
                Status = StatusEnum.Частично;
                XmlProtocol.SaveDocXml<DocSales>(this);
            }

           
        }
    }
}
