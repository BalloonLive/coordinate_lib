using System;
using System.Globalization;

namespace CoordinateLib.Converters
{
    public class Passthrough : IConverter
    {
        public CoordinatesData Convert(double latitude, double longitude, double altitude = 0)
        {
            return new CoordinatesData()
            {
                Line1 = Math.Round(latitude, 5).ToString(CultureInfo.InvariantCulture),
                Line2 = Math.Round(longitude, 5).ToString(CultureInfo.InvariantCulture)
            };
        }

        public CoordinatesNumericData ConvertNumeric(double latitude, double longitude, double altitude = 0)
        {
            return new CoordinatesNumericData { Easting = latitude, Northing = longitude };
        }
    }
}
