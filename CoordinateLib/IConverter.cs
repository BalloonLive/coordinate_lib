
namespace CoordinateLib
{
    public interface IConverter
    {
        CoordinatesData Convert(double latitude, double longitude, double altitude = 0, bool is_valid = true);
        CoordinatesNumericData ConvertNumeric(double latitude, double longitude, double altitude = 0);
    }
}
