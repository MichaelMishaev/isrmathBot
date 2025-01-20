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

            // Emojis for ranks
            var rankEmojis = new[] { "🥇", "🥈", "🥉", "🏅", "🏅" };

            int rank = 1;

            // Display only the top 5 students
            foreach (var student in leaderboard.Take(5))
            {
                string rankEmoji = rank <= rankEmojis.Length ? rankEmojis[rank - 1] : "🏅";
                messageBuilder.AppendLine($"*{rank}. {student.StudentName}* {rankEmoji}");
                messageBuilder.AppendLine($"נקודות: {student.TotalScore}");
                messageBuilder.AppendLine("--------------------------");
                rank++;
            }

            messageBuilder.AppendLine("\n🎯 ממשיכים לכבוש את הפסגה? אל תוותרו! 💪");
            return messageBuilder.ToString();
        }



    }
}
