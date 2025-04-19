
namespace RjProduction.Sql
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class SqlIgnore : System.Attribute
    {
        /// Игнорирует этот элемент и не добавляет его в БД 
    }
}
