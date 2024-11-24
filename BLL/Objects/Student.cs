using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Objects
{
    public class Student
    {
        public int StudentId { get; set; }
        public int UserId { get; set; }
        public int ClassId { get; set; }
        public int WrongAnswerCount { get; set; }
    }
}
