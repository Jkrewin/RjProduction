
using RjProduction.Sql;
using System.Data;

namespace RjProduction.Model
{
    public class Products : SqlParam
    {
        private double _Cubature;
        
        /// <summary>
        /// Название пиломатериала. Для круглого леса есть переменная NameKB
        /// </summary>
        public string NameItem { get; set; } = "def name";
        
        /// <summary>
        /// Общая Кубатура
        /// </summary>
        public double Cubature
        {
            get => _Cubature;
            set
            {
                unchecked
                {
                    _Cubature = value;
                }
            }
        }
        /// <summary>
        /// На этом складе продукция
        /// </summary>
        public WarehouseClass Warehouse { get; set; } = new WarehouseClass();
        /// <summary>
        /// Тип леса
        /// </summary>
        public TypeWoodEnum TypeWood { get; set; } = TypeWoodEnum.Любой;
        /// <summary>
        /// Кубатура одной штуки (нужен для штучного расчета)
        /// </summary>
        public double OnePice { get; set; }

        /// <summary>
        /// Цена за один куб
        /// </summary>
        [SqlIgnore] public double Price { get; set; }
        /// <summary>
        /// Сумма
        /// </summary>
        [SqlIgnore] public decimal Amount => (decimal)(Price * _Cubature);

        public Products() { }

        public Products(DataRow dataRow, Dictionary<long, WarehouseClass> WarehouseHub)
        {
           
            OnePice = 0;
            var r = dataRow["OnePice"];
            if (double.TryParse(r.ToString(), out double dou)) OnePice = dou;
            NameItem = dataRow["NameItem"].ToString() ?? "def name";
            FullSet(dataRow);
            
            if (double.TryParse(dataRow["Cubature"].ToString(),out double d)) _Cubature = d;
            if (dataRow["TypeWood"] is TypeWoodEnum e) TypeWood = e;

            if (long.TryParse(dataRow["Warehouse"].ToString(), out long w))
            {
                if (WarehouseHub.TryGetValue(w, out WarehouseClass? value)) Warehouse = value;
            }
        }

        /// <summary>
        /// Запрос на плюс или минус кубов, к общим остаткам. Помогоает избежать ошибку конкурентного доступа без монопольго режима 
        /// </summary>
        /// <param name="cubature">Количестко которое нужно добавить или уменьшить</param>
        /// <param name="id">ид объекта</param>
        /// <param name="operaton">операция + / -</param>
        public static void ConcurrentReqest(double cubature, OperatonEnum operaton,long id)
        {
            string op = operaton == OperatonEnum.vsPlus ? "+" : "-";
            string sql;
            sql = $"UPDATE Products SET Cubature=(SELECT Cubature FROM Products WHERE id={id}){op}'{cubature}' WHERE id={id}";

            if (MDL.SqlProfile == null) MDL.LogError("Необходимо подключить профиль подключения к БД");
            else
            {
                try
                {
                    MDL.SqlProfile.Conection();
                    MDL.SqlProfile.SqlCommand(sql);
                }
                catch (Exception ex)
                {
                    MDL.LogError("Ошибка отправки запроса", ex.Message);
                }
                finally
                {
                    MDL.SqlProfile.Disconnect();
                }
            }
        }

        /// <summary>
        /// Запрос на плюс или минус кубов, к общим остаткам. Помогоает избежать ошибку конкурентного доступа без монопольго режима 
        /// </summary>
        /// <param name="cubature">Количестко которое нужно добавить или уменьшить</param>
        /// <param name="id">ид объекта</param>
        /// <param name="operaton">операция + / -</param>
        public void ConcurrentReqest(double cubature, OperatonEnum operaton) 
        {
            string op = operaton == OperatonEnum.vsPlus ? "+" : "-";
            string sql;
            sql = $"UPDATE Products SET Cubature=(SELECT Cubature FROM Products WHERE id={ID}){op}'{cubature}' WHERE id={ID}";

            if (MDL.SqlProfile == null) MDL.LogError("Необходимо подключить профиль подключения к БД");
            else
            {
                try
                {
                    MDL.SqlProfile.Conection();
                    MDL.SqlProfile.SqlCommand(sql);
                }
                catch (Exception ex)
                {
                    MDL.LogError("Ошибка отправки запроса", ex.Message);
                }
                finally
                {
                    MDL.SqlProfile.Disconnect();
                }
            }
        }
        /// <summary>
        /// Преобразует тип в текст для круглого леса как в бд название
        /// </summary>
        public static string TypeWoodStr(TypeWoodEnum type) => type switch
        {
            TypeWoodEnum.Хвоя => "Круглый лес хвойный",
            TypeWoodEnum.Листва => "Круглый лес лиственный",
            _ => "Круглый леc"

        };
        /// <summary>
        /// Мат операции
        /// </summary>
        public enum OperatonEnum { 
            vsPlus, vsMunis
        }
    }
}
