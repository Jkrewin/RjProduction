
using System.Diagnostics;
using static RjProduction.Model.DocElement.Tabel_Timbers;

namespace RjProduction.Model.DocElement
{
    /// <summary>
    /// Таблица для учета бревен по штучно
    /// </summary>
    public class Tabel_Timbers : IDoc, IConvertDoc, DocRow.IDocRow
    {
        public List<Timber> Timbers = [];
        public decimal Amount
        {
            get
            {
                return (decimal)(from tv in Timbers select (tv.Куб_М * (tv.Цена + UpRaise))).Sum(x => x);
            }
        }
        public float CubatureAll { get => (float)Timbers.Sum(x => x.Куб_М); }
        public double UpRaise { get; set; }
        public TypeWoodEnum TypeWood { get; set; } = TypeWoodEnum.Любой;

        public void Send_DB()
        {
           
        }

        public void Send_DB(string id_doc)
        {
           
        }

        public DocRow ToDocRow(string grupname, string id_document)
        {
            return new(id_document, grupname, string.Empty, ToProducts().NameItem, -1, Timbers.Sum(x => x.Количество), Amount, DocRow.КруглыйЛес, UpRaise, CubatureAll);
        }

        public Products ToProducts()
        {
            return new Products()
            {
                NameItem = "Круглый лес (" + TypeWood.ToString() + ")",
                OnePice = 0,
                Cubature = CubatureAll,
                TypeWood = TypeWoodEnum.Любой,
                Price = (Timbers.Sum(x => x.Цена) / Timbers.Sum(x => x.Количество))
            };
        }

        public override string ToString() => "Бревен: " + Timbers.Sum(x => x.Количество) + " Куб/м " + Math.Round(Timbers.Sum(x => x.Куб_М), 3) + (UpRaise == 0 ? "" : " + к цене по итогу:" + UpRaise + "p.");

        /// <summary>
        /// Бревна для таблици 
        /// </summary>
        public class Timber : ICloneable
        {
            public int Диаметр { get; set; }
            public double Длинна { get; set; }
            public int Количество { get; set; }
            public double Куб_М { get; set; }
            public double Цена { get; set; }
            public decimal Сумма { get => (decimal)(Куб_М * (Цена + UpRaise)); }
            public double UpRaise { get; set; }

            public object Clone() => MemberwiseClone();

        }
    }
}
