using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Objects
{
    public class TeacherDetails
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalTeacherAttribute { get; set; }
        public int? ClassId { get; set; }
        public string ClassName { get; set; }
        public int? Grade { get; set; }
        public int? SchoolId { get; set; }
        public string SchoolName { get; set; }
        public string Address { get; set; }
    }
}
