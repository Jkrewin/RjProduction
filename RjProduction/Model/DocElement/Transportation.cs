
using RjProduction.Sql;

namespace RjProduction.Model.DocElement
{
    /// <summary>
    /// Грузоперевозки
    /// </summary>
    public class Transportation: IDoc, RjProduction.Model.DocRow.IDocRow
    {
        /// <summary>
        /// Транспорт
        /// </summary>
        public required Catalog.TransportPart Transport;
        /// <summary>
        /// Место отправления
        /// </summary>
        public required Catalog.AddresStruct StartPlace;
        /// <summary>
        /// Место доставки
        /// </summary>
        public required Catalog.AddresStruct EndPlace;
        /// <summary>
        /// Дата транспортировки
        /// </summary>
        public required DateOnly Date { get; set; }

        public  float CubatureAll => 0;
        public  decimal Amount => 0;
        public double UpRaise {  get; set ; }


        public Transportation()
        {
            Transport = new();
            StartPlace = new();
            EndPlace = new();
        }

        public override string ToString()=> Transport.ToString() +" " + StartPlace.Label +">"+ EndPlace.Label;

        public void Send_DB(string id_doc)
        {
            DeliveryWoods delivery = new(this, id_doc) ;
            if (MDL.SqlProfile != null)
            {
                var id = SqlRequest.ExistRecord<DeliveryWoods>(new ISqlProfile.FieldSql("IDdoc", id_doc));
                if (id == -1)
                {
                    SqlRequest.SetData(delivery);
                }
                else
                {
                    SqlRequest.Update(id, delivery);
                }
            }
        }

        public DocRow ToDocRow(string grupname, string id_document)=> new(id_document, grupname, string.Empty, Transport.ToString(), 0, 0, Amount, DocRow.Грузоперевозка, UpRaise, CubatureAll);

        [SqlTabelName("DeliveryWoods")]
        public class DeliveryWoods: SqlParam
        { 
            public string StartPlace { get; set; }
            public string EndPlace { get; set; }
            public string Track { get; set; }
            public float Cubature { get; set; }
            public decimal Summ { get; set; }
            public DateOnly Date { get; set; }
            public string IDdoc { get; set; }

            public DeliveryWoods(Transportation transportation, string idDoc) {
                StartPlace = transportation.StartPlace.Label;
                EndPlace = transportation.EndPlace.Label;
                Cubature = transportation.CubatureAll;
                Summ = transportation.Amount;
                Track = transportation.Transport.CarNumber + ":" + transportation.Transport.TrailerNumber;
                Date= transportation.Date;
                IDdoc = idDoc;
            }

            public DeliveryWoods()
            {
                StartPlace=string.Empty;
                EndPlace=string.Empty;
                Track = string.Empty;
                IDdoc = string.Empty;
            }
        }


    }
}
