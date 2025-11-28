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
                strs.Add("Необходимо выбрать покупателя ");

            }
            if (Warehouse_From is null)
            {
                strs.Add("Склад не выбран ");

            }

            if (strs.Count != 0) { 
                MessageBox.Show(string.Join("\n", strs), "Ошибка заполнения документа", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (Status == StatusEnum.Проведен) return;
            if (MDL.SqlProfile == null)
            {
                IDocMain.ErrorMessage(IDocMain.Error_Txt.Нет_подключенияБД);
                return;
            }

            var id_doc = SqlRequest.ExistRecord<DocSales>(new ISqlProfile.FieldSql(nameof(ID_Doc), ID_Doc));
            if (id_doc != -1)
            {
                IDocMain.ErrorMessage(IDocMain.Error_Txt.Ошибка_в_документе);
                Status = StatusEnum.Частично;
                XmlProtocol.SaveDocXml<DocSales> (this);
            }
                      

            List<SqlRequest.FieldSqlTrn> trn = [];
            // запуск сохранения в БД Products              
            foreach (var item in ListPseudonym)
            {
                Products? cl = SqlRequest.ReadData<Products>([new("NameItem", item.Product.NameItem), new("Warehouse", item.Product.Warehouse.ID)]);
                // Меняет текущее в на этом складе и по этому названию 
                if (cl != null) trn.Add(new SqlRequest.FieldSqlTrn(nameof(Products.Cubature), cl.ID, item.SelectedCub, SqlRequest.OperatonEnum.vsMunis));
                // если не найдено невозможно сохранить документ 
                else 
                {
                    MessageBox.Show($"Отсутствует на складе такой товар ({item.Product.NameItem}) ошибка синхронизации", "Ошибка при сохранение ", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
                        
            SqlRequest.ReqestTransaction(MDL.SqlProfile, nameof(Products), trn).ForEach(item => { 
                var tv = ListPseudonym.Find(x => item.ID == x.Product.ID);
                if (tv is not null) tv.Product.SyncError = item.TransactionСompleted;
            });

            //Если транзакция была проблем 
            if (ListPseudonym.Any(x => x.Product.SyncError == false)) {
                var r = from tv in ListPseudonym where tv.Product.SyncError == false select  $"{tv.Product.NameItem} = {tv.Product.Cubature} Не может быть меньше количества которе на складе  ";
                MessageBox.Show("Были изменения в БД внести изменения невозможно проверте количество: "  + string.Join("\n", r), "Ошибка при сохранение ", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }           

            // далее сохранения документа в БД document
            SqlRequest.SetData(this);
            Status = StatusEnum.Проведен;
            var products = (from tv in ListPseudonym where tv is IConvertDoc select ((IConvertDoc)tv).ToProducts()).ToArray<Products>();
            foreach (var obj in MainTabel)
            {
                foreach (var tv in obj.Tabels)
                {                   
                    if (tv is DocRow.IDocRow d)
                    {
                        SqlRequest.SetData(d.ToDocRow(obj.NameGrup, ID_Doc));
                        d.Send_DB(this,obj); // Дополнительная отправка в дб строики если нужно 
                    }
                    else throw new NotImplementedException("DependentCode: Отуствие класса или структуры " + tv.ToString() + "\n ()CarryOut " + this.ToString());
                }
            }
            SaveDocXml<DocSales>(this);

        }
    }
}
