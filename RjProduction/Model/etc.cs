
using System.Reflection;

namespace RjProduction.Model
{
    /// <summary>
    /// Статусы документа
    /// </summary>
    public enum StatusEnum
    {
        Не_Проведен = 0, Проведен = 1, Ошибка = 6, Частично = 2, НеСохранен = 4, Изменён = 5
    }
    /// <summary>
    /// Типы пород дерева
    /// </summary>
    public enum TypeWoodEnum : int
    {
        Любой,
        Хвоя,
        Листва,
        Сосна,
        Береза,
        Осина,
        Дуб,
        Ель,
        Пихта,
        Лиственица,
        Кедр,
        Ясень,
        Клен,
        Вяз,
        Тополь,
        Ольха,
        Липа,
        Ива
    }
    
    /// <summary>
    /// Коды документов. Нужны для обмена между разными программами 00 - версия документа F - тип документа 00 - код документа
    /// </summary>
    public struct DocCode
    {
        public const string Просмотор_остатков = "";
        public const string Производство_Cклад = "01А02";
        public const string Выравнивание_Остатков = "03A01";
        public const string Перемещение_По_Складам = "03A02";
        public const string Списание_Продукции = "03A03";
        public const string Продажи = "03A04";

        /// <summary>
        /// Выгрузить все названия документов в массив
        /// </summary>
        /// <returns></returns>
        public static string[] ToArray() =>
            [.. typeof(DocCode).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Select(x => x.Name.Replace("_", " "))];
        /// <summary>
        /// Получить код по названию документа
        /// </summary>
        /// <param name="const_code">текст проходит фильтер на замену пробелов на символ _</param>
        /// <returns></returns>
        public static string ToCode(string const_code)
        {
            FieldInfo[] codes = typeof(DocCode).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (var code in codes)
            {
                if (code.Name == const_code.Replace(" ", "_"))
                {
                    object? obj = code.GetValue(null);
                    if (obj is not null) return obj.ToString() ?? string.Empty;
                    else break;
                }
            }
            return string.Empty;
        }


    }


}
