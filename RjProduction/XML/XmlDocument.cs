using System;
using System.Collections.Generic;
using System.IO;

using System.Xml.Serialization;
using static RjProduction.Model.Document;

namespace RjProduction.XML
{
    public sealed class XmlDocument
    {
        public string DataCreate { get; set; } = "";
        public uint Number { get; set; }
        public string DocTitle { get; set; } = "";
        public List<GrupClass> Grups { get; set; } = [];
        public bool RoundingAmountsEmpl { get; set; }

        public string FileName { get => $"{DataCreate}_{Number}.xml"; }
        public List<GrupObj> MainTabel
        {
            set
            {                
                foreach (var item in value)
                {
                    GrupClass grup = new() { NameGrup = item.NameGrup };
                    foreach (var tv in item.Tabels)
                    {
                        if (tv is Tabel_Timbers _Timbers) grup.Timbers.Add(_Timbers);
                        else if (tv is Employee e) grup.Employees.Add(e);
                        else if (tv is MaterialObj m) grup.Materials.Add(m);
                        else if (tv is Surcharges s) grup.Surcharges.Add(s);
                        else if (tv is Track t) grup.Tracks.Add(t);
                    }
                    Grups.Add (grup);
                }
            }
        }

        public class GrupClass
        {
           public string? NameGrup { get; set; } = "";

            public List<Tabel_Timbers> Timbers = [];
            public List<Employee> Employees = [];
            public List<MaterialObj> Materials = [];
            public List<Surcharges> Surcharges = [];
            public List<Track> Tracks = [];

        }


        static public void SaveXml(string sFile, XmlDocument doc)
        {
            XmlSerializer xmlSerializer = new(typeof(XmlDocument));
            using FileStream fs = new(sFile, FileMode.OpenOrCreate); xmlSerializer.Serialize(fs, doc);
        }

        static public Model.Document LoadXML(string sFile)
        {
            XmlSerializer xmlSerializer = new(typeof(XmlDocument));
            XmlDocument x;
            using (FileStream fs = new(sFile, FileMode.OpenOrCreate))
            {
                x = xmlSerializer.Deserialize(fs) as XmlDocument ?? new XmlDocument();
            }
            Model.Document document = new()
            {
                DataCreate = DateOnly.Parse(x.DataCreate),
                DocTitle = x.DocTitle,
                Number = x.Number,
                RoundingAmountsEmpl = x.RoundingAmountsEmpl
            };

            foreach (var item in x.Grups)
            {
                Model.Document.GrupObj grup = new() { NameGrup = item.NameGrup ?? "" };
                foreach (var tv in item.Employees) grup.Tabels.Add(tv);
                foreach (var tv in item.Materials) grup.Tabels.Add(tv);
                foreach (var tv in item.Timbers) grup.Tabels.Add(tv);
                foreach (var tv in item.Surcharges) grup.Tabels.Add(tv);
                foreach (var tv in item.Tracks) grup.Tabels.Add(tv);
                document.MainTabel.Add(grup);
            }

            return document;
        }
    }
}
