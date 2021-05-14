using CoordinateLib.CoordSystems;
using CoordinateLib.Core;

namespace CoordinateLib.Converters
{
    public class BNGConverter : IConverter
    {

        public BNGConverter(CoordSystem coordSystem)
        {
            coordDatumParamsWgs84 = new Params.CoordDatumParams();

            coordDattransParams = coordSystem.CoordDattransParams;
            coordDatumParams = coordSystem.CoordDatumParams;
            coordTmParams = new Params.CoordTmParams(coordSystem.CoordTmPartialParams, coordDatumParams);
        }

        private readonly Params.CoordDattransParams coordDattransParams;
        private readonly Params.CoordDatumParams coordDatumParams;
        private readonly Params.CoordDatumParams coordDatumParamsWgs84;
        private readonly Params.CoordTmParams coordTmParams;

        CoordinatesData IConverter.Convert(double latitude, double longitude, double altitude = 0)
        {
            var coords = Convert(latitude, longitude, altitude);
            return new CoordinatesData { Zone = $"{coords.bng_zone}", Line1 = $"{coords.easting}", Line2 = $"{coords.northing}" };
        }

        CoordinatesNumericData IConverter.ConvertNumeric(double latitude, double longitude, double altitude = 0)
        {
            var coords = Convert(latitude, longitude, altitude);
            return new CoordinatesNumericData { Zone = $"{coords.bng_zone}", Easting = coords.easting, Northing = coords.northing };
        }

        public BngCoordinates Convert(double latitude, double longitude, double altitude)
        {
            long easting, northing;
            string grid;


            double deasting, dnorthing;
            long e100k, n100k;
            long l1, l2;


            double dlat2, dlong2;

            dlat2 = latitude;
            dlong2 = longitude;

            if (coordDattransParams.formula == Params.COORD_DATTRANS_MA)
            {
                CoordFunctions.coord_molodensky_abridged(this.coordDattransParams, this.coordDatumParamsWgs84, latitude, longitude, out dlat2, out dlong2);
            }
            else if (coordDattransParams.formula == Params.COORD_DATTRANS_HELMERT)
            {
                CoordFunctions.coord_helmert(this.coordDattransParams, this.coordDatumParams, this.coordDatumParamsWgs84, latitude, longitude, altitude, out dlat2, out dlong2);
            }

            CoordFunctions.coord_transverse_mercator_convert(this.coordDatumParams, this.coordTmParams, dlat2, dlong2, out deasting, out dnorthing);

            e100k = (long)(deasting / 100000);
            n100k = (long)(dnorthing / 100000);


            if (e100k < 0 || e100k > 7 || n100k < 0 || n100k > 12) return new BngCoordinates()
            {
                easting = 0,
                northing = 0,
                bng_zone = ""
            };

            // translate those into numeric equivalents of the grid letters
            l1 = (19 - n100k) - (19 - n100k) % 5 + ((e100k + 10) / 5);
            l2 = (19 - n100k) * 5 % 25 + e100k % 5;
            // compensate for skipped 'I' and build grid letter-pairs
            if (l1 > 7) l1++;
            if (l2 > 7) l2++;
            var char1 = (int)'A' + l1;
            var char2 = (int)'A' + l2;
            grid = $"{(char)char1}{(char)char2}";


            easting = (long)(deasting) - e100k * 100000;
            northing = (long)(dnorthing) - n100k * 100000;

            if (easting > 99999) easting = 0;
            if (northing > 99999) northing = 0;

            return new BngCoordinates()
            {
                easting = easting,
                northing = northing,
                bng_zone = grid
            };
        }
    }

    public struct BngCoordinates
    {
        public long easting;
        public long northing;
        public string bng_zone;
    }
}
