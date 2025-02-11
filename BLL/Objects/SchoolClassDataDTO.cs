using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Objects
{
    public class SchoolClassDataDTO
    {
        public int SchoolId { get; set; }
        public string SchoolName { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int StudentsInClass { get; set; }
        public int TotalStudentsInSchool { get; set; }
    }
}
