using RjProduction.Sql;
using System.Xml.Serialization;
using static RjProduction.Fgis.XML.forestUsageReport;

namespace RjProduction.Model
{
    /// <summary>
    /// Информация о компании или ИП Будет дополнено 
    /// </summary>
    public class Company : SqlParam
    {
        public string FullName { get; set; } = string.Empty;
        public string INN { get; set; } = string.Empty;
        public string OGRN { get; set; } = string.Empty;
        public AddressClass? Address { get; set; }

        [XmlIgnore, SqlIgnore]
        public TypeiiOrganizationType ToTypeiiOrganizationType
        {
            get
            {
                var t = new TypeiiOrganizationType();
                if (Address is not null)
                {
                    t.address = Address.Address;
                    t.phone = Address.Phone;
                    t.email = Address.Email;
                }
                t.nameFull = FullName;
                t.ogrn = OGRN;
                t.inn = INN;
                return t;
            }
            set
            {
                FullName = value.nameFull;
                INN = value.inn;
                OGRN = value.ogrn;
                Address = new AddressClass()
                {
                    Address = value.address,
                    Phone = value.phone,
                    Email = value.email
                };
            }
        }


        public class AddressClass
        {
            public string Phone { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }
    }
}
