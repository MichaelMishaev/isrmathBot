using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Objects
{
    public class studentTotalScore
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int TotalCorrectAnswers { get; set; }
        public int TotalScore { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
