using static RjProduction.Sql.ISqlProfile;
using static RjProduction.Sql.SqlRequest;

namespace RjProduction.Sql
{
    public interface IConcurrentReqest
    {
        public double ReqestTransaction(ISqlProfile sqlProfile, string tabelName, string field, double valueDbl, long id, OperatonEnum operaton)
        {
            if (id == -1) return -1;

            double result = 0;
            sqlProfile.Conection(true);
            try
            {
                var po = sqlProfile.AdapterSql(tabelName, field, $"ID = {id}");
                if (po is not null)
                {
                    if (double.TryParse(po.ToString(), out double d_db))
                    {
                        switch (operaton)
                        {
                            case OperatonEnum.vsPlus:
                                result = (d_db + valueDbl);
                                break;
                            case OperatonEnum.vsMunis:
                                var tt = d_db - valueDbl;
                                if (tt < 0) return -1;
                                result = tt;
                                break;
                            case OperatonEnum.vsMutation:
                                result = valueDbl;
                                break;
                        }
                        sqlProfile.SqlCommand($"UPDATE {sqlProfile.QuotSql(tabelName)} SET {field} = '{result}' WHERE ID = {id} ");
                    }
                }
            }
            catch
            {
                sqlProfile.Transaction(TypeTransaction.roolback);
                result = -1;
                throw;
            }
            finally
            {
                sqlProfile.Transaction(TypeTransaction.commit);
                sqlProfile.Disconnect();
            }
            return result;
        }

        /// <summary>
        /// Запрос на плюс или минус значение, к общим остаткам. Помогоает избежать ошибку конкурентного доступа без монопольго режима 
        /// </summary>
        /// <param name="sqlProfile">Указвннй профиль открывает транзакцию</param>
        /// <param name="operaton">операция</param>
        /// <param name="value">значение числа</param>
        public void ConcurrentReqest(ISqlProfile sqlProfile, OperatonEnum operaton, double value);

    }
}
