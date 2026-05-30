using UnityEngine;
using System.Globalization;

namespace Utils
{    
    public static class LongToStringConverter
    {
        private static readonly string[] Suffixes =
        {
            "",
            "K",
            "M",
            "B",
            "T",
            "Qa",
            "Qi",
            "Sx",
            "Sp",
            "Oc",
            "No",
            "Dc"
        };

        public static string ConvertFromLongToString(this long value)
        {
            if (value == 0)
            {
                return "0";
            }

            bool isNegative = value < 0;
            ulong absoluteValue = value < 0
                ? (ulong)(-(value + 1)) + 1
                : (ulong)value;

            decimal shortenedValue = absoluteValue;
            int suffixIndex = 0;

            while (shortenedValue >= 1000m && suffixIndex < Suffixes.Length - 1)
            {
                shortenedValue /= 1000m;
                suffixIndex++;
            }

            string format = shortenedValue >= 100m ? "0" : shortenedValue >= 10m ? "0.#" : "0.##";
            string result = shortenedValue.ToString(format, CultureInfo.InvariantCulture) + Suffixes[suffixIndex];

            return isNegative ? "-" + result : result;
        }
    }
}
