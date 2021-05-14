using CoordinateLib.CoordSystems;
using CoordinateLib.Core;

namespace CoordinateLib.Converters
{
    public class TMConverter : IConverter
    {
        public TMConverter(CoordSystem coordSystem)
        {
            coordDattransParams = coordSystem.CoordDattransParams;
            coordDatumParamsWgs84 = new Params.CoordDatumParams();
            coordDatumParams = coordSystem.CoordDatumParams;
            coordTmParams = new Params.CoordTmParams(coordSystem.CoordTmPartialParams, coordDatumParams);
        }

        private readonly Params.CoordDattransParams coordDattransParams = null;
        private readonly Params.CoordDatumParams coordDatumParams;
        private readonly Params.CoordDatumParams coordDatumParamsWgs84;
        private readonly Params.CoordTmParams coordTmParams;

        CoordinatesData IConverter.Convert(double latitude, double longitude, double altitude = 0)
        {
            var coords = Convert(latitude, longitude, altitude);
            return new CoordinatesData { Zone = "", Line1 = $"{coords.EastWestHemisphere} {((long)coords.EastWestCoordinate).ToString("00000000")}", Line2 = $"{coords.NorthSouthHemisphere} {((long)coords.NorthSouthCoordinate).ToString("00000000")}" };
        }

        CoordinatesNumericData IConverter.ConvertNumeric(double latitude, double longitude, double altitude = 0)
        {
            var coords = Convert(latitude, longitude, altitude);
            return new CoordinatesNumericData { Easting = coords.EastWestCoordinate, Northing = coords.NorthSouthCoordinate };
        }

        public TmCoordinates Convert(double latitude, double longitude, double altitude)
        {
            long prec = 10000000;
            latitude = (double)((long)(latitude * prec)) / (double)prec;
            longitude = (double)((long)(longitude * prec)) / (double)prec;

            if (coordDattransParams?.formula == Params.COORD_DATTRANS_MA)
            {
                CoordFunctions.coord_molodensky_abridged(this.coordDattransParams, this.coordDatumParamsWgs84, latitude, longitude, out latitude, out longitude);
            }
            else if (coordDattransParams?.formula == Params.COORD_DATTRANS_HELMERT)
            {
                CoordFunctions.coord_helmert(this.coordDattransParams, this.coordDatumParams, this.coordDatumParamsWgs84, latitude, longitude, altitude, out latitude, out longitude);
            }

            CoordFunctions.coord_transverse_mercator_convert(this.coordDatumParams, this.coordTmParams, latitude, longitude, out double deasting, out double dnorthing);

            double easting = deasting;
            char eastingHemisphere = getHemisphereLetter(ref easting, "EW");
            double northing = dnorthing;
            char northingHemisphere = getHemisphereLetter(ref northing, "NS");


            return new TmCoordinates()
            {
                EastWestHemisphere = eastingHemisphere,
                EastWestCoordinate = easting,
                NorthSouthHemisphere = northingHemisphere,
                NorthSouthCoordinate = northing,
            };
        }

        private char getHemisphereLetter(ref double pos, string signs)
        {
            if (pos > 99999999) pos = 0;

            // Sign
            if (pos > 0)
            {
                return signs[0];
            }
            else
            {
                pos = pos * -1;
                return signs[1];
            }
        }

        public struct TmCoordinates
        {
            // easting
            public char EastWestHemisphere;
            public double EastWestCoordinate;

            // northing
            public char NorthSouthHemisphere;
            public double NorthSouthCoordinate;
        }
    }
}
