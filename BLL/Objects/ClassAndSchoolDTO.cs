using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Objects
{
    public class ClassAndSchoolDTO
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int? Grade { get; set; } // Nullable in case grade is not provided

        public string SchoolName { get; set; }
        public string Address { get; set; }
        public int? StudCount { get; set; }
    }
}
