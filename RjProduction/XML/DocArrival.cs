using RjProduction.Model;
using RjProduction.Sql;
using System.Windows;

namespace RjProduction.XML
{
    
    public sealed class DocArrival : XmlProtocol, IDocMain
    {
        public static readonly string DOC_CODE = DocCode.Производство_склад ;


        /// <summary>
        /// Необходимый индификатор для общей бд бля быстрого поиска этого документа
        /// </summary>
        public string ID_Doc
        {
            get
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes($"{DOC_CODE}{DataCreate}+{Number}");
                return Convert.ToHexString(System.Security.Cryptography.MD5.HashData(inputBytes));
            }
        }
        /// <summary>
        /// Заголовок документа
        /// </summary>
        public string DocTitle { get; set; } = "Производство на склад";

        /// <summary>
        /// Текущий склад
        /// </summary>
        public WarehouseClass Warehouse { get; set; }

        /// <summary>
        /// Статус документа
        /// </summary>
        [SqlIgnore] public StatusEnum Status { get; set; } = StatusEnum.Не_Проведен;


        public double AllCubs
        {
            get
            {
                double d = 0;
                foreach (var obj in MainTabel)
                {
                    foreach (var tv in obj.Tabels)
                    {
                        if (tv is MaterialObj material) d += material.CubatureAll;
                        else if (tv is Tabel_Timbers timber) d += timber.CubatureAll;
                    }
                }
                return d;
            }
        }
        public decimal AllSum { get => MainTabel.Sum(x => x.Amount); }

        public DocArrival() { 
            TypeDoc = DOC_CODE + ":Производство на склад";
            Warehouse = new WarehouseClass() { NameWarehouse = "n/a" };
        }

        /// <summary>
        /// Провести документ 
        /// </summary>
        public void CarryOut()        {
            if (MDL.SqlProfile == null)
            {
                MessageBox.Show("Нет активного подключения к БД, создайте новое подключение к БД.");
                return;
            }
            if (Status == StatusEnum.Проведен) return;

            // если была потерена информация или создан одинаковый документ проверка
            var id_doc = SqlRequest.ExistRecord<DocArrival>(new ISqlProfile.FieldSql("ID_Doc",  ID_Doc));
            if (id_doc != -1)
            {
                MessageBox.Show("Этот документ был ранее проведен. С такой датой и номером. Уже зафиксированы в БД изменения, если вам нужно внести изменения, то нужно выполнить корректировку остатков. Создав документ по корректировки остатков на складе. ");
                goto final;
            }


            // Проверка и создание нового склада и поиск склада из файла в локальной БД
            int w = MDL.MyDataBase.Warehouses.FindIndex(x => x.Equals(Warehouse));
            // если в локальной БД нет такого склада
            if (w == -1)
            {
                long lo = SqlRequest.ExistRecord<WarehouseClass>(new ISqlProfile.FieldSql("NameWarehouse", Warehouse.NameWarehouse));
                if (lo == -1)
                {
                    //то создадим его если нет и в общей БД
                    SqlRequest.SetData(Warehouse);
                }
                else
                {
                    // либо обновим инфу о нем 
                    Warehouse = SqlRequest.ReadData<WarehouseClass>(lo);
                }
                MDL.MyDataBase.Warehouses.Add(Warehouse); // новое значение в локальное БД
                MDL.SaveXml<MDL.Reference>(MDL.MyDataBase, MDL.SFile_DB);
            }
            // Создает если есть склада в локальной БД, создадим в общей  БД
            else
            {
                // Если нет в общей БД создадим
                long lo = SqlRequest.ExistRecord<WarehouseClass>(new ISqlProfile.FieldSql("ID", Warehouse.ID.ToString()));
                if (lo == -1)
                {
                    Warehouse.Rebild();
                    SqlRequest.SetData(Warehouse);
                    MDL.MyDataBase.Warehouses[w] = Warehouse; // обновим ID
                    MDL.SaveXml<MDL.Reference>(MDL.MyDataBase, MDL.SFile_DB);
                }
                // если есть в общей бд то проверим на актуальность 
                WarehouseClass ww = SqlRequest.ReadData<WarehouseClass>(MDL.MyDataBase.Warehouses[w].ID);
                if (ww.Equals(MDL.MyDataBase.Warehouses[w]))
                {
                    // если они одинаковы в общей БД
                    // Warehouse = MDL.MyDataBase.Warehouses[w];
                }
                else
                {
                    // если есть отличте с локальной БД
                    MDL.MyDataBase.Warehouses[w] = ww;
                    MDL.SaveXml<MDL.Reference>(MDL.MyDataBase, MDL.SFile_DB);
                }
            }

            //  Внесение в общую базу Products
            List<Products> products = [];
            foreach (var obj in MainTabel)
            {
                foreach (var tv in obj.Tabels)
                {
                    if (tv is MaterialObj material)
                    {
                        string name_wood;
                        if (material.MaterialType == MaterialObj.MaterialTypeEnum.Количество)
                        {
                            name_wood = material.NameMaterial;

                        }
                        else
                        {
                            // тут н/о
                            if (material.TypeWood == TypeWoodEnum.Любой) name_wood = "Необрезная " + material.LongMaterial.ToString() + "метровый";
                            else name_wood = "Необрезная (" + material.TypeWood.ToString() + ")" + material.LongMaterial.ToString() + "метровый";
                        }

                        var find = products.Find(x => x.NameItem == name_wood);
                        if (find is not null)
                        {
                            find.Cubature += material.CubatureAll;
                        }
                        else
                        {
                            products.Add(new Products()
                            {
                                OnePice = material.Cub,
                                Cubature = material.CubatureAll,
                                NameItem = name_wood,
                                Warehouse = Warehouse,
                                Price = material.Price
                            });
                        }
                    }
                    else if (tv is Tabel_Timbers timbers)
                    {
                        var find = products.Find(x => Products.TypeWoodStr(TypeWoodEnum.Любой) == x.NameItem);
                        if (find is not null)
                        {
                            find.Cubature += timbers.CubatureAll;
                        }
                        else
                        {
                            products.Add(new Products()
                            {
                                OnePice = 0,
                                Cubature = timbers.CubatureAll,
                                TypeWood = TypeWoodEnum.Любой,
                                Price= (timbers.Timbers .Sum(x=>x.Цена) / timbers.Timbers.Sum(x=>x.Количество)),
                                Warehouse = Warehouse 
                            });

                        }
                    }
                }
            }

            try
            {
                // запуск сохранения в БД Products              
                foreach (var item in products)
                {
                    Products? cl = SqlRequest.ReadData<Products>([new ("NameItem",  item.NameItem), new("Warehouse", item.Warehouse.ID)] );
                    // Меняет текущее в на этом складе и по этому названию 
                    if (cl != null) SqlRequest.ConcurrentReqest(cl, new (nameof(item.Cubature), item.Cubature.ToString()), SqlRequest.OperatonEnum.vsPlus);
                    // создает новое значение если не найдено 
                    else SqlRequest.SetData(item);
                }

                // далее сохранения документа в БД document
                id_doc = SqlRequest.SetData(this);
                List<DocRow> rows = [];
                foreach (var obj in MainTabel)
                {
                    foreach (var tv in obj.Tabels)
                    {
                        rows.Add(new DocRow(tv, obj.NameGrup, id_doc));
                    }
                }
                SqlRequest.SetData([.. rows]);
            }
            catch (Exception ex)
            {
                MDL.LogError("Ошибка при попытки провести документ " + DOC_CODE, ex.Message + ex.Source);
                Status = StatusEnum.Ошибка;
                XmlProtocol.SaveDocXml<DocArrival>(this);
                return;
            }

        final:
            Status = StatusEnum.Проведен;
            XmlProtocol.SaveDocXml<DocArrival>(this);
        }



    }
}
