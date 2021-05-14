
namespace CoordinateLib
{
    public interface IConverter
    {
        CoordinatesData Convert(double latitude, double longitude, double altitude = 0);
        CoordinatesNumericData ConvertNumeric(double latitude, double longitude, double altitude = 0);
    }
}
