using System;

namespace CoordinateLib.Core
{
    public class CoordFunctions
    {
        // Convert between map datums using molodensky abridged
        public static void coord_molodensky_abridged(Params.CoordDattransParams coordDattransParams, Params.CoordDatumParams coordDatumParamsWgs84,
            double latitude, double longitude, out double lat_user, out double long_user)
        {
            var d_user = coordDattransParams;
            double sin_1sec = 0.000004848136811076368;
            double long_rad, lat_rad;
            double nu, rho;
            double lat_sin, lat_sin2, lat_cos, long_sin, long_cos;
            double d_lat_arcsec, d_long_arcsec;

            var d_wgs84 = coordDatumParamsWgs84;

            long_rad = longitude / 180.0 * Math.PI;
            lat_rad = latitude / 180.0 * Math.PI;

            lat_sin = Math.Sin(lat_rad);

            lat_cos = Math.Cos(lat_rad);
            long_cos = Math.Cos(long_rad);

            long_sin = Math.Sin(long_rad);
            lat_sin2 = lat_sin * lat_sin;
            nu = d_wgs84.a / Math.Pow((1 - d_wgs84.e2 * lat_sin2), 0.5);
            rho = d_wgs84.a * (1 - d_wgs84.e2) / Math.Pow((1 - d_wgs84.e2 * lat_sin2), 1.5);

            d_lat_arcsec = (-1.0 * d_user.dX * lat_sin * long_cos - d_user.dY * lat_sin * long_sin + d_user.dZ * lat_cos + (d_wgs84.a * d_user.d_f + d_wgs84.f * d_user.d_a) * Math.Sin(2 * lat_rad)) / (rho * sin_1sec);
            d_long_arcsec = (-1.0 * d_user.dX * long_sin + d_user.dY * long_cos) / (nu * lat_cos * sin_1sec);

            lat_user = latitude + d_lat_arcsec / 3600.0;
            long_user = longitude + d_long_arcsec / 3600.0;
        }

        public static void coord_helmert(Params.CoordDattransParams d_user, Params.CoordDatumParams user, Params.CoordDatumParams wgs84,
            double latitude, double longitude, double height, out double lat_user, out double long_user)
        {
            double long_rad, lat_rad, lat_sin, lat_cos;
            double long_sin, long_cos, nu, x1, y1, z1;
            double x2, y2, z2, p, u, sin_u, cos_u;
            double long2_rad, lat2_rad;

            // Convert to cartesian coordiantes
            long_rad = longitude / 180.0 * Math.PI;
            lat_rad = latitude / 180.0 * Math.PI;

            lat_sin = Math.Sin(lat_rad);
            lat_cos = Math.Cos(lat_rad);


            long_sin = Math.Sin(long_rad);
            long_cos = Math.Cos(long_rad);

            nu = wgs84.a / Math.Sqrt(1 - wgs84.e2 * lat_sin * lat_sin);

            x1 = (nu + height) * lat_cos * long_cos;
            y1 = (nu + height) * lat_cos * long_sin;
            z1 = ((1 - wgs84.e2) * nu + height) * lat_sin;

            // Apply Helmert
            x2 = d_user.dX + x1 * d_user.s1 - y1 * d_user.rz + z1 * d_user.ry;
            y2 = d_user.dY + x1 * d_user.rz + y1 * d_user.s1 - z1 * d_user.rx;
            z2 = d_user.dZ - x1 * d_user.ry + y1 * d_user.rx + z1 * d_user.s1;

            // Convert from cartesian coordinates
            p = Math.Sqrt(x2 * x2 + y2 * y2);
            u = Math.Atan(z2 / p * user.a / user.b);

            sin_u = Math.Sin(u);
            cos_u = Math.Cos(u);

            long2_rad = Math.Atan2(y2, x2);
            lat2_rad = Math.Atan2(z2 + user.e2_i * user.b * sin_u * sin_u * sin_u, p - user.e2 * user.a * cos_u * cos_u * cos_u);

            /*	double precision = 2 / user.a;  // results accurate to around 2 metres
                double p = sqrt(x2*x2 + y2*y2);
                double phi = atan2(z2, p * (1 - user.e2));
                double phiP = 2.0 * M_PI;
                double long2_rad, lat2_rad;
                while ( fabs(phi-phiP) > precision ) {	
                    double sin_phi = sin(phi);
                    double nu = user.a / sqrt(1 - user.e2*sin_phi*sin_phi);
                    phiP = phi;
                    phi = atan2(z2 + user.e2*nu*sin_phi, p);
                }
                long2_rad = atan2(y2, x2);
                lat2_rad = phi;*/

            lat_user = lat2_rad * 180.0 / Math.PI;
            long_user = long2_rad * 180.0 / Math.PI;
        }

        public static void coord_transverse_mercator_convert(Params.CoordDatumParams d_params, Params.CoordTmParams tm_params,
            double latitude, double longitude, out double easting, out double northing)
        {
            double long_rad, lat_rad, lat_cos, lat_sin, lat_tan;
            double lat_sin2, nu, rho, eta2, lat_diff, lat_sum;
            double Ma, Mb, Mc, Md, M;
            double lat_cos3, lat_cos5, lat_tan2, lat_tan4;
            double I, II, III, IIIA, IV, V, VI;
            double long_diff, long_diff2, long_diff3, long_diff4, long_diff5, long_diff6;

            long_rad = longitude / 180.0 * Math.PI;
            lat_rad = latitude / 180.0 * Math.PI;

            lat_cos = Math.Cos(lat_rad);
            lat_sin = Math.Sin(lat_rad);
            lat_tan = Math.Tan(lat_rad);


            lat_sin2 = lat_sin * lat_sin;
            nu = d_params.a * tm_params.k0 / Math.Sqrt(1 - d_params.e2 * lat_sin2); // transverse radius of curvature
            rho = d_params.a * tm_params.k0 * (1 - d_params.e2) / (1 - d_params.e2 * lat_sin2); // meridional radius of curvature
            eta2 = nu / rho - 1;


            lat_diff = lat_rad - tm_params.lat0_rad;
            lat_sum = lat_rad + tm_params.lat0_rad;
            Ma = tm_params.Ma_c * (lat_diff);
            Mb = tm_params.Mb_c * Math.Sin(lat_diff) * Math.Cos(lat_sum);


            Mc = tm_params.Mc_c * Math.Sin(2 * lat_diff) * Math.Cos(2 * lat_sum);
            Md = tm_params.Md_c * Math.Sin(3 * lat_diff) * Math.Cos(3 * lat_sum);
            M = d_params.b * tm_params.k0 * (Ma - Mb + Mc - Md); // meridional arc


            lat_cos3 = lat_cos * lat_cos * lat_cos;
            lat_cos5 = lat_cos3 * lat_cos * lat_cos;
            lat_tan2 = lat_tan * lat_tan;
            lat_tan4 = lat_tan2 * lat_tan2;

            I = M + tm_params.fn;
            II = (nu / 2) * lat_sin * lat_cos;
            III = (nu / 24) * lat_sin * lat_cos3 * (5 - lat_tan2 + 9 * eta2);
            IIIA = (nu / 720) * lat_sin * lat_cos5 * (61 - 58 * lat_tan2 + lat_tan4);
            IV = nu * lat_cos;
            V = (nu / 6) * lat_cos3 * (nu / rho - lat_tan2);
            VI = (nu / 120) * lat_cos5 * (5 - 18 * lat_tan2 + lat_tan4 + 14 * eta2 - 58 * lat_tan2 * eta2);


            long_diff = long_rad - tm_params.long0_rad;
            long_diff2 = long_diff * long_diff;
            long_diff3 = long_diff2 * long_diff;
            long_diff4 = long_diff3 * long_diff;
            long_diff5 = long_diff4 * long_diff;
            long_diff6 = long_diff5 * long_diff;

            northing = I + II * long_diff2 + III * long_diff4 + IIIA * long_diff6;
            easting = tm_params.fe + IV * long_diff + V * long_diff3 + VI * long_diff5;
        }
    }
}
