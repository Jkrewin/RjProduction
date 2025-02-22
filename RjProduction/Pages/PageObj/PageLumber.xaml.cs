using RjProduction.Model;
using RjProduction.WpfFrm;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RjProduction.Pages
{

    public partial class PageLumber : Page, IKeyControl
    {
        private readonly Action CloseAction;
        private readonly Action<Model.IDoc> ActionOne;
        private MaterialObj _Material;



        public PageLumber(MaterialObj material, Action<IDoc> act, Action closeAction)
        {
            InitializeComponent();
            CloseAction = closeAction;
            ActionOne = act;
            _Material = material;
        }

        /// <summary>
        /// Обновляет поле и добовляет значение в общюю структуру
        /// </summary>
        /// <param name="cntr">Какое значение следует изменить</param>
        /// <param name="sender">Texbox</param>
        /// <param name="str">По умолчанию значение измененное поменяет значение в TexBox</param>
        private void ChangeField(ref double cntr, object sender, string str = "")
        {
            if (!string.IsNullOrEmpty(str)) ((TextBox)sender).Text = str;
            if (ushort.TryParse(((TextBox)sender).Text, out ushort r)) cntr = r;
            ((TextBox)sender).Text = cntr.ToString();

            if (_Material.MaterialType == MaterialObj.MaterialTypeEnum.Количество)
            {
                // количество
                LabelTest.Content = _Material.Cub;
                if (LabelОбъем != null) LabelОбъем.Content = Math.Round(_Material.CubatureAll, 3);
                Label_Amount.Content = _Material.Amount;
            }
            else if (_Material.MaterialType == MaterialObj.MaterialTypeEnum.Объем)
            {
                // объем
                Label_Amount.Content = _Material.Amount;
                LabelTest.Content = Math.Round(_Material.Cub, 3);
            }
        }

        private void ЗакрытьФорму(object sender, RoutedEventArgs e) => CloseAction?.Invoke();

        private void ОК_Согласие(object sender, RoutedEventArgs e)
        {
            if (_Material.HeightMaterial == 0 | _Material.LongMaterial == 0 | _Material.WidthMaterial == 0)
            {
                MessageBox.Show("Некоторые из полей размеров п/м не заполнены");
                return;
            }
            if (_Material.Quantity == 0)
            {
                MessageBox.Show("Количество не заполнено");
                return;
            }

            if (TypeWood.SelectedValue is not null)           
                _Material.TypeWood = (TypeWoodEnum)Enum.Parse(typeof(Model.TypeWoodEnum), TypeWood.SelectedValue.ToString() ?? nameof(Model.TypeWoodEnum.Любой));            
            else _Material.TypeWood = TypeWoodEnum.Любой;

            // добавить в справочник только п/м
            if (SelectorQ2.IsChecked == false)
            {
                if (!MDL.MyDataBase.MaterialsDic.Any(x => x == _Material))
                {
                    MDL.MyDataBase.MaterialsDic.Add(_Material);
                }
            }
           

            ActionOne?.Invoke(_Material);
            ЗакрытьФорму(null!, null!);
        }

        private void ВходТ_поле(object sender, RoutedEventArgs e) => ((TextBox)sender).Text = "";
        private void СохранитьШирину(object sender, RoutedEventArgs e) => ChangeField(ref _Material.WidthMaterial, sender);
        private void СохранитьВысоту(object sender, RoutedEventArgs e) => ChangeField(ref _Material.HeightMaterial, sender);
        private void СохранитьКоличество(object sender, RoutedEventArgs e) => ChangeField(ref _Material.Quantity, sender);
        private void СохранитьЦену(object sender, RoutedEventArgs e) => ChangeField(ref _Material.Price, sender);

        private void СохранитьДлинну(object sender, RoutedEventArgs e)
        {
            if (((TextBox)sender).Text.Length == 1) ((TextBox)sender).Text += "000";
            ChangeField(ref _Material.LongMaterial, sender);
        }

        private void ИзменениеТекста(object sender, TextChangedEventArgs e)
        {
            if (LabelОбъем != null)
            {
                if (int.TryParse(TBoxКоличество.Text, out int i))
                {
                    LabelОбъем.Content = Math.Round((_Material.Cub * i), 3);
                    if (int.TryParse(TBoxPrice.Text, out int ii))
                    {
                        Label_Amount.Content = Math.Round((_Material.Cub * i) * ii, 2);
                    }
                }
            }
        }

        private void ВыборОбъем(object sender, RoutedEventArgs e)
        {
            Lab_Title.Content = "(см)Ш  х  (см)В х  (см)Д     Кофф";
            TBoxКоличество.Visibility = Visibility.Hidden;
            _Material.Quantity = 1;
            ChangeField(ref _Material.Quantity, TBoxКоличество, "1");
            LabelMaterialType.Content = MaterialObj.MaterialTypeEnum.Объем.ToString();
            _Material.MaterialType = MaterialObj.MaterialTypeEnum.Объем;
            TBoxКофф.Visibility = Visibility.Visible;
        }

        private void ВыборКоличество(object sender, RoutedEventArgs e)
        {
            Lab_Title.Content = "(мм)Ш  х  (мм)В х  (см)Д      (шт) Кол";
            TBoxКоличество.Visibility = Visibility.Visible;
            _Material.Quantity = double.Parse(TBoxКоличество.Text);
            ChangeField(ref _Material.Quantity, TBoxКоличество);
            LabelMaterialType.Content = MaterialObj.MaterialTypeEnum.Количество.ToString();
            _Material.MaterialType = MaterialObj.MaterialTypeEnum.Количество;
            TBoxКофф.Visibility = Visibility.Collapsed;
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            var result = (from tv in Enum.GetNames(typeof(Model.TypeWoodEnum)) select new DeliveredStruct(tv)).ToList();    
            TypeWood.ItemsSource = result;
            TypeWood.Items.Refresh();

            ListBoadr.ItemsSource = MDL.MyDataBase.MaterialsDic;
            ListBoadr.DisplayMemberPath = "NameMaterial";

            TBoxВысота.Text = _Material.HeightMaterial.ToString();
            TBoxДлинна.Text = _Material.LongMaterial.ToString();
            TBoxШирина.Text = _Material.WidthMaterial.ToString();
            TBoxКоличество.Text = _Material.Quantity.ToString();
            if (_Material.MaterialType == MaterialObj.MaterialTypeEnum.Количество)
            {
                ВыборКоличество(null!, null!);
                SelectorQ.IsChecked = true;
            }
            else
            {
                ВыборОбъем(null!, null!);
                SelectorQ2.IsChecked = true;
            }
            TBoxPrice.Text = _Material.Price.ToString();

            if (_Material.TypeWood != TypeWoodEnum.Любой) {
                for (int i = 0; i < TypeWood.Items.Count; i++)
                {
                    if (TypeWood.Items[i].ToString() == _Material.TypeWood.ToString()) {
                        TypeWood.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void ВыполнитьВыбор(object sender, EventArgs e)
        {
            if (ListBoadr.Text == "") return;
            MaterialObj x = (MaterialObj)ListBoadr.SelectedItem;
            ChangeField(ref _Material.WidthMaterial, TBoxШирина, x.WidthMaterial.ToString());
            ChangeField(ref _Material.HeightMaterial, TBoxВысота, x.HeightMaterial.ToString());
            ChangeField(ref _Material.LongMaterial, TBoxДлинна, x.LongMaterial.ToString());
            ChangeField(ref _Material.Price, TBoxPrice, x.Price.ToString());
            int i = 0;
            foreach (var item in Enum.GetNames(typeof(Model.TypeWoodEnum)))
            {
                if (item == x.TypeWood.ToString())
                {
                    TypeWood.SelectedIndex = i;
                    break;
                }
                i++;
            }
            TBoxКоличество.Focus();
        }

        private void СохранитьКофф(object sender, RoutedEventArgs e)
        {
            ChangeField(ref _Material.Ratio, sender);            
        }

        public void HandleKeyPress(object sender, KeyEventArgs e)
        {           
                if (e.Key == Key.F1) ОК_Согласие(null!, null!);
                else if (e.Key == Key.Escape) CloseAction?.Invoke();
        }
    }
}
