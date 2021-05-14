using CoordinateLib.Core;
using System;

namespace CoordinateLib.Converters
{
    public class DMConverter : IConverter
    {
        public DMConverter()
        {
        }

        CoordinatesData IConverter.Convert(double latitude, double longitude, double altitude = 0, bool is_valid = true)
        {
            if (!is_valid)
                return new CoordinatesData { Zone = "", Line1 = getFormatedCoordinate(0, "EW"), Line2 = getFormatedCoordinate(0, "NS") };

            var coords = Convert(latitude, longitude, altitude);
            return new CoordinatesData { Zone = "", Line1 = $"{coords.EastWestCoordinate}", Line2 = $"{coords.NorthSouthCoordinate}" };
        }

        public DmCoordinates Convert(double latitude, double longitude, double altitude = 0)
        {

            return new DmCoordinates()
            {
                EastWestCoordinate = getFormatedCoordinate(longitude, "EW"),
                NorthSouthCoordinate = getFormatedCoordinate(latitude, "NS"),
            };
        }

        public CoordinatesNumericData ConvertNumeric(double latitude, double longitude, double altitude = 0)
        {
            throw new NotImplementedException();
        }

        private char getHemisphereLetter(ref double pos, string signs)
        {
            if ((long)pos > 99999999) pos = 0;

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

        private string getFormatedCoordinate(double pos, string signs)
        {
            double value, value2;
            string text = "";

            var sign = getHemisphereLetter(ref pos, signs);
            text += sign;

            // Degrees
            value = pos;
            if (value < 100) text += ' ';
            if (value < 10) text += ' ';
            text = FormatFunctions.Itoa((long)Math.Truncate(value), text, 10);
            text += '°';

            // Minutes
            value = value - Math.Truncate(value);
            value *= 60;
            if (value < 10)
                text += '0';
            text = FormatFunctions.Itoa((long)Math.Truncate(value), text, 10);
            text += '.';

            // Decimals
            value = value - Math.Truncate(value);
            value = Math.Round(value * 10000);
            //if (value > 5000) value++;
            if (value >= 10000) value = 9999; // Make sure the number is not too big. Maybe from the rounding...
            return FormatFunctions.ItoaFormat((long)Math.Truncate(value), text, 10, 4);
        }

        public struct DmCoordinates
        {
            // easting
            public string EastWestCoordinate;

            // northing
            public string NorthSouthCoordinate;
        }
    }
}
