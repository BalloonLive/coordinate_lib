using CoordinateLib.Enums;
using System;
using System.Globalization;

namespace CoordinateLib.Core
{
    public class Params
    {
        public static readonly int COORD_DATTRANS_NONE = 0;
        public static readonly int COORD_DATTRANS_MA = 1;
        public static readonly int COORD_DATTRANS_HELMERT = 2;
        public class CoordDattransParams
        {
            public CoordDattransParams()
            {

            }

            public CoordDattransParams(double dXp, double dYp, double dZp, double d_ap, double d_fp)
            {
                //-133,-48,148,-23,-0.00000008116374765896,AGD1966
                this.formula = COORD_DATTRANS_MA;
                this.dX = dXp * -1;
                this.dY = dYp * -1;
                this.dZ = dZp * -1;
                this.d_a = d_ap * -1;
                this.d_f = d_fp * -1;
            }

            public CoordDattransParams(double dXp, double dYp, double dZp, double d_ap, double d_fp, double rxp, double ryp, double rzp, double s1p)
            {
                //-102,-102,-129,-251,-0.00001419266264467492,-0.413,0.184,-0.385,2.4664,ED1950
                this.formula = COORD_DATTRANS_HELMERT;
                this.dX = dXp * -1;
                this.dY = dYp * -1;
                this.dZ = dZp * -1;
                this.d_a = d_ap * -1;
                this.d_f = d_fp * -1;
                this.rx = -1.0 * rxp / 3600.0 * Math.PI / 180.0;
                this.ry = -1.0 * ryp / 3600.0 * Math.PI / 180.0;
                this.rz = -1.0 * rzp / 3600.0 * Math.PI / 180.0;
                this.s1 = -1.0 * s1p / 1e6 + 1;

                this.precision = 4 / new CoordDatumParams(this).a;
            }

            public readonly int formula;
            public readonly double dX;
            public readonly double dY;
            public readonly double dZ;
            public readonly double d_a;
            public readonly double d_f;
            public readonly double rx;
            public readonly double ry;
            public readonly double rz;
            public readonly double s1;

            // Temp
            public readonly double precision;

            public override string ToString()
            {
                if (formula == COORD_DATTRANS_MA)
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0:F},{1:F},{2:F},{3:F},{4:F}",
                        -1.0 * dX, -1.0 * dY, -1.0 * dZ, -1.0 * d_a, -1.0 * d_f);
                }
                else if ( formula == COORD_DATTRANS_HELMERT)
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0:F3},{1:F3},{2:F3},{3:F4},{4:F20},{5:F4},{6:F4},{7:F4},{8:F4}",
                        -1.0 * dX, -1.0 * dY, -1.0 * dZ, -1.0 * d_a, -1.0 * d_f,
                        rx * -1.0 * 3600.0 / Math.PI * 180.0, ry * -1.0 * 3600.0 / Math.PI * 180.0, rz * -1.0 * 3600.0 / Math.PI * 180.0, (s1 - 1) * 1e6);
                }
                else
                {
                    return "";
                }
            }
        }

        public class CoordDatumParams
        {
            public CoordDatumParams(CoordDattransParams datatransParams = null)
            {
                this.a = 6378137;
                this.f = 0.003352810703188d;
                this.iswgs84 = 1;

                if (datatransParams != null)
                {
                    this.iswgs84 = 0;
                    this.a = this.a - (-1 * datatransParams.d_a);
                    this.f = this.f - (-1 * datatransParams.d_f);
                }

                b = a * (1 - f);
                e2 = 2.0 * f - f * f;
                e2_i = e2 / (1.0 - e2);
            }
            public readonly int iswgs84;
            public readonly double a;
            public readonly double b;
            public readonly double f;
            public readonly double e2;
            public readonly double e2_i;
        }

        /// <summary>
        /// // Generic datum parameters
        /// </summary>
        public class CoordUtmParams
        {
            public CoordUtmParams(CoordDatumParams d)
            {
                fixedutmzone = 0;
                Mh1 = 1.0 - d.e2 / 4.0 - 3.0 * (d.e2 * d.e2) / 64.0 - 5.0 * (d.e2 * d.e2 * d.e2) / 256.0;
                Mh2 = 3.0 * d.e2 / 8.0 + 3.0 * (d.e2 * d.e2) / 32.0 + 45.0 * (d.e2 * d.e2 * d.e2) / 1024.0;
                Mh3 = 15.0 * d.e2 * d.e2 / 256.0 + 45.0 * d.e2 * d.e2 * d.e2 / 1024.0;
                Mh4 = 35.0 * d.e2 * d.e2 * d.e2 / 3072.0;
            }
            int fixedutmzone;
            public double Mh1;
            public double Mh2;
            public double Mh3;
            public double Mh4;
        }

        // Parameters specific to transverse mercator
        public class CoordTmPartialParams
        {
            public CoordTmPartialParams()
            {

            }
            public CoordTmPartialParams(CoordTmPartialParams coordTmPartialParams)
            {
                if (coordTmPartialParams == null)
                    // use defaults
                    return;

                this.k0 = coordTmPartialParams.k0;
                this.long0 = coordTmPartialParams.long0;
                this.lat0 = coordTmPartialParams.lat0;
                this.fe = coordTmPartialParams.fe;
                this.fn = coordTmPartialParams.fn;
            }
            public CoordTmPartialParams(double k0, double long0, double lat0, double fe, double fn)
            {
                this.k0 = k0;
                this.long0 = long0;
                this.lat0 = lat0;
                this.fe = fe;
                this.fn = fn;
            }

            public readonly double k0 = 0.9996012717;
            public readonly double fe = 400000;
            public readonly double fn = -100000;
            public readonly double long0 = -2;
            public readonly double lat0 = 49;

            public override string ToString()
            {
                return string.Format(CultureInfo.InvariantCulture, "{0:F},{1:F},{2:F},{3:F},{4:F}",
                        k0, long0, lat0, fe, fn);
            }
        }
        public class CoordTmParams : CoordTmPartialParams
        {
            public CoordTmParams()
            {

            }
            public CoordTmParams(CoordTmPartialParams coordTmPartialParams, CoordDatumParams d) : base(coordTmPartialParams)
            {
                this.n = (d.a - d.b) / (d.a + d.b);
                this.n2 = this.n * this.n;
                this.n3 = this.n2 * this.n;

                this.Ma_c = 1 + this.n + (5.0 / 4.0) * this.n2 + (5.0 / 4.0) * this.n3;
                this.Mb_c = 3.0 * this.n + 3.0 * this.n2 + (21.0 / 8.0) * this.n3;
                this.Mc_c = (15.0 / 8.0) * this.n2 + (15.0 / 8.0) * this.n3;
                this.Md_c = (35.0 / 24.0) * this.n3;

                this.long0_rad = this.long0 / 180.0 * Math.PI;
                this.lat0_rad = this.lat0 / 180.0 * Math.PI;
            }

            // Temp calculations
            public readonly double n;
            public readonly double n2;
            public readonly double n3;
            public readonly double Ma_c;
            public readonly double Mb_c;
            public readonly double Mc_c;
            public readonly double Md_c;
            public readonly double long0_rad;
            public readonly double lat0_rad;
        }
    }
}
