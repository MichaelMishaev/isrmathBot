using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Objects
{
    public class RiddleModel
    {
            public int RiddleId { get; set; }
            public string Question { get; set; }
            public string Answer { get; set; }
            public string Difficulty { get; set; }
            public bool IsAssigned { get; set; }
            public bool IsSolved { get; set; }
            public string Hint { get; set; }

    }
}
