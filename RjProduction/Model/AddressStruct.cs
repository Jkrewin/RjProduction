
namespace RjProduction.Model
{
   public struct AddressStruct
    {
        public string? City { get; set; }
        public string? Subject { get; set; }
        public string? Street { get; set; }
        public string? House { get; set; }
        public int? Flat { get; set; }
        public string? Forestry { get; set; }
        public string? Subforestry { get; set; }
        public uint? Quarter { get; set; }
        public uint? TaxationUnit { get; set; }
        public uint? CuttingArea { get; set; }

        public TypeAdressEnum TypeAdress { get; set; } 

        public enum TypeAdressEnum { ЛеснойКвартал, АдресМесто }
    }
}
