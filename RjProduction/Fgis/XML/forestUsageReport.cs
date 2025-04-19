

using RjProduction.WpfFrm;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml.Serialization;
using static RjProduction.Fgis.Mod;

namespace RjProduction.Fgis.XML
{
#pragma warning disable IDE1006 // Стили именования
    public class forestUsageReport
    {
        static readonly string Guid_Text = System.Guid.NewGuid().ToString();       
        /// <summary>
        /// Сохранить файл XML
        /// </summary>
        public void SaveXml(string sFile)
        {
            XmlSerializerNamespaces xmlns = new();
            foreach (var item in typeof(forestUsageReport).GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            {
                // сюда загрузить все private const string
                xmlns.Add(item.Name, item.GetValue(this)!.ToString());
            }

            try
            {
                XmlSerializer xmlSerializer = new(typeof(forestUsageReport));
                using FileStream fs = new(sFile, FileMode.Create, FileAccess.Write, FileShare.None);
                fs.SetLength(0);
                xmlSerializer.Serialize(fs, this, xmlns);
                fs.Close();
            }
            catch (Exception ex)
            {
                MDL.LogError($"Ошибка при сохрании в файл {this}", sFile + "\n   " + ex.Message + "\n" + ex.Source + "\n" + ex.InnerException);
            }
        }

        private const string tns = "http://rosleshoz.gov.ru/xmlns/forestUsageReport/types/5.0.3";  //xmlns="http://rosleshoz.gov.ru/xmlns/forestUsageReport/types/5.0.3"
        private const string comR = "http://rosleshoz.gov.ru/fgis-lk/common/commonReport/types/2.1.3";
        private const string sub = "http://rosleshoz.gov.ru/fgis-lk/common/subject/types/2.2.6";    // ns4
        private const string st = "http://rosleshoz.gov.ru/fgis-lk/common/simple/types/2.2.1";
        private const string complex = "http://rosleshoz.gov.ru/fgis-lk/common/complex/types/2.1.2"; //ns3
        private const string doc = "http://rosleshoz.gov.ru/fgis-lk/common/document/types/2.2.5";   //ns5
        private const string attach = "http://rosleshoz.gov.ru/fgis-lk/common/attachment/types/2.1.2";  //ns7
       // private const string addr = "http://rosleshoz.gov.ru/fgis-lk/common/address/types/2.1.2";  //ns6


        [Mod(MTypeEnum.Optional, FormatEnum.Text, min: 0, max: 1000)]
        public string ID { get; set; } = Guid_Text;

        public ServiceInfoType serviceInfo = new();
        [XmlTypeAttribute(Namespace = tns)]
        public class ServiceInfoType
        {

            [Mod(MTypeEnum.Optional, FormatEnum.Text)]
            public string? provider { get; set; } = System.AppDomain.CurrentDomain.FriendlyName;

            [Mod(MTypeEnum.Optional, FormatEnum.Text)]
            public string? guid { get; set; } = Guid_Text;

        }

        public headerClass header = new();
        public class headerClass
        {

            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 128)]
            public string number { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.Date)]
            public string date { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dConstituentEntity)]
            public string subject { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dSubordinateAuthority)]
            public string executiveAuthority { get; set; } = string.Empty;

            public ForestUserType partner = new();
            [XmlTypeAttribute(Namespace = comR)]
            public class ForestUserType
            {
                public TypeivPrivatePersonType? PrivatePerson { get; set; }
                public TypeivBusinessmanType? Businessman { get; set; }
                public TypeivForeignPrivatePersonType? ForeignPrivatePerson { get; set; }
                public TypeivForeignBusinessmanType? ForeignBusinessman { get; set; }
                public TypeiiOrganizationType? Organization { get; set; }
                public TypeivForeignOrganizationType? ForeignOrganization { get; set; }
                public PersonRussianFederationType? personRussianFederation { get; set; }
                public PersonSubjectRussianFederationType? personSubjectRussianFederation { get; set; }
                public PersonMunicipalityType? personMunicipality { get; set; }
                public PersonUnionStateType? personUnionState { get; set; }
                public PersonForeignStateType? personForeignState { get; set; }
            }

            public PeriodType period = new();
            [XmlTypeAttribute(Namespace = comR)]
            public class PeriodType
            {
                [Mod(MTypeEnum.Required, FormatEnum.Date)]
                public string begin { get; set; } = string.Empty;

                [Mod(MTypeEnum.Required, FormatEnum.Date)]
                public string end { get; set; } = string.Empty;
            }

            public ContractType contract = new();
            [XmlTypeAttribute(Namespace = comR), Mod("Информация о договоре аренды лесного участка или ином документе")]
            public class ContractType
            {
                [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dDocumentType, ui: "Вид документа")]
                public string type { get; set; } = string.Empty;

                [Mod(MTypeEnum.Required, FormatEnum.Text, ui: "Номер договора аренды лесного участка или иного документа")]
                public string number { get; set; } = string.Empty;

                [Mod(MTypeEnum.Required, FormatEnum.Date, ui: "Дата договора аренды лесного участка или иного документа")]
                public string date { get; set; } = string.Empty;

                [Mod(MTypeEnum.Optional, FormatEnum.Text, min: 0, max: 256, ui: "Номер государственной регистрации")]
                public string registrationNumber { get; set; } = string.Empty;
            }

            public UsageTypes usageTypes = new();
            [XmlTypeAttribute(Namespace = tns)]
            public class UsageTypes
            {
                public List<UsageType> usageType = new();
                [XmlTypeAttribute(Namespace = tns)]
                public class UsageType
                {
                    [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dUsageType)]
                    public string usage { get; set; } = string.Empty;
                }
            }

            public SignerDataType signerData = new();
            [XmlTypeAttribute(Namespace = comR)]
            public class SignerDataType
            {
                public EmployeeType employee = new();

                [Mod(MTypeEnum.Optional, FormatEnum.Date)]
                public string date { get; set; } = string.Empty;

                [Mod(MTypeEnum.Optional, FormatEnum.Text)]
                public string organization { get; set; } = string.Empty;

                [Mod(MTypeEnum.Optional, FormatEnum.RegisterNumber)]
                public string registerNumber { get; set; } = string.Empty;
            }
        }

        public ForestUsageReportInfoType forest = new();
        public class ForestUsageReportInfoType
        {
            [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dForestry)]
            public string forestry { get; set; } = string.Empty;

            public GeneralInformationType generalInformation = new();
            public class GeneralInformationType
            {
                public ClearcutAndResinsInfoType? clearcutAndResins;
                public class ClearcutAndResinsInfoType
                {
                    public List<ClearcutAndResinType> clearcutAndResin = [];
                    public class ClearcutAndResinType
                    {
                        [Mod(MTypeEnum.Optional, FormatEnum.Text, min: 0, max: 255)]
                        public string number { get; set; } = string.Empty;

                        [Mod(MTypeEnum.Required, FormatEnum.Text)]
                        public string FGISNumber { get; set; } = string.Empty;

                        public AreaInfoType area = new();
                        public AreaInfoType usageArea = new();
                        public LocationGeneralType location = new();
                        public List<NotesType>? notes;
                    }
                }

                public ObjectsInfoType? objectsInfo;
                public class ObjectsInfoType
                {
                    public List<ObjectInfoType> objectInfo = [];
                    public class ObjectInfoType
                    {
                        [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 255)]
                        public string number { get; set; } = string.Empty;

                        [Mod(MTypeEnum.Optional, FormatEnum.Text)]
                        public string FGISNumber { get; set; } = string.Empty;

                        public ObjectNameType objectName = new();
                        public class ObjectNameType
                        {
                            [Mod(MTypeEnum.Select, dic: KindTypes.TypesEnum.dForestFacilities)]
                            public string? objectForestName { get; set; } = string.Empty;

                            [Mod(MTypeEnum.Select, dic: KindTypes.TypesEnum.dForestNonInfrastruct)]
                            public string? objectNonForestName { get; set; } = string.Empty;

                            [Mod(MTypeEnum.Optional, FormatEnum.Text, min: 0, max: 4000)]
                            public string objectNote { get; set; } = string.Empty;
                        }

                        public ForestSteadNumsType? forestSteadNums;
                        public AreaInfoType area = new();
                        public AreaInfoType usageArea = new();
                        public SubforestryInfoType location = new();                        
                        public List<NotesType>? notes;
                        public List<ObjectClearcutType>? clearcut;
                        
                    }
                }

                public ForestSteadsInfoType? forestSteads;
                public class ForestSteadsInfoType
                {
                    public List<ForestSteadType> forestStead = [];
                    public class ForestSteadType
                    {
                        [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 50)]
                        public string number { get; set; } = string.Empty;

                        public ForestSteadNumsType? FGISNumber;

                        [Mod(MTypeEnum.Optional, FormatEnum.Cadastr)]
                        public string cadNumber { get; set; } = string.Empty;

                        public AreaInfoType? area;
                        public SubforestryInfoType? location;
                        public List<NotesType>? notes;
                    }
                    public List<ObjectClearcutType>? Clearcut;
                }
            }

            public List<harvestingWoodGroup> woodsHarvesting = [];
            public class harvestingWoodGroup
            {
                public List<CuttingInfoRowType> harvestingWood = [];
            }

            public List<NotWoodHarvestingRowType> notWoodsHarvesting = [];
            public class NotWoodHarvestingRowType
            {
                public ForestSteadNumsType? otherUsageType1;
                public OtherUsageType2Type? otherUsageType2;
                public OtherUsageType3Type? otherUsageType3;

                [XmlTypeAttribute(Namespace = tns)]
                public class OtherUsageType2Type
                {
                    [Mod(MTypeEnum.Required, FormatEnum.Text)]
                    public string taxUnitIdRef { get; set; } = string.Empty;
                }
                [XmlTypeAttribute(Namespace = tns)]
                public class OtherUsageType3Type
                {
                    [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 50)]
                    public string number { get; set; } = string.Empty;
                }

                [Mod(MTypeEnum.Required, FormatEnum.Ints)]
                public string area { get; set; } = string.Empty;

                public ResourcesInfoType? resourcesInfo;

            }
            public List<MeasureType> measures = [];
            public class MeasureType
            {
                [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dToForestDeclaration)]
                public string measureName { get; set; } = string.Empty;

                public CuttingInfoRowType? сobjectCutting;
                public HarvestingObjectRowType? objectLinear;
                public class HarvestingObjectRowType
                {
                    [Mod(MTypeEnum.Required, FormatEnum.Text)]
                    public string taxUnitObjectIdRef { get; set; } = string.Empty;
                }
                public CuttingInfoRowType? otherUsageCutting;
            }
        }

        public AttachmentListType? attachments;
        [XmlTypeAttribute(Namespace = attach)]
        public class AttachmentListType
        {
            public List<AttachmentType>? attachmen;
            [XmlTypeAttribute(Namespace = attach)]
            public class AttachmentType
            {
                [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 128)]
                public string id { get; set; } = string.Empty;

                [Mod(MTypeEnum.Optional, FormatEnum.Text, min: 0, max: 255)]
                public string desc { get; set; } = string.Empty;

                public AttachmentFileType file = new();
                public AttachmentFileType? signature;

                [XmlTypeAttribute(Namespace = attach)]
                public class AttachmentFileType
                {
                    [XmlIgnore] public string FilePatch { get; set; } = "";

                    [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 128)]
                    public string fileURI { get; set; } = string.Empty;

                    [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 128)]
                    public string md5sum
                    {
                        get
                        {
                            if (File.Exists(FilePatch)==false) return string.Empty;
                            using MD5 md5 = MD5.Create();
                            using FileStream stream = File.OpenRead(FilePatch);
                            byte[] hashBytes = md5.ComputeHash(stream);
                            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                        }
                        set { }
                    }

                }
            }
        }

        /// ===============================================================================================================================================================================
        #region "Все вложенные классы"

        public class ForestSteadNumsType
        {
            public List<string> forestSteadNumber = [];
        }

        public class SubforestryInfoType
        {
            public List<SubforestryType> subforestryInfo = [];
            public class SubforestryType
            {
                [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dDistrictForestries)]
                public string subforestry { get; set; } = string.Empty;

                public QuarterType quarter = new();
            }
        }

        public class ObjectClearcutType
        {
            [Mod(MTypeEnum.Optional, FormatEnum.Text)]
            public string number { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.Text)]
            public string FGISNumber { get; set; } = string.Empty;

            public AreaInfoType area = new();
            public AreaInfoType usageArea = new();
            public LocationGeneralType location = new();
            public List<NotesType>? notes;
        }

        
        [Mod(ui: " Информация о рубке")]
        public class CuttingInfoRowType
        {
            [Mod(ui: "Рубка лесных насаждений")]
            public CuttingType cutting { get; set; } = new();

            [Mod(MTypeEnum.Required, FormatEnum.Text, ui: "Ссылка на выдел лесосеки"), XmlAttribute]
            public string taxUnitClearcutIdRef { get; set; } = string.Empty;
        }

        public class ResourcesInfoType
        {
            public List<ResourceInfoType> resourceInfo = [];
            public class ResourceInfoType
            {
                [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dForestResours)]
                public string resource { get; set; } = string.Empty;

                [Mod(MTypeEnum.Required, FormatEnum.Ints)]
                public string volume { get; set; } = string.Empty;

                public UnitTypeType unitType = new();
            }
        }

        [Mod(ui: "Рубка лесных насаждений")]
        public class CuttingType
        {
            [Mod(MTypeEnum.Optional, dic: KindTypes.TypesEnum.dFellingForest, ui:"Форма рубки")]
            public string formCutting { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, dic: KindTypes.TypesEnum.dCuttingGroupCategory, ui: "Вид рубки")]
            public string typeCutting { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.Ints, ui: "Площадь рубки, га")]
            public string cuttingArea { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, dic: KindTypes.TypesEnum.dFarmsKind, ui: "Хозяйства")]
            public string farm { get; set; } = string.Empty;

            [Mod(ui: "Информация о породах")]
            public TreesInfoTypeList? treesInfo { get; set; }

            public override string ToString()=> cuttingArea == string.Empty ? "<Необходимо заполнить форму>" : "Рубка лесных насаждений";
        }

        [Mod(ui: "Информация о породе"), XmlType(TypeName = "TreesInfoType")]
        public class TreesInfoTypeList
        {
            [Mod(MTypeEnum.Required, FormatEnum.List,ui: "Информация о породе")]
            public List<TreeInfoType> treeInfo { get; set; } = [];
            [Mod(ui: "Информация о породе")]
            public class TreeInfoType : IDefault
            {
                public void Default(KindTypes kindTypes)
                {
                    var dl = kindTypes.GetDate(KindTypes.TypesEnum.dUnitMeasurement).ToList().Find(x => x.Comment == "113");
                    if (string.IsNullOrEmpty(dl.Comment) == false)
                    {
                        unitType = new UnitTypeType()
                        {
                            abbreviation = dl.Name,
                            unitType = dl.Comment
                        };
                    }
                }

                /// <summary>
                /// Справочник «Общероссийский классификатор продукции по видам экономической деятельности (ОКПД2)»(Okpd2KindEType) dOkpd2KindTypes-
                /// </summary>
                [Mod(MTypeEnum.Optional, dic: KindTypes.TypesEnum.dOkpd2, ui: "Cортиментный состав древесины  ")]
                public string sortiment { get; set; } = string.Empty;

                [Mod(MTypeEnum.Optional, FormatEnum.winClass, ui: "Порода древесины")]
                public TreeType? tree { get; set; }
                [XmlTypeAttribute(Namespace = comR), Mod(ui: "Порода древесины")]
                public class TreeType
                {
                    private const int MAX_SIZE_abbreviation = 128;

                    [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dTreeNewKind, ui: "Породы деревьев")]
                    public string Tree { get; set; } = string.Empty;

                    [Mod(MTypeEnum.Optional, FormatEnum.Hide, min: 0, max: MAX_SIZE_abbreviation, ui: "Сокращение")]
                    public string abbreviation { get; set; } = string.Empty;
                  
                    public void Act_abbreviation(DeliveredStruct delivered) => abbreviation = delivered.Name[..Math.Min(delivered.Name.Length, MAX_SIZE_abbreviation)];
                    public override string ToString() => abbreviation;
                }

                [Mod(MTypeEnum.Required, FormatEnum.winClass, ui: "Единица измерения")]
                public UnitTypeType unitType { get; set; } = new();

                [Mod(MTypeEnum.Required, FormatEnum.Ints, ui: "Объем")]
                public string volume { get; set; } = string.Empty;

                [Mod(MTypeEnum.Optional, FormatEnum.Ints, ui: "Указывается фактический объем полученной деловой древесины")]
                public string commercialValue { get; set; } = string.Empty;

                public override string ToString()
                {
                    string s = "";
                    if (tree is not null) s = tree.abbreviation;
                    return $"{volume} {unitType.abbreviation }  {s} ";
                }
            }

            public override string ToString()=> "Всего в списке:" + treeInfo.Count;
        }

        [XmlTypeAttribute(Namespace = comR), Mod(ui: "Единица измерения")]
        public class UnitTypeType
        {          
            [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dUnitMeasurement, ui: "Единица измерения")]
            public string unitType { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Hide, ui: "Сокращение")]
            public string abbreviation { get; set; } = string.Empty;
                        
            public void Act_abbreviation(DeliveredStruct delivered) => abbreviation = delivered.Name;
            public override string ToString() => abbreviation;
        }

        [XmlTypeAttribute(Namespace = tns)]
        public class NotesType
        {
            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 4000)]
            public string comment { get; set; } = string.Empty;

            public NotePhotoVideoType? content;
        }

        public class NotePhotoVideoType
        {
            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 4000)]
            public string nameContent { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.Date)]
            public string dateContent { get; set; } = string.Empty;
        }

        public class LocationGeneralType
        {
            public ForestSteadNumsType? forestSteadFGISNumber;
           
            [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dDistrictForestries)]
            public string subforestry { get; set; } = string.Empty;

            public QuarterType quarter = new();
        }

        public class QuarterType
        {
            [Mod(MTypeEnum.Select, FormatEnum.Text)]
            public string? quarterNumberFGIS { get; set; } = string.Empty;

            [Mod(MTypeEnum.Select, FormatEnum.Text, min: 0, max: 25)]
            public string? quarterNumberUser { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Text, min: 0, max: 255)]
            public string tract { get; set; } = string.Empty;

            public TaxationUnitsType? taxationUnits;
        }

        public class TaxationUnitsType
        {
            public List<TaxationUnitType> taxationUnit = [];
            public class TaxationUnitType
            {
                [Mod(MTypeEnum.Select, FormatEnum.Text)]
                public string? taxationUnitFGIS { get; set; } = string.Empty;

                [Mod(MTypeEnum.Select, FormatEnum.Text, min: 0, max: 25)]
                public string? taxationUnitUser { get; set; } = string.Empty;

                [Mod(MTypeEnum.Required, FormatEnum.Ints)]
                public string taxationUnitArea { get; set; } = string.Empty;

                [Mod(MTypeEnum.Required, FormatEnum.Text)]
                public string taxUnitId { get; set; } = string.Empty;
            }
        }

        public class AreaInfoType
        {
            [Mod(MTypeEnum.Required, FormatEnum.Ints)]
            public string value { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Ints)]
            public string inaccuracy { get; set; } = string.Empty;
        }

        [XmlTypeAttribute(Namespace = comR)]
        public class EmployeeType
        {
            public PersonNameType fullName = new();

            [Mod(MTypeEnum.Optional, FormatEnum.Text)]
            public string post { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Text)]
            public string basisAuthority { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Text)]
            public string number { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Date)]
            public string date { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Text)]
            public string phone { get; set; } = string.Empty;
        }

        [XmlTypeAttribute(Namespace = sub)]
        public class PersonForeignStateType
        {
            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 4000)]
            public string name { get; set; } = string.Empty;
        }

        [XmlTypeAttribute(Namespace = sub)]
        public class PersonMunicipalityType
        {
            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 4000)]
            public string name { get; set; } = string.Empty;
        }

        [XmlTypeAttribute(Namespace = sub)]
        public class PersonUnionStateType
        {
            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 1000)]
            public string name { get; set; } = string.Empty;
        }

        [XmlTypeAttribute(Namespace = sub)]
        public class PersonRussianFederationType
        {
            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 1000)]
            public string name { get; set; } = string.Empty;
        }

        [XmlTypeAttribute(Namespace = sub)]
        public class PersonSubjectRussianFederationType
        {
            [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dConstituentEntity)]
            public string name { get; set; } = string.Empty;
        }

        [XmlTypeAttribute(Namespace = sub)]
        public class TypeivForeignBusinessmanType
        {
            public PersonNameType name = new();

            [Mod(MTypeEnum.Required, FormatEnum.Date)]
            public string birthDate { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 255)]
            public string birthPlace { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dCountry)]
            public string citizenship { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Text, min: 0, max: 15)]
            public string snils { get; set; } = string.Empty;

            public PersonDocumentIdentityType personDocumentIdentity = new();

            [Mod(MTypeEnum.Required, FormatEnum.INN)]
            public string inn { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.OGRN_ip)]
            public string ogrnip { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Email)]
            public string email { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Text)]
            public string phone { get; set; } = string.Empty;

            public PrivatePersonTranslationRussianType translationRussian = new();
        }

        [XmlTypeAttribute(Namespace = sub)]
        public class TypeivPrivatePersonType
        {
            public PersonNameType name = new();

            [Mod(MTypeEnum.Required, FormatEnum.Date)]
            public string birthDate { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 255)]
            public string birthPlace { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dCountry)]
            public string citizenship { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Text, min: 0, max: 15)]
            public string snils { get; set; } = string.Empty;

            public PersonDocumentIdentityType personDocumentIdentity = new();

            [Mod(MTypeEnum.Optional, FormatEnum.INN)]
            public string inn { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Email)]
            public string email { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Text)]
            public string phone { get; set; } = string.Empty;
        }

        [XmlTypeAttribute(Namespace = sub)]
        public class TypeivForeignPrivatePersonType
        {
            public PersonNameType name = new();

            [Mod(MTypeEnum.Required, FormatEnum.Date)]
            public string birthDate { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 255)]
            public string birthPlace { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dCountry)]
            public string citizenship { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Text, min: 0, max: 15)]
            public string snils { get; set; } = string.Empty;

            public PersonDocumentIdentityType personDocumentIdentity = new();

            [Mod(MTypeEnum.Optional, FormatEnum.INN)]
            public string inn { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Email)]
            public string email { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Text)]
            public string phone { get; set; } = string.Empty;

            public PrivatePersonTranslationRussianType translationRussian = new();
        }

        [XmlTypeAttribute(Namespace = sub)]
        public class TypeivBusinessmanType
        {
            public PersonNameType name = new();

            [Mod(MTypeEnum.Required, FormatEnum.Date)]
            public string birthDate { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 255)]
            public string birthPlace { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dCountry)]
            public string citizenship { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Text, min: 0, max: 15)]
            public string snils { get; set; } = string.Empty;

            public PersonDocumentIdentityType personDocumentIdentity = new();

            [Mod(MTypeEnum.Required, FormatEnum.INN)]
            public string inn { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.OGRN_ip)]
            public string ogrnip { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Email)]
            public string email { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Text)]
            public string phone { get; set; } = string.Empty;
        }

        [XmlTypeAttribute(Namespace = doc)]
        public class PersonDocumentIdentityType
        {

            public TypesIdentityDocKindEType typesIdentityDoc = new();

            [Mod(MTypeEnum.Optional, FormatEnum.Text, min: 0, max: 25)]
            public string series { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 50)]
            public string number { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 255)]
            public string issueName { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.Date)]
            public string issueDate { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Text, min: 0, max: 8)]
            public string issueCode { get; set; } = string.Empty;
        }

        [XmlTypeAttribute(Namespace = tns)]
        public class TypesIdentityDocKindEType
        {
            [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dTypesIdentity)]
            public string code { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 1000)]
            public string value { get; set; } = string.Empty;
        }

        [XmlTypeAttribute(Namespace = sub), Mod(ui: "Юридическое лицо")]
        public class TypeiiOrganizationType
        {
            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 1000, ui: "Наименование, указанное в ЕГРЮЛ")]
            public string nameFull { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.OGRN, ui: "ОГРН - для российского юридического лица")]
            public string ogrn { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.INN, ui: "ИНН - для российского юридического лица")]
            public string inn { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Email, ui: "Адрес электронной почты")]
            public string email { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Text, ui: "Телефон")]
            public string phone { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Text, min: 0, max: 4000, ui: "Перевод на русский язык")]
            public string address { get; set; } = string.Empty;
        }

        [XmlTypeAttribute(Namespace = sub)]
        public class TypeivForeignOrganizationType
        {
            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 1000)]
            public string nameFull { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, dic: KindTypes.TypesEnum.dCountry)]
            public string country { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 50)]
            public string regNumber { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.Date)]
            public string regDate { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 1000)]
            public string regName { get; set; } = string.Empty;

            public ForeignAddressType address = new();

            [Mod(MTypeEnum.Required, FormatEnum.INN)]
            public string inn { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Email)]
            public string email { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Text)]
            public string phone { get; set; } = string.Empty;

            public ForeignOrganizationTranslationRussianType translationRussian = new();
        }

        [XmlTypeAttribute(Namespace = st)]
        public class ForeignAddressType
        {
            [Mod(MTypeEnum.Optional, FormatEnum.Text, min: 0, max: 4000)]
            public string address { get; set; } = string.Empty;
        }

        [XmlTypeAttribute(Namespace = st)]
        public class ForeignOrganizationTranslationRussianType
        {
            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 1000)]
            public string nameFull { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 1000)]
            public string regName { get; set; } = string.Empty;

            public ForeignAddressType address = new();

            public TranslationNotarizedType translationNotarized = new();
        }

        [XmlTypeAttribute(Namespace = complex)]
        public class TranslationNotarizedType
        {
            [Mod(MTypeEnum.Required, FormatEnum.Date)]
            public string notarizeDate { get; set; } = string.Empty;

            public PersonNameType notaryName = new();

            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 255)]
            public string notaryActionNum { get; set; } = string.Empty;
        }

        [XmlTypeAttribute(Namespace = tns)]
        public class PrivatePersonTranslationRussianType
        {
            public PersonNameType name = new();

            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 255)]
            public string birthPlace { get; set; } = string.Empty;

            public PersonDocumentIdentityType personDocumentIdentity = new();

            public TranslationNotarizedType translationNotarized = new();
        }

        [XmlTypeAttribute(Namespace = st), Mod(ui: "ФИО")]
        public class PersonNameType
        {

            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 60, ui: "Фамилия")]
            public string last { get; set; } = string.Empty;

            [Mod(MTypeEnum.Required, FormatEnum.Text, min: 0, max: 60, ui: "Имя")]
            public string first { get; set; } = string.Empty;

            [Mod(MTypeEnum.Optional, FormatEnum.Text, min: 0, max: 60, ui: "Отчество")]
            public string middle { get; set; } = string.Empty;
        }
        #endregion

        /// <summary>
        /// Значение по умолчанию
        /// </summary>
        public interface IDefault {
            /// <summary>
            /// Значение по умолчанию
            /// </summary>
            /// <param name="ls">Список занчений для списка поиск</param>
            /// <param name="name">Название переменной</param>
            /// <returns>true - значение по умолчанию использованно следует обновить ui</returns>
            public void Default(KindTypes kindTypes);
        }
    }
#pragma warning restore IDE1006 // Стили именования
}
