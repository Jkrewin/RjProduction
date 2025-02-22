
using System.Xml;
using RjProduction.WpfFrm;
using System.IO;
using RjProduction.Pages.MainPage;

namespace RjProduction.Fgis
{
    public class KindTypes
    {
        private readonly PageFgis.Config_fgis Config;
        private readonly Dictionary<TypesEnum, string> _kinds = [];
        private Dictionary<TypesEnum, DeliveredStruct[]> _xsd = [];


        public KindTypes(PageFgis.Config_fgis config )
        {
            Config = config;
            //_kinds.Add(TypesEnum.none, "");
            _kinds.Add(TypesEnum.dConstituentEntity, "dConstituentEntityKindTypes-");
            _kinds.Add(TypesEnum.dSubordinateAuthority, "dSubordinateAuthorityKindTypes-");
            _kinds.Add(TypesEnum.dCountry, "dCountryKindTypes-");
            _kinds.Add(TypesEnum.dTypesIdentity, "dTypesIdentityDocKindTypes-");
            _kinds.Add(TypesEnum.dDocumentType, "dDocumentTypeKindTypes-");
            _kinds.Add(TypesEnum.dUsageType, "dUsageTypeKindTypes-");
            _kinds.Add(TypesEnum.dForestry, "dForestryKindTypes-");
            _kinds.Add(TypesEnum.dDistrictForestries, "dDistrictForestriesKindTypes-");
            _kinds.Add(TypesEnum.dForestFacilities, "dForestFacilitiesKindTypes-");
            _kinds.Add(TypesEnum.dForestNonInfrastruct, "dForestNonInfrastructKindTypes-");
            _kinds.Add(TypesEnum.dFellingForest, "dFellingForestKindTypes-");
            _kinds.Add(TypesEnum.dCuttingGroupCategory, "dCuttingGroupCategoryKindTypes-");
           // _kinds.Add(TypesEnum.dFarmsKind, "dFarmsKindTypes-");
            _kinds.Add(TypesEnum.dOkpd2, "dOkpd2KindTypes-");
            _kinds.Add(TypesEnum.dTreeNewKind, "dTreeNewKindTypes-");
            _kinds.Add(TypesEnum.dUnitMeasurement, "dUnitMeasurementKindTypes-");
            _kinds.Add(TypesEnum.dForestResours, "dForestResoursKindTypes-");
            _kinds.Add(TypesEnum.dToForestDeclaration, "dToForestDeclarationKindTypes-");
            _kinds.Add(TypesEnum.none, "");

            // Обновим файл
            var ff = Directory.GetFiles(Config.Common).Select(x => Path.GetFileName(x));
            foreach (var item in _kinds)
            {
                foreach (var f in ff)
                {
                    if (item.Value == f.Substring(0, Math.Min(f.Length, item.Value.Length)))
                    {
                        _kinds[item.Key] = f;
                        break;
                    }
                }
            }

            Loader();
        }
        /// <summary>
        /// Получить полный списов значений 
        /// </summary>
        /// <param name="typ">sKindTypes</param>
        /// <param name="filter">Поиск по слову которое находиться в название </param>
        public DeliveredStruct[] GetDate(TypesEnum typ, string filter = "")
        {
            if (filter == "") return _xsd[typ];
            else return (from tv in _xsd[typ] where tv.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1 select tv).ToArray();
        }
        /// <summary>
        /// Получить название по id 
        /// </summary>
        /// <param name="typ">sKindTypes</param>
        /// <param name="id">Название id который лежив в комменте</param>
        /// <returns>Name</returns>
        public string GetName(TypesEnum typ, string id)
        {
            foreach (var tv in _xsd[typ])
            {
                if (tv.Comment == id) return tv.Name;
            }
            return "";
        }

        private async void Loader()
        {
            _xsd = [];
            await Task.Run(() =>
            {
                foreach (var item in _kinds)
                {
                    XmlDocument xsdDoc = new();
                    string s = Config.Common + "\\" + item.Value;
                    if (File.Exists(s) == false) throw new FileNotFoundException(s);
                    xsdDoc.Load(s);
                    XmlElement? schemaElement = xsdDoc.DocumentElement ?? throw new Exception("Нет элементов для XmlElement");
                    XmlNodeList simpleTypeNodes = schemaElement.GetElementsByTagName("xs:enumeration");
                    _xsd.Add(item.Key, (from XmlNode tv in simpleTypeNodes select new WpfFrm.DeliveredStruct(tv.LastChild!.InnerText, 0, tv.Attributes!["value"]!.Value)).ToArray());
                }
            });
        }

        public enum TypesEnum
        {
            none,
            /// <summary>
            /// Государственные (муниципальные ) учреждения, подведомственные ОГВ субъектов РФ
            /// </summary>
            dSubordinateAuthority,
            /// <summary>
            /// Субъекты Российской Федерации
            /// </summary>
            dConstituentEntity,
            /// <summary>
            /// Страны мира
            /// </summary>
            dCountry,
            /// <summary>
            /// Типы документов, удостоверяющих личность
            /// </summary>
            dTypesIdentity,
            /// <summary>
            /// Типы документов
            /// </summary>
            dDocumentType,
            /// <summary>
            /// Виды использования лесов
            /// </summary>
            dUsageType,
            /// <summary>
            /// Лесничества
            /// </summary>
            dForestry,
            /// <summary>
            /// Участковые лесничества
            /// </summary>
            dDistrictForestries,
            /// <summary>
            /// Объекты лесной инфраструктуры
            /// </summary>
            dForestFacilities,
            /// <summary>
            /// Объекты, не связанные с созданием лесной инфраструктуры
            /// </summary>
            dForestNonInfrastruct,
            /// <summary>
            /// Форма рубки
            /// </summary>
            dFellingForest,
            /// <summary>
            /// Виды рубки
            /// </summary>
            dCuttingGroupCategory,
            /// <summary>
            /// Хозяйства
            /// </summary>
            dFarmsKind,
            /// <summary>
            /// Общероссийский классификатор продукции по видам экономической деятельности (ОКПД2)
            /// </summary>
            dOkpd2,
            /// <summary>
            /// Породы деревьев
            /// </summary>
            dTreeNewKind,
            /// <summary>
            /// Единицы измерения
            /// </summary>
            dUnitMeasurement,
            /// <summary>
            /// Лесные ресурсы и лекарственные растения
            /// </summary>
            dForestResours,
            /// <summary>
            /// Вид использования объектов лесной инфраструктуры для лесной декларации
            /// </summary>
            dToForestDeclaration


        }
    }
}
