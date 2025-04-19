using RjProduction.Fgis;
using RjProduction.Pages;
using RjProduction.Sql;
using System.Xml.Serialization;
using static RjProduction.Fgis.Mod;
using static RjProduction.Fgis.XML.forestUsageReport;

namespace RjProduction.Model.Catalog
{
    /// <summary>
    /// Информация о компании или ИП 
    /// </summary>
    public class Company : SqlParam, IToolFind  
    {
        [Mod(MTypeEnum.Required, FormatEnum.Text)] 
        public string FullName { get; set; } = string.Empty;

        [Mod(MTypeEnum.Required, FormatEnum.Text)] 
        public string ShortName { get; set; } = string.Empty;

        [Mod(MTypeEnum.Optional, FormatEnum.INN )] 
        public string INN { get; set; } = string.Empty;

        [Mod(MTypeEnum.Optional, FormatEnum.OGRN)] 
        public string OGRN { get; set; } = string.Empty;

        [Mod(MTypeEnum.Optional, FormatEnum.Ints, min:9,max:9)] 
        public string KPP { get; set; } = string.Empty;

        [Mod(MTypeEnum.Optional, FormatEnum.Ints, min: 9, max: 9)] 
        public string BIK { get; set; } = string.Empty;

        [Mod(MTypeEnum.Optional, FormatEnum.Ints, min: 8, max: 8)]
        public string OKPO { get; set; } = string.Empty;

        [Mod(MTypeEnum.Optional, FormatEnum.Ints, min: 20, max: 20)] 
        public string RS { get; set; } = string.Empty;

        [Mod(MTypeEnum.Optional, FormatEnum.Ints, min: 20, max: 20)]
        public string KS { get; set; } = string.Empty;

        [Mod(MTypeEnum.Optional, FormatEnum.Text)]
        public string BankName { get; set; } = string.Empty;

        public AddresStruct Address { get; set; }

        public TypeCompanyEnum TypeCompany { get; set; } = TypeCompanyEnum.Organization;


        public enum TypeCompanyEnum {
            /// <summary>
            /// Физическое лицо РФ
            /// </summary>
            PrivatePerson,
            /// <summary>
            /// Индивидуальный предприниматель РФ
            /// </summary>
            Businessman,
            /// <summary>
            /// Физическое лицо иностранного государства
            /// </summary>
            ForeignPrivatePerson,
            /// <summary>
            /// Индивидуальный предприниматель иностранного государства
            /// </summary>
            ForeignBusinessman,
            /// <summary>
            /// Юридическое лицо РФ
            /// </summary>
            Organization,
            /// <summary>
            /// Юридическое лицо иностранного государства
            /// </summary>
            ForeignOrganization,
            /// <summary>
            /// Российская Федерация
            /// </summary>
            personRussianFederation,
            /// <summary>
            /// Субъект Российской Федерации
            /// </summary>
            personSubjectRussianFederation,
            /// <summary>
            /// Муниципальное образование
            /// </summary>
            personMunicipality,
            /// <summary>
            /// Союзное государство
            /// </summary>
            personUnionState,
            /// <summary>
            /// Иностранное государство
            /// </summary>
            personForeignState
        }

        [XmlIgnore, SqlIgnore]
        public TypeiiOrganizationType ToTypeiiOrganizationType
        {
            get
            {
                var t = new TypeiiOrganizationType();
                if ( string.IsNullOrEmpty( Address.Address))
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
                Address = new AddresStruct()
                {
                    Address = value.address,
                    Phone = value.phone,
                    Email = value.email
                };
            }
        }

        [XmlIgnore, SqlIgnore]
        public string Finder => ShortName;

        public override string ToString() => ShortName;
               
        public override bool Equals(object? obj)
        {
            if (obj is not Company) return false;

            if (this.TypeCompany != ((Company)obj).TypeCompany) return false;
            if (string.IsNullOrEmpty(this.INN))
            {
                return this.FullName.Equals(((Company)obj).FullName, StringComparison.CurrentCultureIgnoreCase)
                    & this.ShortName.Equals(((Company)obj).ShortName, StringComparison.CurrentCultureIgnoreCase);
            }
            else
            {
                return this.INN == ((Company)obj).INN;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
