using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Functions
{
    public class MathFunctions
    {
        public static string FormatExerciseString(string exercise)
        {
            // Replace '*' with '×' to ensure proper multiplication symbol
            exercise = exercise.Replace("*", "×");

            string exerciseText;

            // Check if the exercise is in a vertical format or already contains a special placeholder
            if (exercise.Contains("?") || exercise.Contains("_") || exercise.Contains("\n"))
            {
                // If it's a columnar format or has a placeholder, use it as is
                exerciseText = exercise;
            }
            else
            {
                // Otherwise, add the suffix "= ?"
                exerciseText = $"{exercise} = ?";
            }

            // Add the LTR character if needed
            exerciseText = "\u202A" + exerciseText;

            return exerciseText;
        }

    }
}
