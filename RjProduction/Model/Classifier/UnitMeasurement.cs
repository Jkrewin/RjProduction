using System.IO;
using System.Xml.Linq;

namespace RjProduction.Model.Classifier
{
    /// <summary>
    /// Общероссийский классификатор единиц измерения (ОКЕИ) ОК 015-94 (МК 002-97)
    /// (утв.Постановлением Госстандарта РФ от 26.12.94 N 366)
    /// </summary>
    public readonly struct UnitMeasurement: Sql.SqlParam.IClassifier
    {
        /// <summary>
        /// Код
        /// </summary>
        public readonly ushort Code;
        /// <summary>
        /// Наименование единицы измерения
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Условное обозначение  национальное
        /// </summary>
        public readonly string SymbolRu;
        /// <summary>
        /// Условное обозначение международное
        /// </summary>
        public readonly string SymbolEn;
        /// <summary>
        /// Кодовое буквенное обозначение национальное
        /// </summary>
        public readonly string AlphaRu;
        /// <summary>
        /// Кодовое буквенное обозначение международное
        /// </summary>
        public readonly string AlphaEn;
        /// <summary>
        /// Группа
        /// </summary>
        public readonly string GroupName;

        public string ID_Base { get => Name; }

        public bool IsDefault => Equals(default(UnitMeasurement));

        public UnitMeasurement(ushort code, string name, string designationRUS, string codeDesignationRUS, string designationMain ="", string codeDesignationMain ="") {
            Code = code;
            Name = name;
            SymbolRu = designationRUS;
            SymbolEn = codeDesignationMain;
            AlphaEn = codeDesignationMain;
            AlphaRu= codeDesignationMain;
            GroupName = "";
        }

        public UnitMeasurement(string groupName, ushort code, string name, string designationRUS, string codeDesignationRUS, string designationMain = "", string codeDesignationMain = "")
        {
            Code = code;
            Name = name;
            SymbolRu = designationRUS;
            SymbolEn = codeDesignationMain;
            AlphaEn = codeDesignationMain;
            AlphaRu = codeDesignationMain;
            GroupName = groupName;
        }

        public override string ToString() => Name + " (" + SymbolRu + ")";

        /// <summary>
        /// Загружает список на основе  https://github.com/solienko/okei
        /// </summary>
        public static List<UnitMeasurement> LoadList(string sFile)
        {
            if (File.Exists(sFile) == false)
            {
                MDL.LogError("Файл классификатор единиц измерения не найден " + sFile);
                return [];
            }
            List<UnitMeasurement> ls = [];
            XDocument doc = XDocument.Load(sFile);
            var okeiElement = doc.Element("OKEI");
            if (okeiElement is null) return [];
            var sectionElement = okeiElement.Element("Section");
            if (sectionElement is null) return [];

              foreach (var groupElement in sectionElement.Elements("Group"))
               {
                   string groupName = groupElement.Attribute("Name")!.Value;
                   foreach (var tv in groupElement.Elements("Uom").Select(u => new UnitMeasurement(groupName, ushort.Parse(u.Attribute("Code")!.Value ?? "0"), u.Attribute("Name")!.ToString() ?? "", u.Attribute("SymbolRu")!.Value, u.Attribute("AlphaRu")!.Value, u.Attribute("SymbolEn")!.Value, u.Attribute("AlphaEn")!.Value)))
                   {
                       ls.Add(tv);
                   }
               }
                      
            return ls;
        }


    }
}
