using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Functions
{
    public class LeaderBoardFuncs
    {
        private readonly ExerciseRepository _exerciseRepository;

        public LeaderBoardFuncs(ExerciseRepository exerciseRepository)
        {
            _exerciseRepository = exerciseRepository;
        }

        public async Task<string> GetTotalLeaderBoard()
        {
            var leaderboard = await _exerciseRepository.GetWeeklyLeaderboard();
            if (leaderboard == null || !leaderboard.Any())
            {
                return "🌟 הפסגה פנויה, נכבוש אותה ביחד? 🏆💪✨";
            }

            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("🏆 *טבלת המובילים השבועית!* 🎯\n");
            int rank = 1;
            foreach (var student in leaderboard)
            {
                messageBuilder.AppendLine(
                    $"{rank}. {student.StudentName} - {student.TotalScore} נקודות ✨");
                rank++;
            }

            messageBuilder.AppendLine("\n🎯 ממשיכים לכבוש את הפסגה? אל תוותרו! 💪");
            return messageBuilder.ToString();
        }
    }
}
