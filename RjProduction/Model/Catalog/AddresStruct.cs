
namespace RjProduction.Model.Catalog
{
    /// <summary>
    /// Адрес места
    /// </summary>
    public struct AddresStruct
    {
        public string Phone { get; set; }
        public string Address { get; set; } 
        public string Email { get; set; }

        public AddresStruct(string address) {
            Address = address;
            Email = string.Empty;
            Phone = string.Empty;
        }

        public override string ToString() => Address;
    }
}
