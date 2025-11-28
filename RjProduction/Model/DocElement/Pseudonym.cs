using RjProduction.Sql;

namespace RjProduction.Model.DocElement
{
    /// <summary>
    /// Общая форма материалов необходимы для учета в общей бд
    /// </summary>
    public class Pseudonym : IDoc, IConvertDoc, DocRow.IDocRow
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
        public required float SelectedCub;
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
        public float CubatureAll
        {
            get
            {
                if (Operation == SqlRequest.OperatonEnum.vsPlus) return (float)(SelectedCub + Product.Cubature);
                else if (Operation == SqlRequest.OperatonEnum.vsMunis) return (float)(Product.Cubature - SelectedCub);
                else if (Operation == SqlRequest.OperatonEnum.vsMutation) return (float)SelectedCub;
                else return (float)Product.Cubature;
            }
        }

        public Products ToProducts() => Product;

        public DocRow ToDocRow(string grupname, string id_document)
        {
            return new(id_document, grupname, Operation.ToString(), Product.NameItem, PriceCng, Math.Round(SelectedCub / Product.OnePice, 0), Amount, DocRow.Пиломатериалы, 0, CubatureAll);
        }

        public void Send_DB(IDocMain doc, GrupObj grp)
        {
           
        }
    }

}
