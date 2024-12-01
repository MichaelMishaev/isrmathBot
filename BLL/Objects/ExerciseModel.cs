using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Objects
{
    using Newtonsoft.Json;

    public class ExerciseModel
    {
        public int ProgressId { get; set; }
        public int ExerciseId { get; set; }  // Represents the unique ID of the exercise
        public int TeacherId { get; set; }   // Represents the ID of the teacher who created the exercise
        [JsonProperty("exercise")]
        public string Exercise { get; set; } // The exercise text, e.g., "7 * 8"
        [JsonProperty("answer")]
        public string CorrectAnswer { get; set; } // The correct answer to the exercise, e.g., "56"
        [JsonProperty("hint")]
        public string Hint { get; set; }  // The hint provided for the exercise
        public string DifficultyLevel { get; set; } // Optional: The difficulty level, e.g., "Easy", "Medium", "Hard"
        public string Subject { get; set; } // Optional: The subject of the exercise, e.g., "Math"
        public string Topic { get; set; }   // Optional: The topic of the exercise, e.g., "Multiplication"
        public DateTime CreatedAt { get; set; } // The creation date and time of the exercise
        public int CreatedByUserId { get; set; }
        public string CreatedByRole { get; set; } // 'Teacher' or 'Parent'
        public DateTime UpdatedAt { get; set; } // The time when the exercise was last updated
        public int IncorrectAttempts { get; set; }
        public bool IsWaitingForHelp { get; set; }
    }


}

