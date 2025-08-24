
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
        public string Label { get; set; }

        public AddresStruct(string address) {
            Address = address;
            Email = string.Empty;
            Phone = string.Empty;
            Label = address;
        }

        public AddresStruct(string address, string label)
        {
            Address = address;
            Email = string.Empty;
            Phone = string.Empty;
            Label= label;
        }

        public AddresStruct(string address, string email, string phone, string label)
        {
            Address = address;
            Email = email;
            Phone = phone;
            Label = label;
        }

        public override readonly string ToString()
        {
            if (Label is null) return Address;
            else return Label;
        }
    }
}
