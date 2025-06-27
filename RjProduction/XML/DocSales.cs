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
                XmlProtocol.SaveDocXml<DocSales> (this);
            }
                      

            // запуск сохранения в БД Products              
            foreach (var item in ListPseudonym)
            {
                Products? cl = SqlRequest.ReadData<Products>([new("NameItem", item.Product.NameItem), new("Warehouse", item.Product.Warehouse.ID)]);
                // Меняет текущее в на этом складе и по этому названию 
                if (cl != null)
                {
                    cl.ConcurrentReqest(MDL.SqlProfile, SqlRequest.OperatonEnum.vsMunis, item.SelectedCub);
                    item.Product.SyncError = cl.SyncError; // сохранить ошибку если была
                }
                // создает новое значение если не найдено 
                else SqlRequest.SetData(item.Product);
            }

            // далее сохранения документа в БД document
            SqlRequest.SetData(this);
            List<DocRow> rows = [];
            Status = StatusEnum.Проведен;
            var products = (from tv in ListPseudonym where tv is IConvertDoc select ((IConvertDoc)tv).ToProducts()).ToArray<Products>();
            foreach (var obj in MainTabel)
            {
                foreach (var tv in obj.Tabels)
                {
                    if (tv is IConvertDoc doc)
                    {
                        // Если транзакция была проблем сохраняет в этот документ если были ошибки то частично проведен 
                        var p = doc.ToProducts();
                        if (products.Any(x => x.NameItem == p.NameItem & p.SyncError == true))
                        {
                            Status = StatusEnum.Частично;
                            continue;
                        }
                    }
                    rows.Add(new DocRow(tv, obj.NameGrup, ID_Doc));
                }
            }
            SqlRequest.SetData([.. rows]);
            XmlProtocol.SaveDocXml<DocSales>(this);

        }
    }
}
