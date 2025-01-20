using BLL.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio.Types;

namespace BLL.Services
{
    public class HelperService
    {
        private readonly LeaderBoardFuncs _leaderBoardFuncs;

        public HelperService(LeaderBoardFuncs leaderBoardFuncs)
        {
            _leaderBoardFuncs = leaderBoardFuncs;
        }
        public async Task<string> LeaderBoardTable()
        {
            string leaderString = await _leaderBoardFuncs.GetTotalLeaderBoard();
            string motivationalMessage = "🚀 *האם אתם מוכנים לכבוש את הפסגה?* 🏆🎯\n\n";
            leaderString = motivationalMessage + leaderString;
            return leaderString;
        }

    }
}
