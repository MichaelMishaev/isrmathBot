using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BLL.Functions
{
    public class MathFunctions
    {
        public static string FormatExerciseString(string exercise)
        {
            // Regular expression to match fractions in the format numerator/denominator
            var fractionRegex = new Regex(@"\b(\d+)/(\d+)\b");

            // Replace all fractions with their Unicode equivalents
            exercise = fractionRegex.Replace(exercise, match =>
            {
                if (int.TryParse(match.Groups[1].Value, out int numerator) &&
                    int.TryParse(match.Groups[2].Value, out int denominator))
                {
                    return GetMixedFractionWithUnicode(0, numerator, denominator); // Use 0 for wholeNumber since it’s not a mixed fraction
                }
                return match.Value; // Return original text if parsing fails
            });

            return exercise;
        }

        // The GetMixedFractionWithUnicode method remains as defined previously.


        public static string GetMixedFractionWithUnicode(int wholeNumber, int numerator, int denominator)
         {
                            // Unicode mappings for superscript and subscript digits
                            var superscripts = new Dictionary<char, char>
            {
                { '0', '⁰' }, { '1', '¹' }, { '2', '²' }, { '3', '³' },
                { '4', '⁴' }, { '5', '⁵' }, { '6', '⁶' }, { '7', '⁷' },
                { '8', '⁸' }, { '9', '⁹' }
            };

                    var subscripts = new Dictionary<char, char>
            {
                { '0', '₀' }, { '1', '₁' }, { '2', '₂' }, { '3', '₃' },
                { '4', '₄' }, { '5', '₅' }, { '6', '₆' }, { '7', '₇' },
                { '8', '₈' }, { '9', '₉' }
            };

                    // Convert numerator and denominator to superscripts and subscripts
                    string numeratorUnicode = ConvertToUnicode(numerator.ToString(), superscripts);
                    string denominatorUnicode = ConvertToUnicode(denominator.ToString(), subscripts);

            // Combine into fraction format
            string fraction = $"{numeratorUnicode}/{denominatorUnicode}";

            // Return mixed number with fraction, omit wholeNumber if 0
            return wholeNumber == 0 ? fraction : $"{wholeNumber} {fraction}";
        }

        // Helper method to convert a number string into Unicode characters
        static string ConvertToUnicode(string number, Dictionary<char, char> unicodeMapping)
        {
            var result = "";
            foreach (char c in number)
            {
                result += unicodeMapping.ContainsKey(c) ? unicodeMapping[c] : c;
            }
            return result;
        }

    }
}
