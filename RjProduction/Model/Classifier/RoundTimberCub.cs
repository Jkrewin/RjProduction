using System.Globalization;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RjProduction.Model.Classifier
{
    /// <summary>
    /// ЛЕСОМАТЕРИАЛЫ КРУГЛЫЕ ГОСТ 2708-75   
    /// <i>Нечетные числа которые отсутствуют будут посчитаны отдельно по формуле a-b/2</i>
    /// </summary>
    public readonly struct RoundTimberCub : Sql.SqlParam.IClassifier
    {
        /// <summary>
        /// Длинна бревна
        /// </summary>
        public readonly double Long;
        /// <summary>
        /// Список размеров
        /// </summary>
        public readonly RowCub[] Rows;

        public string ID_Base => Long.ToString();
        public bool IsDefault => Equals(default(CountryOKSM));

        /// <summary>
        /// Поиск значение по диаметру бревна
        /// </summary>
        /// <param name="diametor">Диаметор бревна бревна</param>
        /// <returns>-1 - нет такого диаметра</returns>
        public double DictionarySize(string diametor) {
            foreach (var row in Rows) { 
                if (row.Diameter== diametor) return row.Value;
            }
            return -1;
        }

        public RoundTimberCub(double long_size , RowCub[] rows)
        {
            Long = long_size;
            Rows = rows;            
        }

        /// <summary>
        /// Структура размера диаметра
        /// </summary>
        public readonly struct RowCub(string d, double value)
        {
            /// <summary>
            /// Диаметор бревна
            /// </summary>
            [XmlAttribute] public readonly string Diameter = d;
            /// <summary>
            /// Объем бревна
            /// </summary>
            [XmlAttribute] public readonly double Value = value;
        }


        public static List<RoundTimberCub> LoadList(string sFile)
        {
            if (File.Exists(sFile) == false)
            {
                MDL.LogError("Файл классификатор размеров бревен не найден " + sFile);
                return [];
            }
            List<RoundTimberCub> ls = [];
            XDocument doc = XDocument.Load(sFile);
            
            XElement? root = doc.Root;
            if (root != null)
            {
                var roundTimberCubs = root.Elements("RoundTimberCub");
                foreach (var rtc in roundTimberCubs)
                {
                    double length = double.Parse(rtc.Element("Long")?.Value ?? "0", CultureInfo.InvariantCulture);
                    var rows = rtc.Element("Rows")?.Elements("RowCub");
                    List<RowCub> ls_rows = [];
                    if (rows != null)
                    {
                        foreach (var row in rows)
                        {
                            string diameter;
                            string value;
                            try
                            {
                                var objA = row.Attribute("Diameter");
                                var objB = row.Attribute("Value");
                                if (objA == null || objB == null)
                                {
                                    throw new InvalidOperationException("Отсутствует необходимый атрибут Diameter или Value в элементе RowCub.");
                                }
                                else
                                {
                                    diameter = objA.Value ?? "0";
                                    value = objB.Value ?? "0";                                    

                                    ls_rows.Add(new RowCub(diameter, double.Parse(value, CultureInfo.InvariantCulture)));
                                }
                            }
                            catch (Exception)
                            {
                                MDL.LogError ("Ошибка чтения классификатора размеров бревен "  +
                                    " Диаметр " + row.Attribute("Diameter")?.Value +
                                    " Объем " + row.Attribute("Value")?.Value);
                            }    
                            

                        }
                    }
                    ls.Add(new RoundTimberCub(length, [.. ls_rows]));
                }
            }
                return ls;
        }

    }
}
