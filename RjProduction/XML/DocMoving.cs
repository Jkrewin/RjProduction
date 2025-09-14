using RjProduction.Model;
using RjProduction.Sql;
using System.Windows;
using RjProduction.Model.DocElement;

namespace RjProduction.XML
{
    /// <summary>
    /// Перемещение со склада на склад
    /// </summary>
    public class DocMoving : XmlProtocol, IDocMain
    {
        [SqlIgnore]
        public string Doc_Code
        {
            get => DocCode.Перемещение_По_Складам;
        }             

        public string ID_Doc => Convert.ToHexString(System.Security.Cryptography.MD5.HashData(System.Text.Encoding.ASCII.GetBytes($"{Doc_Code}{DataCreate}+{Number}")));

        public string DocTitle { get; set; } = "Перемещение по складу";

        public required WarehouseClass Warehouse_From { get; set; }

        public WarehouseClass? Warehouse_To { get; set; }

        public DocMoving()
        {
            TypeDoc = $"{Doc_Code}:{DocTitle}";
        }


        public void CarryOut()
        {
            if (MDL.SqlProfile == null)
            {
                IDocMain.ErrorMessage(IDocMain.Error_Txt.Нет_подключенияБД);
                return;
            }
            if (Status != StatusEnum.Не_Проведен) {
                MessageBox.Show("Только не проведенные документы можно проводить. ");
                return; 
            }
            // если была потерена информация или создан одинаковый документ проверка
            var id_doc = SqlRequest.ExistRecord<DocMoving>(new ISqlProfile.FieldSql("ID_Doc", ID_Doc));
            if (id_doc != -1)
            {
                IDocMain.ErrorMessage(IDocMain.Error_Txt.Ошибка_в_документе);
                goto final;
            }

            if (Warehouse_To is null) {
                MessageBox.Show("Склад куда отправить не выбран.");
                return;
            }

            if (Warehouse_To.Equals(Warehouse_From))
            {
                MessageBox.Show("Вы выполняете отправку на этот же склад. ");
                return;
            }

            Warehouse_From.SyncClass();
            Warehouse_To.SyncClass();

            var o = new Products();

            // общая сборка
            List<Pseudonym> ls = [];           
            foreach (var item in MainTabel)
            {
                foreach (var tv in item.Tabels)
                {
                    if (tv is Pseudonym pseudonym)
                    {
                        ls.Add (pseudonym);
                    }
                }
            }

            // Проводим по остаткам документ сначало вычитаем из склада
            foreach (var pseudonym in ls)
            {
                pseudonym.Product.ConcurrentReqest(MDL.SqlProfile, SqlRequest.OperatonEnum.vsMunis, pseudonym.SelectedCub);
            }

            // проверка на ошибки 
            if (ls.Any(x => x.Product. SyncError == true)) {
                Status = StatusEnum.Частично;
            }
            // затем добовляем значения 
            try
            {
                // запуск сохранения в БД Products              
                foreach (var pseudonym in ls)
                {
                    Products? cl = SqlRequest.ReadData<Products>([new(nameof(o.NameItem), pseudonym.Product.NameItem), new("Warehouse", Warehouse_To.ID)]);
                    // Меняет текущее в на этом складе и по этому названию 
                    if (cl != null) cl.ConcurrentReqest(MDL.SqlProfile, SqlRequest.OperatonEnum.vsPlus, pseudonym.SelectedCub);
                    // создает новое значение если не найдено 
                    else
                    {
                        // новую строку создадим если ее не существует 
                        // создадин по клону приведущей строки 
                        Products clon = (Products)pseudonym.Product.Clone();
                        clon.Warehouse = Warehouse_To;
                        clon.Cubature = pseudonym.SelectedCub;
                        SqlRequest.SetData(clon);
                    }
                }

                // далее сохранения документа в БД document
                SqlRequest.SetData(this);
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
            }
            catch (Exception ex)
            {
                MDL.LogError("Ошибка при попытки провести документ " + Doc_Code, ex.Message + " " + ex.Source);
                Status = StatusEnum.Ошибка;
                XmlProtocol.SaveDocXml<DocMoving>(this);
                return;
            }


        final:
            if (Status == StatusEnum.Частично) {
                MessageBox.Show ("В момент отправки изменились остатки на складе. Указанные вами данные устарели и они меньше тех остатков которые уже есть на этом складе и не могут быть меньше нуля. Документ частично проведен ");
            }
            else Status = StatusEnum.Проведен;
            XmlProtocol.SaveDocXml<DocMoving>(this);
        }
    }
}
