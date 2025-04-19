using RjProduction.Sql;

namespace RjProduction.Model.DocElement
{
    /// <summary>
    /// Общая форма материалов необходимы для учета в общей бд
    /// </summary>
    public class Pseudonym : IDoc
    {
        private Products _Product = new();

        /// <summary>
        /// Продукт  обрабатываеммый класс
        /// </summary>
        public required Products Product
        {
            get => _Product;
            set
            {
                NamePseudonym = value.NameItem;
                _Product = value;
            }
        }

        /// <summary>
        /// Может быть измененно не как у Product, это необходимо для документов 
        /// </summary>
        public string NamePseudonym { get; set; } = "";
        /// <summary>
        /// Цена за один куб. Это цена своя а не из Products, измененная цена
        /// </summary>
        public double PriceCng;
        /// <summary>
        /// Выбранная куботура для этого Product
        /// </summary>
        public required double SelectedCub;
        /// <summary>
        /// Операция с кубами
        /// </summary>
        public SqlRequest.OperatonEnum Operation = SqlRequest.OperatonEnum.vsNone;
        /// <summary>
        /// Строка id которая  к этому псевдониму для изменении
        /// </summary>
        public long ID_Prod { get => Product.ID; }
        /// <summary>
        /// Сумма по Pseudonym измененная а не Product там своя 
        /// </summary>
        public decimal Amount => (decimal)(SelectedCub * PriceCng);
        /// <summary>
        /// Проводит расчет кубатуры в итоге из SelectedCub и Product.Cubature
        /// </summary>
        public double CubatureAll
        {
            get
            {
                if (Operation == SqlRequest.OperatonEnum.vsPlus) return SelectedCub + Product.Cubature;
                else if (Operation == SqlRequest.OperatonEnum.vsMunis) return Product.Cubature - SelectedCub;
                else if (Operation == SqlRequest.OperatonEnum.vsMutation) return SelectedCub;
                else return Product.Cubature;
            }
        }

    }

}
