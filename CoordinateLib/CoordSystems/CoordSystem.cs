using CoordinateLib.Enums;
using System;
using static CoordinateLib.Core.Params;

namespace CoordinateLib.CoordSystems
{
    public class CoordSystem : IEquatable<CoordSystem>
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Format { get; set; }
        public GridSys GridSys { get; set; }
        public CoordDattransParams CoordDattransParams { get; set; }
        public CoordTmPartialParams CoordTmPartialParams { get; set; }
        public CoordDatumParams CoordDatumParams { get; set; }
        public string DatumName { get; set; }
        public string DatumParamLine { get; set; }
        public string GridSysParamLine { get; set; }

        #region comparison implementation
        public override bool Equals(object obj)
        {
            return this.Equals(obj as CoordSystem);
        }

        public bool Equals(CoordSystem p)
        {
            // If parameter is null, return false.
            if (Object.ReferenceEquals(p, null))
            {
                return false;
            }

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, p))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != p.GetType())
            {
                return false;
            }

            // Return true if the fields match.
            // Note that the base class is not invoked because it is
            // System.Object, which defines Equals as reference equality.
            return (this.Name == p.Name) && (this.Version == p.Version) && (this.Format == p.Format);
        }

        public string GetGridSysParam()
        {
            if (this.GridSysParamLine != "")
                return this.GridSysParamLine;
            if ( this.CoordTmPartialParams != null)
                return this.CoordTmPartialParams.ToString();
            else
                return "";
        }

        public string GetDatumParam()
        {
            if (this.DatumParamLine != "")
                return this.DatumParamLine;
            if (this.CoordDattransParams != null)
                return this.CoordDattransParams.ToString() + "," + this.DatumName;
            else
                return this.DatumName;
        }

        public string Line1Header
        {
            get
            {
                switch (this.GridSys)
                {
                    case GridSys.LATLONG:
                        return "Latitude";
                    case GridSys.BNG:
                    case GridSys.SWISS:
                    case GridSys.UTM:
                    case GridSys.USER_TM:
                        return "Easting";
                    default:
                        return "Position";
                }
            }
        }

        public string Line2Header
        {
            get
            {
                switch (this.GridSys)
                {
                    case GridSys.LATLONG:
                        return "Longitude";
                    case GridSys.BNG:
                    case GridSys.SWISS:
                    case GridSys.UTM:
                    case GridSys.USER_TM:
                        return "Northing";
                    default:
                        return "";
                }
            }
        }
        public static bool operator ==(CoordSystem lhs, CoordSystem rhs)
        {
            // Check for null on left side.
            if (Object.ReferenceEquals(lhs, null))
            {
                if (Object.ReferenceEquals(rhs, null))
                {
                    // null == null = true.
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(CoordSystem lhs, CoordSystem rhs)
        {
            return !(lhs == rhs);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() * 0x00010000 + this.Version?.GetHashCode() ?? 0 + this.Format?.GetHashCode() ?? 0;
        }

        public static CoordSystem CreateUtmWGS84()
        {
            CoordSystem cs = new CoordSystem();
            cs.Name = "UTM WGS84";
            cs.DatumName = "WGS84";
            cs.GridSys = GridSys.UTM;
            cs.CoordDatumParams = new CoordDatumParams();

            return cs;
        }

        public override string ToString()
        {
            return this.Name;
        }
        #endregion
    }
}
