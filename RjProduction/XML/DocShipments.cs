using RjProduction.Model;
using RjProduction.Sql;
using System.Windows;
using RjProduction.Model.DocElement;

namespace RjProduction.XML
{
    /// <summary>
    /// Выравнивание Остатков
    /// </summary>
    public class DocShipments: XmlProtocol, IDocMain
    {
        [SqlIgnore]        public string Doc_Code
        {
            get => DocCode.Выравнивание_Остатков;
        }

        /// <summary>
        /// Необходимый индификатор для общей бд бля быстрого поиска этого документа
        /// </summary>
        public string ID_Doc
        {
            get
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes($"{Doc_Code}{DataCreate}+{Number}");
                return Convert.ToHexString(System.Security.Cryptography.MD5.HashData(inputBytes));
            }
        }
        /// <summary>
        /// Заголовок документа
        /// </summary>
        public string DocTitle { get; set; } = "Выравнивание Остатков";
       
        public DocShipments() {
            TypeDoc =   $"{Doc_Code}:{DocTitle}";
        }

        public void CarryOut()
        {
            if (MDL.SqlProfile == null)
            {
                IDocMain.ErrorMessage(IDocMain.Error_Txt.Нет_подключенияБД);
                return;
            }
            if (Status == StatusEnum.Проведен) return;

            // если была потерена информация или создан одинаковый документ проверка
            var id_doc = SqlRequest.ExistRecord<DocShipments>(new ISqlProfile.FieldSql("ID_Doc", ID_Doc));
            if (id_doc != -1)
            {
                IDocMain.ErrorMessage(IDocMain.Error_Txt.Ошибка_в_документе);
                goto final;
            }

            // Получаем название поля для изменений          
            bool error = false; 
            // Проводим по остаткам документ 
            foreach (var item in MainTabel)
            {
                foreach (var tv in item.Tabels)
                {
                    if (tv is Pseudonym pseudonym) 
                    {
                        pseudonym.Product.ConcurrentReqest(MDL.SqlProfile, pseudonym.Operation, pseudonym.SelectedCub);
                        if (pseudonym.Product.SyncError == true) error = true;
                    }
                }
            }

            if (error)
            {
                MessageBox.Show("У вас возникли проблемы при синхронизации, так как данные были ранее изменены другим пользователем. Поэтому документ проведен частично ");
                SqlRequest.SetData(this);
                Status = StatusEnum.Частично;
                XmlProtocol.SaveDocXml<DocShipments>(this);
                return;
            }


            // Сохраним документ
               SqlRequest.SetData(this);
            //соберем список строк
            foreach (var item in MainTabel)
            {
                foreach (var tv in item.Tabels)
                {        
                    if (tv is DocRow.IDocRow d)
                    {  // сохраним их
                        SqlRequest.SetData(d.ToDocRow(item.NameGrup, ID_Doc));
                        d.Send_DB(ID_Doc); // Дополнительная отправка в дб строики если нужно 
                    }
                    else throw new NotImplementedException("DependentCode: Отуствие класса или структуры " + tv.ToString() + "\n ()CarryOut " + this.ToString());
                }
            }          

        final:
            Status = StatusEnum.Проведен;
            XmlProtocol.SaveDocXml<DocShipments>(this);
        }
    }
}
