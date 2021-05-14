using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Tests.Validation
{
    abstract class PositionParser
    {
        public abstract GeotransOutputFile.Position Parse(string line);
    }

    class PositionParserBNG : PositionParser
    {
        public override GeotransOutputFile.Position Parse(string line)
        {
            if (line.Substring(0, 1) == "#")
            {
                return new GeotransOutputFile.Position(false, "", 0, 0);
            }
            string zone = line.Substring(0, 2);
            double easting = double.Parse(line.Substring(3, 5));
            double northing = double.Parse(line.Substring(8, 5));
            return new GeotransOutputFile.Position(true, zone, easting, northing);
        }
    }

    class PositionParserTM : PositionParser
    {
        public override GeotransOutputFile.Position Parse(string line)
        {
            var provider = CultureInfo.InvariantCulture;

            if (line.Substring(0, 1) == "#")
            {
                return new GeotransOutputFile.Position(false, "", 0, 0);
            }
            string zone = "";
            int pos = line.IndexOf(',');
            if (pos < 0) throw new Exception("Parse error, missing ','");
            double easting = double.Parse(line.Substring(0, pos), provider);
            double northing = double.Parse(line.Substring(pos + 1), provider);
            return new GeotransOutputFile.Position(true, zone, easting, northing);
        }
    }

    class PositionParserUTM : PositionParser
    {
        public override GeotransOutputFile.Position Parse(string line)
        {
            var provider = CultureInfo.InvariantCulture;
            if (line.Substring(0, 1) == "#")
            {
                if (line.Length > 10 && line.Substring(2, 7) == "Warning")
                {
                    return null;
                }
                else
                {
                    return new GeotransOutputFile.Position(false, "", 0, 0);
                }
            }
            int pos = line.IndexOf(',');
            string zone = line.Substring(0, pos);
            string north = line.Substring(pos + 2, 1);
            int pos2 = line.IndexOf(',', pos + 4);
            double easting = double.Parse(line.Substring(pos + 5, pos2 - pos - 5), provider);
            double northing = double.Parse(line.Substring(pos2 + 1, line.Length - pos2 - 1), provider);
            return new GeotransOutputFile.Position(true, zone, easting, northing);
        }
    }
    public class GeotransOutputFile
    {
        public class Position
        {
            double easting;
            double northing;
            string zone;
            bool valid;

            public Position(bool valid, string zone, double easting, double northing)
            {
                this.valid = valid;
                this.zone = zone;
                this.easting = easting;
                this.northing = northing;
            }

            public bool Valid
            {
                get { return this.valid; }
            }

            public string Zone
            {
                get { return this.zone; }
            }

            public double Easting
            {
                get { return this.easting; }
            }

            public double Northing
            {
                get { return this.northing; }
            }
        }

        List<Position> positions;

        public GeotransOutputFile(string filename)
        {
            this.positions = new List<Position>();
            using (StreamReader sr = new StreamReader(filename))
            {
                string line1 = sr.ReadLine();
                PositionParser parser = null;
                switch (line1)
                {
                    case "COORDINATES: British National Grid (BNG)":
                        parser = new PositionParserBNG();
                        break;
                    case "COORDINATES: Universal Transverse Mercator (UTM)":
                        parser = new PositionParserUTM();
                        break;
                    case "PROJECTION: Transverse Mercator":
                        parser = new PositionParserTM();
                        break;
                }
                if (parser == null)
                {
                    throw new Exception("No parser for this format! " + line1);
                }

                bool start = false;
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (line == "") start = true;
                    else if (start)
                    {
                        Position result = parser.Parse(line);
                        if (result != null)
                        {
                            this.positions.Add(result);
                        }
                    }
                }
            }
        }
        public List<Position> Positions
        {
            get { return this.positions; }
        }
    }
}
