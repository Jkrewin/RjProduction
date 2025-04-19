using System.ComponentModel.DataAnnotations;

namespace RjProduction.Model.Catalog
{
    /// <summary>
    /// Траспорт 
    /// </summary>
    public class TruckClass
    {
        /// <summary>
        /// Гос номер машины
        /// </summary>
        [RegularExpression(@"^[a-zA-Zа-яА-Я]{1}\d{3}[a-zA-Zа-яА-Я]{2} \d{2}$", ErrorMessage = "Некорректный номер машинны")]
        public string CarNumber { get; set; } = "АА123АА 66";
        /// <summary>
        /// Гос номер прицепа
        /// </summary>
        [RegularExpression(@"^[a-zA-Zа-яА-Я]{2}\d{4} \d{2}$", ErrorMessage = "Некорректный номер прицепа")]
        public string? TrailerNumber { get; set; }
        /// <summary>
        /// Компания владелей транспортом
        /// </summary>
        public Company? CargoCarriers { get; set; }
        /// <summary>
        /// Марка машины
        /// </summary>
        public string? CarLabel { get; set; }
        /// <summary>
        /// Марка прицепа
        /// </summary>
        public string? TrailerLabel { get; set; }
    }
}
