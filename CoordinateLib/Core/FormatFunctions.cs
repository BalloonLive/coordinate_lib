using System;

namespace CoordinateLib.Core
{
    public class FormatFunctions
    {
        public static string Itoa(long value, string str, int @base)
        {
            return ItoaFormat(value, str, @base, 0);
        }

        public static string ItoaFormat(long value, string str, int @base, int minlen)
        {
            string num = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            string wstr = str;

            long sign, i;
            i = 0;


            // Validate base

            if (@base < 2 || @base > 35) { wstr += '\0'; return "0"; }



            // Take care of sign

            if ((sign = value) < 0) value = -value;



            // Conversion. Number is reversed.

            do
            {
                wstr += num[(int)(value % @base)];
                value = value / @base;
                i++;
            } while (value > 0);

            if (sign < 0) i++;
            while (i < minlen)
            {
                i++;
                wstr += '0';
            }
            if (sign < 0) wstr += '-';



            // Reverse string

            char[] charArray = wstr.ToCharArray();
            Array.Reverse(charArray, str.Length, charArray.Length - str.Length);
            return new string(charArray);

        }
    }
}
