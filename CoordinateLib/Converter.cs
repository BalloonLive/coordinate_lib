using CoordinateLib.Converters;
using CoordinateLib.CoordSystems;
using CoordinateLib.Enums;
using System;

namespace CoordinateLib
{
    public class Converter
    {
        private CoordSystem _selectedCoordSystem = null;
        private IConverter _converter = null;
        private static object _sync = new object();

        public CoordinatesData Convert(double latitude, double longitude, CoordSystem coordSystem)
        {
            lock (_sync)
            {
                return GetConverter(coordSystem).Convert(latitude, longitude, 0);
            }
        }

        public CoordinatesData Convert(double latitude, double longitude, double altitude, CoordSystem coordSystem)
        {
            lock (_sync)
            {
                return GetConverter(coordSystem).Convert(latitude, longitude, altitude);
            }
        }

        public CoordinatesNumericData ConvertNumeric(double latitude, double longitude, CoordSystem coordSystem)
        {
            lock (_sync)
            {
                return GetConverter(coordSystem).ConvertNumeric(latitude, longitude, 0);
            }
        }

        public CoordinatesNumericData ConvertNumeric(double latitude, double longitude, double altitude, CoordSystem coordSystem)
        {
            lock (_sync)
            {
                return GetConverter(coordSystem).ConvertNumeric(latitude, longitude, altitude);
            }
        }

        private IConverter GetConverter(CoordSystem coordSystem)
        {
            if (coordSystem != _selectedCoordSystem || _converter == null)
            {
                _selectedCoordSystem = coordSystem;

                switch (coordSystem.GridSys)
                {
                    case GridSys.LATLONG:
                        if (coordSystem.Format == "DM")
                        {
                            _converter = new DMConverter();
                        }
                        else
                        {
                            _converter = new Passthrough();
                        }
                        break;
                    case GridSys.UTM:
                        _converter = new UTMConverter(coordSystem);
                        break;
                    case GridSys.SWISS:
                        _converter = new SwissConverter();
                        break;
                    case GridSys.BNG:
                        _converter = new BNGConverter(coordSystem);
                        break;
                    case GridSys.USER_TM:
                        _converter = new TMConverter(coordSystem);
                        break;
                    default:
                        throw new IndexOutOfRangeException($"Coordinate system not supported ({coordSystem.Name})");
                }
            }

            return _converter;
        }
    }

    public struct CoordinatesData
    {
        public string Zone { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
    }

    public struct CoordinatesNumericData
    {
        public string Zone { get; set; }
        public int NumericZone { get; set; }
        public string Sign { get; set; }
        public double Easting { get; set; }
        public double Northing { get; set; }
    }
}
