using CoordinateLib.Core;

namespace CoordinateLib.Converters
{
    public class SwissConverter : IConverter
    {
        public SwissConverter()
        {
        }

        CoordinatesData IConverter.Convert(double latitude, double longitude, double altitude = 0, bool is_valid = true)
        {
            if (!is_valid)
                return new CoordinatesData { Zone = "", Line1 = "000000", Line2 = "000000" };

            var coords = Convert(latitude, longitude, altitude);

            string text_northing = "";
            text_northing = FormatFunctions.ItoaFormat(coords.Easting, text_northing, 10, 6);
            string text_easting = "";
            text_easting = FormatFunctions.ItoaFormat(coords.Northing, text_easting, 10, 6);

            return new CoordinatesData { Zone = coords.Sign, Line1 = text_easting, Line2 = text_northing };
        }

        public CoordinatesNumericData ConvertNumeric(double latitude, double longitude, double altitude = 0)
        {
            var coords = Convert(latitude, longitude, altitude);
            return new CoordinatesNumericData { Sign = coords.Sign, Easting = coords.Easting, Northing = coords.Northing };
        }

        public SwissCoordinates Convert(double latitude, double longitude, double altitude = 0)
        {

            double lat = ((double)latitude) * 3600;
            double lang = ((double)longitude) * 3600;

            double lat_ = (lat - 169028.66) / 10000.0;
            double long_ = (lang - 26782.5) / 10000.0;

            double lat_2 = lat_ * lat_;
            double lat_3 = lat_2 * lat_;
            double long_2 = long_ * long_;
            double long_3 = long_2 * long_;

            double x, y;

            y = 600072.37 + 211455.93 * long_ - 10938.51 * long_ * lat_ - 0.36 * long_ * lat_2 - 44.54 * long_3;
            x = 200147.07 + 308807.95 * lat_ + 3745.25 * long_2 + 76.63 * lat_2 - 194.56 * long_2 * lat_ + 199.79 * lat_3;

            var easting = (long)x;
            var northing = (long)y;



            return new SwissCoordinates()
            {
                Sign = "SUI",
                Easting = easting,
                Northing = northing
            };
        }

        private char getHemisphereLetter(ref long pos, string signs)
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

        public struct SwissCoordinates
        {
            public string Sign { get; set; }
            public long Easting;
            public long Northing;
        }
    }
}
