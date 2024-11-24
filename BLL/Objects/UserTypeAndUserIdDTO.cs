using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Objects
{
    public class UserTypeAndUserIdDTO
    {
        public int UserId { get; set; }           // The user's unique ID
        public string UserType { get; set; }      // 'Student', 'Teacher', or 'Parent'
        public int? RoleSpecificId { get; set; }  // TeacherId, ParentId, or StudentId
    }
}
