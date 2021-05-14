using CoordinateLib.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using static CoordinateLib.Core.Params;

namespace CoordinateLib.CoordSystems
{
    public class CoordSystemDirectory : List<CoordSystem>
    {
        public string Version { get; set; }
    }
    public class CoordSystemsParser
    {
        public CoordSystemDirectory Parse(string fileName = "CoordSystems.xml")
        {
            FileStream fs = new FileStream(fileName, FileMode.Open);
            return Parse(fs);
        }

        public CoordSystemDirectory Parse(Stream stream)
        {
            var coordSystems = new CoordSystemDirectory();

            XmlSerializer serializer = new XmlSerializer(typeof(XmlModel));
            var xmlModel = (XmlModel)serializer.Deserialize(stream);

            foreach (var coordData in xmlModel.Systems)
            {
                coordSystems.Add(Parse(name: coordData.Name, version: xmlModel.Version, format: coordData.Format,
                    gridSys: coordData.GridSys, gridSysParam: coordData.GridSysParam, datumParam: coordData.DatumParam));
            }

            coordSystems.Version = xmlModel.Version;

            return coordSystems;
        }


        public CoordSystem Parse(string name, string version, string format, string gridSys, string gridSysParam, string datumParam)
        {
            var provider = CultureInfo.InvariantCulture;
            var coordSystem = new CoordSystem();
            coordSystem.Name = name;
            coordSystem.Format = format;
            coordSystem.Version = version;
            coordSystem.GridSys = (GridSys)Enum.Parse(typeof(GridSys), gridSys);
            coordSystem.DatumParamLine = "";
            coordSystem.GridSysParamLine = "";

            #region parse datum params
            if (!string.IsNullOrEmpty(datumParam))
            {
                coordSystem.DatumParamLine = datumParam;
                var split = datumParam.Split(',');
                if (split.Length >= 10)
                {
                    //double dXp, double dYp, double dZp, double d_ap, double d_fp, double rxp, double ryp, double rzp, double s1p
                    coordSystem.CoordDattransParams = new CoordDattransParams
                    (
                        dXp: double.Parse(split[0], provider),
                        dYp: double.Parse(split[1], provider),
                        dZp: double.Parse(split[2], provider),
                        d_ap: double.Parse(split[3], provider),
                        d_fp: double.Parse(split[4], provider),
                        rxp: double.Parse(split[5], provider),
                        ryp: double.Parse(split[6], provider),
                        rzp: double.Parse(split[7], provider),
                        s1p: double.Parse(split[8], provider)
                    );
                    coordSystem.DatumName = split[9];

                }
                else if (split.Length > 0 && split.Length < 9)
                {
                    //double dXp, double dYp, double dZp, double d_ap, double d_fp
                    coordSystem.CoordDattransParams = new CoordDattransParams
                    (
                        dXp: double.Parse(split[0], provider),
                        dYp: double.Parse(split[1], provider),
                        dZp: double.Parse(split[2], provider),
                        d_ap: double.Parse(split[3], provider),
                        d_fp: double.Parse(split[4], provider)
                    );
                    coordSystem.DatumName = split[5];
                }
            }
            #endregion
            #region parse sys params and calculate related
            if (!string.IsNullOrEmpty(gridSysParam))
            {
                coordSystem.GridSysParamLine = gridSysParam;
                var split = gridSysParam.Split(',');
                if (split.Length > 0)
                {
                    coordSystem.CoordTmPartialParams = new CoordTmPartialParams(
                        k0: double.Parse(split[0], provider),
                        long0: double.Parse(split[1], provider),
                        lat0: double.Parse(split[2], provider),
                        fe: double.Parse(split[3], provider),
                        fn: double.Parse(split[4], provider)
                    );
                }
            }
            #endregion


            coordSystem.CoordDatumParams = new CoordDatumParams(coordSystem.CoordDattransParams);
            return coordSystem;
        }

    }
}
