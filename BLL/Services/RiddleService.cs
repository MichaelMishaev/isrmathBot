using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class RiddleService
    {
        private readonly ExerciseRepository _exerciseRepository;
        public RiddleService( ExerciseRepository exerciseRepository)
        {
            _exerciseRepository = exerciseRepository;
        }
        public async Task<string> HandleRiddleLogic(int studentId, string phoneNumber)
        {
            // Check if the student has an ongoing riddle session
            var inProgressRiddle = await _exerciseRepository.GetInProgressRiddle(studentId);
            if (inProgressRiddle != null)
            {
                return $"🔮 יש לך חידה שכבר התחלת:\n{inProgressRiddle.Exercise}\nמה התשובה שלך?";
            }

            // Fetch a new riddle (predefined or generated dynamically)
            var newRiddle = await _exerciseRepository.GetNextRiddleForStudent(studentId);
            if (newRiddle == null)
            {
                return "לא מצאתי חידות זמינות כרגע. נסה מאוחר יותר! 🕰️";
            }

            // Save the riddle as in-progress for the student
            await _exerciseRepository.AddRiddleToStudentProgress(studentId, newRiddle.ExerciseId);

            // Return the riddle question
            return $"🔮 הנה חידה בשבילך:\n{newRiddle.Exercise}\nנסה לפתור!";
        }

    }
}
