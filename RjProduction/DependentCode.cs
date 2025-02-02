
namespace RjProduction
{
    /// <summary>
    /// Маркирует часть кода которая зависит от изменений в других частях программы
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Class | AttributeTargets.Method)]
    public class DependentCode : System.Attribute
    {
        /// Маркирует часть кода которая зависит от изменений в других частях программы
    }
}
