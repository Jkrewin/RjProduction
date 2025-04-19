
namespace RjProduction.Model.Catalog
{
    /// <summary>
    /// Транспортная часть
    /// </summary>
    public class TransportPart: RjProduction.Model.DocElement.IDoc
    {
        public TruckClass Truck { get; set; } = new();

        /// <summary>
        /// Собственный транспорт 
        /// </summary>
        public bool OwnTransport
        {
            get
            {
                if (Truck.CargoCarriers is null & MDL.MyDataBase.CompanyOwn is null) return false;
                return Truck.CargoCarriers == MDL.MyDataBase.CompanyOwn;
            }
        }

        public AddresStruct AddresFrom { get; set; }
        public AddresStruct AddresTo { get; set; }

        public decimal Amount => throw new NotImplementedException();

        public double CubatureAll => throw new NotImplementedException();

        public override string ToString() => Truck.CarNumber + "=>" + AddresTo.Address;
    }
}
