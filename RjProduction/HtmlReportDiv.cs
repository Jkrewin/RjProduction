using HtmlAgilityPack;
using System.Diagnostics;
using System.IO;

namespace RjProduction
{
    /// <summary>
    /// Создает отчет на основе html страницы с использованием div рамок.
    /// </summary>
    public class HtmlReportDiv
    {
        readonly Dictionary<string, string> _paramDictionary = [];
        readonly Dictionary<string, string> _divDictionary = [];

        public HtmlReportDiv(string sFile)
        {
            if (File.Exists(sFile) == false) throw new Exception("Файл не доступен по пути " + sFile);            

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(File.ReadAllText(sFile, System.Text.Encoding.UTF8));
                        
            foreach (var node in htmlDoc.DocumentNode.SelectNodes("//div"))
            {
                string name = node.Id;
                string value = node.InnerHtml;
                if (!string.IsNullOrEmpty(name) & _divDictionary.ContainsKey(name) == false) _divDictionary[name] = value;
            }

            var k = htmlDoc.DocumentNode.SelectNodes("//param");
            if (k != null)
            {
                foreach (var node in k)
                {
                    string name = node.GetAttributeValue("name", "");
                    string value = node.GetAttributeValue("value", "");
                    if (!string.IsNullOrEmpty(name) & _paramDictionary.ContainsKey(name) == false) _paramDictionary[name] = value;
                }
            }

        }

        public string GetDiv(string id) => _divDictionary[id];

        public string GetDiv(string id, params SetValue[] sets) { 
            string deep = _divDictionary[id];
            foreach (var set in sets) { 
               deep= deep.Replace("$" + set.Name + "$", set.Value);
            }
            return deep;        
        }

        /// <summary>
        /// Показать собранный отчет
        /// </summary>
        public static void OpenReport(string _text)
        {
            string a = "<html lang=\"ru\"><head><meta http-equiv=Content-Type content=\"text/html;\"></head><body lang=RU style='word-wrap:break-word'>";
            string s = "</body></html>";
            string file = AppDomain.CurrentDomain.BaseDirectory + @"Res\Отчет_div.html";
            File.WriteAllText(file, a+ _text + s, System.Text.Encoding.Default);
            Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
        }


        public readonly struct SetValue {
            public readonly string Name;
            public readonly string Value;

            public SetValue(string name, string value)
            {
                this.Name = name;
                this.Value = value;
            }
            public SetValue(string name, double value)
            {
                this.Name = name;
                this.Value = value.ToString();
            }
            public SetValue(string name, decimal value)
            {
                this.Name = name;
                this.Value = value.ToString();
            }
        } 
    }
}
