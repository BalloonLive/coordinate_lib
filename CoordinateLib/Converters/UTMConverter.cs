using CoordinateLib.CoordSystems;
using CoordinateLib.Core;
using System;

namespace CoordinateLib.Converters
{
    public class UTMConverter : IConverter
    {
        public UTMConverter(CoordSystem coordSystem)
        {
            coordDatumParamsWgs84 = new Params.CoordDatumParams();
            coordDattransParams = coordSystem.CoordDattransParams;
            coordDatumParams = coordSystem.CoordDatumParams;
            coordUtmParams = new Params.CoordUtmParams(coordDatumParams);
        }

        static char[] nzones = new char[] { 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X' };
        private readonly Params.CoordDattransParams coordDattransParams = null;
        private readonly Params.CoordDatumParams coordDatumParams;
        private readonly Params.CoordUtmParams coordUtmParams;
        private readonly Params.CoordDatumParams coordDatumParamsWgs84;

        CoordinatesData IConverter.Convert(double latitude, double longitude, double altitude = 0, bool is_valid = true)
        {
            if (!is_valid)
                return new CoordinatesData { Zone = "", Line1 = "N00000000", Line2 = "E00000000" };

            var coords = Convert(latitude, longitude, altitude);
            return new CoordinatesData { Zone = $"{coords.utm_zone.zone}{coords.utm_zone.nzone}", Line1 = coords.easting, Line2 = coords.northing };
        }

        CoordinatesNumericData IConverter.ConvertNumeric(double latitude, double longitude, double altitude = 0)
        {
            var coords = coord_utm_convert(latitude, longitude, altitude);
            return new CoordinatesNumericData
            {
                NumericZone = coords.utm_zone.zone,
                Sign = coords.utm_zone.nzone.ToString(),
                Zone = $"{coords.utm_zone.zone}{coords.utm_zone.nzone}",
                Easting = coords.easting,
                Northing = coords.northing
            };
        }

        public UtmCoordinatesFormated Convert(double latitude, double longitude, double altitude = 0)
        {
            var utm_coordinates = coord_utm_convert(latitude, longitude, altitude);
            return new UtmCoordinatesFormated
            {
                utm_zone = utm_coordinates.utm_zone,
                easting = ((int)utm_coordinates.easting).ToString(),
                northing = ((int)utm_coordinates.northing).ToString(),
            };

        }

        private UtmCoordinates coord_utm_convert(double latitude, double longitude, double altitude)
        {
            Params.CoordDattransParams coord_dattrans_user = coordDattransParams;
            Params.CoordDatumParams d_params = coordDatumParams;
            Params.CoordUtmParams utm_params = coordUtmParams;

            if (coord_dattrans_user?.formula == Params.COORD_DATTRANS_MA)
            {
                CoordFunctions.coord_molodensky_abridged(coord_dattrans_user, coordDatumParamsWgs84, latitude, longitude, out latitude, out longitude);
            }
            else if (coord_dattrans_user?.formula == Params.COORD_DATTRANS_HELMERT)
            {
                CoordFunctions.coord_helmert(coord_dattrans_user, d_params, coordDatumParamsWgs84, latitude, longitude, altitude, out latitude, out longitude);
            }


            const double k0 = 0.9996;
            const double fe = 500000;

            double fn, lamda0, long_rad, lat_rad, lamda0_rad, lat_cos, lat_tan, lat_sin;
            double A, T, C, n, M;
            double A2, A3, A4, A5, A6;

            fn = 0;
            if (latitude < 0)
            {
                fn = 10000000;
            }
            var utmZone = coord_utm_getzone(latitude, longitude);
            lamda0 = ((double)utmZone.zone - 30) * 6 - 3;

            long_rad = longitude / 180.0 * Math.PI;
            lat_rad = latitude / 180.0 * Math.PI;
            lamda0_rad = lamda0 / 180.0 * Math.PI;
            lat_cos = Math.Cos(lat_rad);

            lat_tan = Math.Tan(lat_rad);
            lat_sin = Math.Sin(lat_rad);

            A = (long_rad - lamda0_rad) * lat_cos;
            T = lat_tan * lat_tan;
            C = d_params.e2 / (1 - d_params.e2) * lat_cos * lat_cos;
            n = d_params.a / Math.Sqrt(1 - d_params.e2 * lat_sin * lat_sin);
            M = d_params.a * (utm_params.Mh1 * lat_rad - utm_params.Mh2 * Math.Sin(2 * lat_rad) + utm_params.Mh3 * Math.Sin(4 * lat_rad) - utm_params.Mh4 * Math.Sin(6 * lat_rad));

            A2 = A * A;
            A3 = A2 * A;
            A4 = A2 * A2;
            A5 = A2 * A3;
            A6 = A3 * A3;

            return new UtmCoordinates
            {
                utm_zone = utmZone,
                easting = fe + k0 * n * (A + (1 - T + C) * A3 / 6 + (5 - 18 * T + T * T + 72 * C - 58 * d_params.e2_i) * A5 / 120),
                northing = fn + k0 * (M + n * lat_tan * (A2 / 2 + (5 - T + 9 * C + 4 * C * C) * A4 / 24 + (61 - 58 * T + T * T + 600 * C - 330 * d_params.e2_i) * A6 / 720))
            };
        }

        private UtmZone coord_utm_getzone(double latitude, double longitude)
        {
            var utmZone = new UtmZone();
            int index;
            Params.CoordUtmParams utm = coordUtmParams;

            if (longitude < 0)
            {
                index = (int)Math.Floor(-1 * longitude / 6);
                utmZone.zone = 30 - index;
            }
            else
            {
                index = (int)Math.Floor(longitude / 6);
                utmZone.zone = index + 31;
            }

            if (latitude < 0)
            {
                index = (int)Math.Floor(-1 * latitude / 8);
                utmZone.nzone = nzones[9 - index];
            }
            else
            {
                index = (int)Math.Floor(latitude / 8);
                utmZone.nzone = nzones[index + 10];
            }

            //Special case zone 32V and 31V
            if (latitude >= 56 && latitude < 64)
            {
                if (longitude > 3 && longitude < 12)
                {
                    utmZone.zone = 32;
                }
                /*		else if ( longitude > -1 && longitude < 3 )
                        {
                            utmZone.zone = 31;
                        }*/
            }

            return utmZone;
        }
    }


    public struct UtmZone
    {
        public int zone;
        public char nzone;
    }

    public struct UtmCoordinates
    {
        public double easting;
        public double northing;
        public UtmZone utm_zone;
    }

    public struct UtmCoordinatesFormated
    {
        public string easting;
        public string northing;
        public UtmZone utm_zone;
    }
}
