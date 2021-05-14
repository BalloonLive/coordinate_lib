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

        public Converter()
        {
        }

        public Converter(CoordSystem coordSystem) 
        {
            GetConverter(coordSystem);
        }

        public CoordinatesData Convert(double latitude, double longitude, CoordSystem coordSystem = null, bool is_valid = true)
        {
            lock (_sync)
            {
                return GetConverter(coordSystem).Convert(latitude, longitude, 0, is_valid);
            }
        }

        public CoordinatesData Convert(double latitude, double longitude, double altitude, CoordSystem coordSystem = null, bool is_valid = true)
        {
            lock (_sync)
            {
                return GetConverter(coordSystem).Convert(latitude, longitude, altitude, is_valid);
            }
        }

        public CoordinatesNumericData ConvertNumeric(double latitude, double longitude, CoordSystem coordSystem = null)
        {
            lock (_sync)
            {
                return GetConverter(coordSystem).ConvertNumeric(latitude, longitude, 0);
            }
        }

        public CoordinatesNumericData ConvertNumeric(double latitude, double longitude, double altitude, CoordSystem coordSystem = null)
        {
            lock (_sync)
            {
                return GetConverter(coordSystem).ConvertNumeric(latitude, longitude, altitude);
            }
        }

        private IConverter GetConverter(CoordSystem coordSystem)
        {
            if (coordSystem != null && (coordSystem != _selectedCoordSystem || _converter == null))
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
            if ( _converter == null )
            {
                throw new ArgumentException("Coordinate system missing");
            }

            return _converter;
        }
    }

    public struct CoordinatesData
    {
        public string Zone { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }

        public string ZoneLine1
        {
            get
            {
                if (this.Zone != "")
                    return this.Zone + " " + this.Line1;
                else
                    return this.Line1;
            }
        }
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
