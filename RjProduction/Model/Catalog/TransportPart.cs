
namespace RjProduction.Model.Catalog
{
    /// <summary>
    /// Транспортная часть
    /// </summary>
    public class TransportPart: TruckClass, RjProduction.Model.DocElement.IDoc
    {       
        /// <summary>
        /// Собственный транспорт 
        /// </summary>
        public bool OwnTransport
        {
            get
            {
                if (CargoCarriers is null & MDL.MyDataBase.CompanyOwn is null) return false;
                return CargoCarriers == MDL.MyDataBase.CompanyOwn;
            }
        }

        public decimal Amount => 0;
        public float CubatureAll => 0;

        public override string ToString() => CarNumber + ":" + TrailerNumber;
    }
}
