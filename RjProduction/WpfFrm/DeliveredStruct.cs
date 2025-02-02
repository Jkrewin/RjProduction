
using System.Diagnostics.CodeAnalysis;

namespace RjProduction.WpfFrm
{
    public readonly struct DeliveredStruct
    {
        public string Name { get; }
        public int ID { get; }
        public string Comment { get; }
        public object? Obj { get; }
        public bool IsEmpty { get => ID == -1 || (string.IsNullOrEmpty(Name) & string.IsNullOrEmpty(Comment) & ID == 0); }
        public Action? GetAct { get; }


        public DeliveredStruct(string name)
        {
            Name = name;
            ID = 0;
            Comment = name;
            Obj = null;
            GetAct = null;
        }

        public DeliveredStruct(string name, int id, string comment)
        {
            Name = name;
            ID = id;
            Comment = comment;
            Obj = null;
            GetAct = null;
        }

        public DeliveredStruct(string name, int id, string comment, object obj)
        {
            Name = name;
            ID = id;
            Comment = comment;
            Obj = obj;
            GetAct = null;
        }

        public DeliveredStruct(string name, int id, string comment, Action act)
        {
            Name = name;
            ID = id;
            Comment = comment;
            Obj = null;
            GetAct = act;
        }

     
        public override string ToString() => Name;
        public static bool operator ==(DeliveredStruct a1, DeliveredStruct a2)
        {
            if (a1.ID != -1 & a2.ID != -1) return a1.ID == a2.ID;
            else if (a1.Obj != null & a2.Obj != null) return a1.Obj == a2.Obj;
            else return a1.Name == a2.Name;
        }
        public static bool operator !=(DeliveredStruct a1, DeliveredStruct a2)
        {
            if (a1.ID != -1 & a2.ID != -1) return a1.ID != a2.ID;
            else if (a1.Obj != null & a2.Obj != null) return a1.Obj != a2.Obj;
            else return a1.Name != a2.Name;
        }
        public override bool Equals([NotNullWhen(true)]  object? obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
    }
}
