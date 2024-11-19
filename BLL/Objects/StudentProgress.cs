using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Objects
{
    public class StudentProgress
    {
        public int ProgressId { get; set; }
        public int StudentId { get; set; }
        public int ExerciseId { get; set; }
        public string StudentAnswer { get; set; }
        public bool? IsCorrect { get; set; }
        public bool HelpRequested { get; set; }
        public bool HasStarted { get; set; }
        public int IncorrectAttempts { get; set; }
    }
}
