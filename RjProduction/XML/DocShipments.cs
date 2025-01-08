using RjProduction.Model;
using RjProduction.Sql;
using System.Windows;

namespace RjProduction.XML
{
   public class DocShipments: XmlProtocol, IDocMain
    {
        public static readonly string DOC_CODE = DocCode.ВыравниваниеОстатков;

        /// <summary>
        /// Необходимый индификатор для общей бд бля быстрого поиска этого документа
        /// </summary>
        public string ID_Doc
        {
            get
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes($"{DOC_CODE}{DataCreate}+{Number}");
                return Convert.ToHexString(System.Security.Cryptography.MD5.HashData(inputBytes));
            }
        }
        /// <summary>
        /// Заголовок документа
        /// </summary>
        public string DocTitle { get; set; } = "Выравнивание Остатков";
        /// <summary>
        /// Статус документа
        /// </summary>
        [SqlIgnore] public StatusEnum Status { get; set; } = StatusEnum.Не_Проведен;

        public DocShipments() {
            TypeDoc =   $"{DOC_CODE}:{DocTitle}";
        }

        public void CarryOut()
        {
            if (MDL.SqlProfile == null)
            {
                MessageBox.Show("Нет активного подключения к БД, создайте новое подключение к БД.");
                return;
            }
            if (Status == StatusEnum.Проведен) return;

            // если была потерена информация или создан одинаковый документ проверка
            var id_doc = SqlRequest.ExistRecord<DocShipments>(new ISqlProfile.FieldSql("ID_Doc", ID_Doc));
            if (id_doc != -1)
            {
                MessageBox.Show("Этот документ был ранее проведен. С такой датой и номером. Уже зафиксированы в БД изменения, если вам нужно внести изменения, то нужно выполнить корректировку остатков. Создав документ по корректировки остатков на складе. ");
                goto final;
            }

            // Получаем название поля для изменений
            var o = new Products();
            string fild = nameof(o.Cubature);
            bool error = false;

            // Проводим по остаткам документ 
            foreach (var item in MainTabel)
            {
                foreach (var tv in item.Tabels)
                {
                    if (tv is Pseudonym pseudonym) {
                        if (Sql.SqlRequest.ConcurrentReqest(o.TabelName, pseudonym.ID_Prod, new (fild, pseudonym.CubAll.ToString()), pseudonym.Operation) )
                        {
                            pseudonym.SyncError = true;
                           error = true;
                        }
                    }
                }
            }

            if (error) {
                MessageBox.Show("У вас возникли проблемы при синхронизации, так как данные были ранее изменены другим пользователем. Поэтому документ проведен частично ");
                SqlRequest.SetData(this);
                Status = StatusEnum.Частично;
                XmlProtocol.SaveDocXml<DocShipments>(this);
                return;
            }

            // Сохраним документ
            SqlRequest.SetData(this);

        final:
            Status = StatusEnum.Проведен;
            XmlProtocol.SaveDocXml<DocShipments>(this);
        }
    }
}
