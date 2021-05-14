using System.Collections.Generic;
using System.Xml.Serialization;

namespace CoordinateLib.CoordSystems
{
    [XmlRootAttribute("CoordSystems", IsNullable = false)]
    public class XmlModel
    {
        public string Version { get; set; }
        [XmlArrayItem("CoordSystem")]
        public List<CoordSystemXml> Systems { get; set; }
    }



    public class CoordSystemXml
    {
        public string Name { get; set; }
        public string GridSys { get; set; }
        public string DatumParam { get; set; }
        public string GridSysParam { get; set; }
        public string Format { get; set; }
    }
}
