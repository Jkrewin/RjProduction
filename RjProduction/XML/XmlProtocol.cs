using RjProduction.Model;
using RjProduction.Sql;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace RjProduction.XML
{
    /// <summary>
    /// Создает среду переменных понятную для файла xml обходя интерфейсы также типы данных которые не потдерживаються
    /// Винмание Dependent code зависиит от новых Idoc объектов нужно дополнить  
    /// </summary>
    public abstract class XmlProtocol : SqlParam
    {        
        private uint _number = 0;

        /// <summary>
        /// Только для xml сохраняет дату / DateOnly тут не работает
        /// </summary>
        [SqlIgnore, XmlElement] public DateTime XmlDate { get => DataCreate.ToDateTime(TimeOnly.MinValue); set { DataCreate = DateOnly.FromDateTime(value); } }

        /// <summary>
        /// Статус документа
        /// </summary>
        [SqlIgnore] public StatusEnum Status { get; set; } = StatusEnum.НеСохранен;
        /// <summary>
        /// Дата создания документа
        /// </summary>
        [XmlIgnore] public DateOnly DataCreate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        /// <summary>
        /// Номер документа, также сохраняет последний номер в MDL
        /// </summary>
        public uint Number
        {
            get => _number;
            set
            {
                _number = value;
                MDL.MyDataBase.NumberDef = value + 1;
            }
        }
        /// <summary>
        /// Версия документиа
        /// </summary>
        public static string VerDoc { get => "v.1.0"; }
        /// <summary>
        /// Тип документа
        /// </summary>
        [SqlIgnore] public string TypeDoc { get; set; } = "NaN";

        public string FileName (string typedoc) => $"{typedoc}_{DataCreate}_{Number}.xml"; 


        [SqlIgnore, XmlElement, DependentCode]
        public GrupClass[] Grups
        {
            get
            {
                List<GrupClass> g = [];
                foreach (var item in MainTabel)
                {
                    GrupClass grup = new() { NameGrup = item.NameGrup ?? "" };
                    foreach (var tv in item.Tabels)
                    {
                        if (tv is Tabel_Timbers _Timbers) grup.Timbers.Add(_Timbers);
                        else if (tv is Employee e) grup.Employees.Add(e);
                        else if (tv is MaterialObj m) grup.Materials.Add(m);
                        else if (tv is Surcharges s) grup.Surcharges.Add(s);
                        else if (tv is Pseudonym p) grup.Pseudonyms.Add(p);
                        else if (tv is Track t) grup.Tracks.Add(t);
                        else throw new NotImplementedException("DependentCode: Отуствие класса или структуры " + tv.ToString());
                    }
                    g.Add(grup);
                }
                return [.. g];
            }
            set
            {
                foreach (var item in value)
                {
                    Model.GrupObj grup = new() { NameGrup = item.NameGrup ?? "" };
                    foreach (var tv in item.Employees) grup.Tabels.Add(tv);
                    foreach (var tv in item.Materials) grup.Tabels.Add(tv);
                    foreach (var tv in item.Timbers) grup.Tabels.Add(tv);
                    foreach (var tv in item.Surcharges) grup.Tabels.Add(tv);
                    foreach (var tv in item.Pseudonyms) grup.Tabels.Add(tv);
                    foreach (var tv in item.Tracks) grup.Tabels.Add(tv);
                    MainTabel.Add(grup);
                }
            }
        }
        /// <summary>
        /// Главная  таблица
        /// </summary>
        [SqlIgnore, XmlIgnore] public List<GrupObj> MainTabel { get; set; } = [];

        [XmlIgnore]
        public decimal Amount
        {
            get
            {
                decimal amount = 0;
                foreach (var item in MainTabel)
                {
                    amount += item.Amount;
                }
                return amount;
            }
        }
        [XmlIgnore]
        public double Cubs
        {
            get
            {
                double cuball = 0;
                foreach (var item in MainTabel)
                {
                    cuball += item.Cubature;
                }
                return cuball;
            }
        }

        static public T? LoadDocXML<T>(string sFile) where T : IDocMain
        {
            XmlSerializer xmlSerializer = new(typeof(T));
            try
            {
                using FileStream fs = new(sFile, FileMode.OpenOrCreate);
                object? obj = xmlSerializer.Deserialize(fs);
                if (obj is T t) return t;
                else return default;
            }
            catch (InvalidOperationException ax)
            {
                MessageBox.Show("Ошибка чтения файла Xml (" + sFile + ") причина : " + ax.Message);
                return default;
            }
        }
        static public void SaveDocXml<T>(IDocMain doc) where T : XmlProtocol
        {
            string sFile = MDL.XmlPatch(doc.DataCreate);
            if (doc.Status == StatusEnum.НеСохранен) doc.Status = StatusEnum.Не_Проведен;
            MDL.SaveXml<T>((T)doc, $"{sFile}\\{((XmlProtocol)doc).FileName(doc.Doc_Code)}");
        }

        /// <summary>
        /// Реформирует интерфейс 
        /// </summary>
       [DependentCode] public class GrupClass
        {
            public required string NameGrup { get; set; } = "";

            public List<Tabel_Timbers> Timbers = [];
            public List<Employee> Employees = [];
            public List<MaterialObj> Materials = [];
            public List<Surcharges> Surcharges = [];
            public List<Pseudonym> Pseudonyms = [];
            public List<Track> Tracks = [];

        }
    }
}
