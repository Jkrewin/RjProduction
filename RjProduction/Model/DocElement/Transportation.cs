
namespace RjProduction.Model.DocElement
{
    /// <summary>
    /// Грузоперевозки
    /// </summary>
    public class Transportation: IDoc
    {
        /// <summary>
        /// Транспорт
        /// </summary>
        public required Catalog.TransportPart Track;
        /// <summary>
        /// Место отправления
        /// </summary>
        public required Catalog.AddresStruct StartPlace;
        /// <summary>
        /// Место доставки
        /// </summary>
        public required Catalog.AddresStruct EndPlace;

        public  double CubatureAll => 0;
        public  decimal Amount => 0;
        public double UpRaise {  get => 0; set => throw new NotImplementedException(); }
    }
}
