using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RjProduction.Model.Classifier
{
    /// <summary>
    /// ОКСМ — Общероссийский классификатор стран мира
    /// Классификатор ОКСМ ОК (МК (ИСО 3166) 004-97) 025-2001 с изменением №33 от 1 августа 2024 г.
    /// Скачен от сюда https://classifikators.ru/oksm#download сохранен через excel формат xml 2003
    /// </summary>
    public readonly struct CountryOKSM : Sql.SqlParam.IClassifier
    {
        /// <summary>
        /// необязательный код в справочнике можно использовать как id
        /// </summary>
        public readonly int Number;
        /// <summary>
        /// Код страны
        /// </summary>
        public readonly ushort Code;
        /// <summary>
        /// Короткое название все заглавные буквы
        /// </summary>
        public readonly string ShortName;
        /// <summary>
        /// Полное название 
        /// </summary>
        public readonly string FullName;
        /// <summary>
        /// Буквенный код из 2 символов 
        /// </summary>
        public readonly string WordCode2;
        /// <summary>
        /// Буквенный код из 3 символов 
        /// </summary>
        public readonly string WordCode3;

        public bool IsDefault => Equals(default(CountryOKSM));
        public string ID_Base => ShortName;

        public CountryOKSM(int number, ushort code, string shortName, string fullName, string wordCode2, string wordCode3)
        {
            Number = number;
            Code = code;
            ShortName = shortName;
            FullName = fullName;
            WordCode2 = wordCode2;
            WordCode3 = wordCode3;
        }

        public override string ToString() => Code + " " + FullName;

        public static List<CountryOKSM> LoadList(string sFile)
        {
            if (File.Exists(sFile) == false)
            {
                MDL.LogError("Файл классификатор единиц измерения не найден " + sFile);
                return [];
            }
            List<CountryOKSM> ls = [];
            var htmlDoc = new HtmlDocument();
            XDocument doc = XDocument.Load(sFile);
            XNamespace ss = "urn:schemas-microsoft-com:office:spreadsheet";

            foreach (var row in doc.Descendants(ss + "Row"))
            {
                var cells = row.Elements(ss + "Cell");

                if (cells.Count() >= 6)
                {
                    string number = cells.ElementAt(0).Value;
                    if (int.TryParse(number, out int number_result) == false)
                    {
                        continue;
                    }
                    string code = cells.ElementAt(1).Value;
                    if (ushort.TryParse(code, out ushort code_result) == false)
                    {
                        continue;
                    }
                    string shortName = cells.ElementAt(2).Value;
                    string fullName = cells.ElementAt(3).Value;
                    string alpha2 = cells.ElementAt(4).Value;
                    string alpha3 = cells.ElementAt(5).Value;
                    ls.Add(new(number_result, code_result, shortName, fullName, alpha2, alpha3));
                }
            }

            return ls;
        }
    }
}
