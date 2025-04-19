using RjProduction.Model;
using RjProduction.Model.DocElement;
using RjProduction.XML;
using System.Windows;
using System.Windows.Controls;
using static RjProduction.MDL;

namespace RjProduction.Pages
{
    public partial class PageReport : Page
    {
        private List<IDocMain>? _docs;

        public Action Report_Gen_МесячнаяЗарплата { get => () => Report_01(); }
        public Action Report_Gen_ВсеДниРабочих { get => () => ReportAllDay(); }
        public Action? SelectReport;

        public PageReport()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Выбор нужного месяца
        /// </summary>
        /// <returns></returns>
        private int SetMonth()
        {
            int mon = 1;
            if (RB_NowMon.IsChecked == true) mon = DateTime.Now.Month;
            else if (RB_LastMon.IsChecked == true & DateTime.Now.Month != 1) mon = DateTime.Now.Month - 1;
            else if (RB_AllMon.IsChecked == true)
            {

                mon = ComboBox_Mon.SelectedIndex + 1;
            }
            return mon;
        }

        private void ГенерироватьОтчет(object sender, RoutedEventArgs e)=> SelectReport?.Invoke();
        private void ФокусМесяца(object sender, RoutedEventArgs e)=> RB_AllMon.IsChecked = true; 

        /// <summary>
        /// отчет за все дни
        /// </summary>
        private void ReportAllDay() {
            _docs = MDL.GetDocuments(DateTime.Now.Year, SetMonth(),DocCode.Производство_Cклад)!;
            if (_docs == null || _docs.Count == 0)
            {
                Label_Error.Visibility = Visibility.Visible;
                Label_Error.Content = "Соответствие с настройками отчета не было найдено документов или сведений по данный параметрам";
                return;
            }

            MDL.HtmlReport report = new(AppDomain.CurrentDomain.BaseDirectory + @"Res\Html\Report_AllDay.htm");
            report.LoadValue(["heading", "mbody", "tabel", "row", "rowf", "final"]);
            report.AddText = report.DicHtml["heading"];

            foreach (var item in _docs)
            {
                string str = report.DicHtml["mbody"];
                HtmlReport.SetValue("MNumber", item.Number.ToString(), ref str);
                HtmlReport.SetValue("MDateCr", item.DataCreate.ToString("dd.MM.yyyy"), ref str);
                report.AddText = str;

                foreach (var tv in item.MainTabel)
                {
                    double sum = 0;
                    str = report.DicHtml["tabel"];
                    HtmlReport.SetValue("MnameGrup", tv.NameGrup, ref str);
                    report.AddText = str;

                    int i = 1;
                    foreach (var tv2 in tv.Tabels)
                    {
                        str = report.DicHtml["row"];
                        HtmlReport.SetValue("text_num", i.ToString(), ref str);
                        HtmlReport.SetValue("text_sum", tv2.Amount.ToString(), ref str);

                        string text = "";
                        if (tv2 is Employee eml)
                        {
                            string r = eml.Worker == false ? eml.Note : "Cдельная";
                            text = $"{eml.NameEmployee} / {eml.Payment}p. {r}";
                        }
                        else if (tv2 is MaterialObj m)
                        {
                            sum += m.CubatureAll;
                            text = $"{m.HeightMaterial}x{m.WidthMaterial}x{m.LongMaterial} кол-во: {m.Quantity} = {Math.Round(m.CubatureAll, 3)} * {m.Price}p; {m.Amount}p.";
                        }
                        else if (tv2 is Tabel_Timbers tt)
                        {
                            sum += tt.CubatureAll;
                            text = "н/о " + tt.ToString();
                        }
                        else text = tv2.ToString() ?? "n/a";

                        HtmlReport.SetValue("text", text, ref str);
                        report.AddText = str;
                        i++;
                    }

                    str = report.DicHtml["rowf"];
                    HtmlReport.SetValue("text_sum", Math.Round(sum, 3).ToString() + " Куб/м3", ref str);
                    report.AddText = str;
                }
            }

            report.AddText = report.DicHtml["final"];
            report.OpenReport();
        }
        /// <summary>
        /// зарплатный отчет
        /// </summary>
        private void Report_01() {
             _docs= MDL.GetDocuments(DateTime.Now.Year, SetMonth(), DocCode.Производство_Cклад)!;
            if (_docs == null || _docs.Count == 0)
            {
                Label_Error.Visibility = Visibility.Visible;
                Label_Error.Content = "Соответствие с настройками отчета не было найдено документов или сведений по данный параметрам";
                return;
            }

            // сборка сведений
            List<(DateOnly, string, decimal)> ls = [];
            foreach (DocArrival s in _docs.Cast<DocArrival>())
            {
                Dictionary<string, decimal> dic = []; // зарплата в течение всего дня у работника
                foreach (var item in s.MainTabel)
                {
                    foreach (var tv in item.Tabels)
                    {
                        if (tv is Employee employee)
                        {
                            if (dic.ContainsKey(employee.NameEmployee)) dic[employee.NameEmployee] += (decimal)employee.Payment;
                            else dic.Add(employee.NameEmployee, (decimal)employee.Payment);
                        }
                    }
                }
                // создаем списко зарплат сотрудников
                foreach (var tv in dic) ls.Add(new(s.DataCreate, tv.Key, tv.Value));
            }

            HashSet<int> list_dat = [];
            foreach (var item in ls) list_dat.Add(item.Item1.Day);// сборка даты для сотрудников
            HashSet<string> name_emp = [];
            foreach (var item in ls) name_emp.Add(item.Item2); // сборка имен сотрудников

            //загрузка отчета
            MDL.HtmlReport report = new(AppDomain.CurrentDomain.BaseDirectory + @"Res\Html\Report01.htm");
            report.LoadValue(["start", "header", "main", "final", "mvs"]);
            report.LoadArr("mvs");

            report.AddText = report.DicHtml["start"];
            string add_header = "";
            foreach (var item in list_dat)
            {
                add_header += report.SetValueArr("text", item.ToString(), "FF001");
            }
            report.AddText = report.SetValueStr("FF001", add_header, "header");

            string main_add = "";
            foreach (var item in name_emp)
            {

                string sst = report.DicHtml["main"];
                HtmlReport.SetValue("FF002", report.SetValueArr("text", item, "FF002"), ref sst);

                string sst2 = "";
                foreach (var item2 in list_dat)
                {
                    sst2 += report.SetValueArr("text", ls.Find(x => x.Item2 == item & x.Item1.Day == item2).Item3.ToString(), "FF003");
                }

                decimal sum = (from x in ls where x.Item2 == item select x).Sum(x => x.Item3);
                HtmlReport.SetValue("FF003", sst2, ref sst);
                HtmlReport.SetValue("FF004", report.SetValueArr("text", sum.ToString(), "FF004"), ref sst);
                main_add += sst;
            }
            report.AddText = main_add;
            report.AddText = report.DicHtml["final"];
            report.OpenReport();
        }

        private void Настройка_отчетов(object sender, RoutedEventArgs e)
        {

        }
    }
}
