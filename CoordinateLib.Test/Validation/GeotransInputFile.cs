using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Tests.Validation
{
    public class GeotransInputFile
    {
        public class Position
        {
            double latitude;
            double longitude;
            double altitude;

            public Position(double latitude, double longitude, double altitude)
            {
                this.altitude = altitude;
                this.longitude = longitude;
                this.latitude = latitude;
            }

            public double Altitude
            {
                get { return this.altitude; }
            }

            public double Longitude
            {
                get { return this.longitude; }
            }

            public double Latitude
            {
                get { return this.latitude; }
            }
        }

        private List<Position> positions;

        public GeotransInputFile(double latMin, double latMax, double longMin, double longMax, double altMin, double altMax, int count)
        {
            this.positions = new List<Position>();
            Random rnd = new Random();

            for (int i = 0; i < count; i++)
            {
                double lat = rnd.NextDouble() * (latMax - latMin) + latMin;
                double lon = rnd.NextDouble() * (longMax - longMin) + longMin;
                double alt = rnd.NextDouble() * (altMax - altMin) + altMin;
                this.positions.Add(new Position(lat, lon, alt));
            }
        }

        public GeotransInputFile(string filename)
        {
            var provider = CultureInfo.InvariantCulture;
            this.positions = new List<Position>();
            using (StreamReader sr = new StreamReader(filename))
            {
                string line = sr.ReadLine();
                if (line != "COORDINATES: Geodetic") throw new Exception("Invalid Geotrans input file!");
                line = sr.ReadLine();
                if (line != "DATUM: WGE") throw new Exception("Invalid Geotrans input file!");
                line = sr.ReadLine();
                if (line != "# ELLIPSOID: WE") throw new Exception("Invalid Geotrans input file!");
                line = sr.ReadLine();
                if (line != "ELLIPSOID HEIGHT") throw new Exception("Invalid Geotrans input file!");
                line = sr.ReadLine();
                if (line != "COORDINATE ORDER: LATITUDE-LONGITUDE") throw new Exception("Invalid Geotrans input file!");
                line = sr.ReadLine();
                if (line != "END OF HEADER") throw new Exception("Invalid Geotrans input file!");
                line = sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    int pos = line.IndexOf(',');
                    int pos2 = line.IndexOf(',', pos + 1);
                    string part1 = line.Substring(0, pos);
                    string part2 = line.Substring(pos + 1, pos2 - pos - 1);
                    string part3 = line.Substring(pos2 + 1);
                    double lat = double.Parse(part1.Substring(0, part1.Length - 1), provider);
                    if (part1.Substring(part1.Length - 1, 1) == "S") lat = lat * -1;
                    double lon = double.Parse(part2.Substring(0, part2.Length - 1), provider);
                    if (part2.Substring(part2.Length - 1, 1) == "W") lon = lon * -1;
                    double alt = double.Parse(part3, provider);
                    this.positions.Add(new Position(lat, lon, alt));
                }
            }
        }

        public void Write(string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine("COORDINATES: Geodetic");
                sw.WriteLine("DATUM: WGE");
                sw.WriteLine("# ELLIPSOID: WE");
                sw.WriteLine("ELLIPSOID HEIGHT");
                sw.WriteLine("COORDINATE ORDER: LATITUDE-LONGITUDE");
                sw.WriteLine("END OF HEADER");
                sw.WriteLine("");
                foreach (Position p in this.positions)
                {
                    double lat2 = Math.Abs(p.Latitude);
                    string lat = lat2.ToString("0.00000");
                    lat += (p.Latitude >= 0 ? "N" : "S");
                    double lon2 = Math.Abs(p.Longitude);
                    string lon = lon2.ToString("0.00000");
                    lon += (p.Longitude >= 0 ? "E" : "W");
                    string alt = p.Altitude.ToString("0");
                    sw.WriteLine("{0}, {1}, {2}", lat, lon, alt);
                }
            }
        }

        public List<Position> Positions
        {
            get { return this.positions; }
        }
    }
}
