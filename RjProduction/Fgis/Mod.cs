
using RjProduction.WpfFrm;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RjProduction.Fgis
{
    /// <summary>
    /// Дает описание, помогает установить обязательные поля также проверяет на ошибки и форматирование строк. + Генерация ui
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Class)]
    public class Mod : System.Attribute
    {
        /// <summary>
        /// Тип данных
        /// </summary>
        public MTypeEnum MType { get; }
        /// <summary>
        /// Формат данных
        /// </summary>
        public FormatEnum Format { get; }
        /// <summary>
        /// Максимум символов 0- без ограничение на символы
        /// </summary>
        public int Max { get; } = 0;
        /// <summary>
        /// Минимум символы  0- без ограничение на минимум символы
        /// </summary>
        public int Min { get; } = 0;
        /// <summary>
        /// Это значит что тут справочник 
        /// </summary>
        public KindTypes.TypesEnum Dic { get; }
        /// <summary>
        /// Необходим название текста поля UI
        /// </summary>
        public string Comment { get; } = string.Empty;
        

        public Mod(MTypeEnum mType, FormatEnum format)
        {
            MType = mType;
            Format = format;
        }
        public Mod(MTypeEnum mType, FormatEnum format, string ui)
        {
            MType = mType;
            Format = format;
            Comment = ui;
        }
        public Mod(MTypeEnum mType, FormatEnum format, int min, int max)
        {
            MType = mType;
            Format = format;
            Min = min;
            Max = max;
        }
        public Mod(MTypeEnum mType, FormatEnum format, int min, int max, string ui)
        {
            MType = mType;
            Format = format;
            Min = min;
            Max = max;
            Comment = ui;
        }
        public Mod(MTypeEnum mType, KindTypes.TypesEnum dic)
        {
            MType = mType;
            Format = FormatEnum.dictionary;
            Dic = dic;
        }
        public Mod(MTypeEnum mType, KindTypes.TypesEnum dic, string ui)
        {
            MType = mType;
            Format = FormatEnum.dictionary;
            Dic = dic;
            Comment = ui;
        }      
        public Mod(string ui)
        {
            Comment = ui;
            Format = FormatEnum.winClass;
        }

        /// <summary>
        /// Извлекает mod
        /// </summary>
        static public Mod? Extract_Mod(PropertyInfo property)
        {
            foreach (var m in property.GetCustomAttributes(false))
            {
                if (m is Mod mod) return mod;
            }
            return null;
        }

        static public bool CheckRule(string text, Mod mod) => CheckRule(new DeliveredStruct(text, 0, text), mod);
        /// <summary>
        /// Проверка на соотвествие правилам 
        /// </summary>
        /// <param name="text">текст</param>
        /// <param name="mod">мод в полю</param>
        /// <returns>false - не соотвествует</returns>
        static public bool CheckRule(DeliveredStruct dl, Mod mod)
        {
            string text = dl.Comment;
            if (string.IsNullOrEmpty(text))
            {
                if (mod.MType == MTypeEnum.Required || mod.MType == MTypeEnum.Select) return false;
                return true;
            }

            if (mod.Min != 0 & text.Length <= mod.Min) return false;
            if (mod.Max != 0 & text.Length >= mod.Max) return false;

            switch (mod.Format)
            {
                case FormatEnum.Hide:
                case FormatEnum.Text:
                    break;
                case FormatEnum.Ints:
                    foreach (char c in text)
                    {
                        if (c != '.')
                        {
                            if (char.IsDigit(c) == false) return false;
                        }
                    }
                    break;
                case FormatEnum.Date:
                    try
                    {
                        DateOnly p = DateOnly.ParseExact(text, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        return false;
                    }
                    break;
                case FormatEnum.Email:
                    return Regex.IsMatch(text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                case FormatEnum.INN:
                    if (text.Length == 12 || text.Length == 10)
                    {
                        if (text.Any(x => char.IsDigit(x) == false)) return false;
                    }
                    else return false;
                    break;
                case FormatEnum.OGRN:
                    if (text.Any(x => char.IsDigit(x) == false) || text.Length != 13) return false;
                    break;
                case FormatEnum.OGRN_ip:
                    if (text.Any(x => char.IsDigit(x) == false) || text.Length != 15) return false;
                    break;
                case FormatEnum.Cadastr:
                    return Regex.IsMatch(text, @"^\d+:\d+:\d+:\d+$");
                case FormatEnum.winClass:
                    if (mod.MType == MTypeEnum.Required || mod.MType == MTypeEnum.Select)
                    {
                        if (dl.Obj is null) return false;
                    }
                    return true;
                case FormatEnum.dictionary:
                    break;
                case FormatEnum.RegisterNumber:
                    return Regex.IsMatch(text, @"^\d{1}-\d{2}-\d{2}-\d{6}-\d{2}/\d{2}-\d{6}$");

            }

            return true;
        }

        public enum FormatEnum
        {
            /// <summary>
            /// Любой текст и цифры
            /// </summary>
            Text,
            /// <summary>
            /// Положительное число до 4 знаков после запятой
            /// </summary>
            Ints,
            /// <summary>
            /// Дата формате yyyy-MM-dd
            /// </summary>
            Date,
            /// <summary>
            /// Электронная почта 60 символов
            /// </summary>
            Email,
            /// <summary>
            /// инн организации 12 символов
            /// </summary>
            INN,
            /// <summary>
            /// огрн 13 символов
            /// </summary>
            OGRN,
            /// <summary>
            /// огрн ип 15 символов
            /// </summary>
            OGRN_ip,
            /// <summary>
            /// Кадастровый номер лесного участка Пример: 47:14:1203001:814
            /// </summary>
            Cadastr,
            /// <summary>
            /// Окно свойств для класса
            /// </summary>
            winClass,
            /// <summary>
            /// Это справочник
            /// </summary>
            dictionary,
            /// <summary>
            /// Номер реестра специалистов
            /// </summary>
            RegisterNumber,
            /// <summary>
            /// Это поле скрыто от пользователя в UI
            /// </summary>
            Hide,
            /// <summary>
            /// Это пoле список UI
            /// </summary>
            List



        }

        public enum MTypeEnum
        {
            /// <summary>
            /// О – обязательный элемент, должен обязательно присутствовать в XML-документе
            /// </summary>
            Required,
            /// <summary>
            /// Н – необязательный элемент, может как присутствовать, так и отсутствовать в XML-документе
            /// </summary>
            Optional,
            /// <summary>
            /// У – символ, обозначающий условие выбора (или-или), позволяющее присутствовать лишь одному из указанных элементов. В зависимости от заданного условия либо должен обязательно присутствовать только один элемент из представленных в группе условно-зависимых элементов, либо может присутствовать только один элемент из представленных в группе условно-зависимых элементов.
            /// </summary>
            Select,
            /// <summary>
            /// Заполнение не по названию а коду для этого элемента
            /// </summary>
            Code
        }
    }
}
