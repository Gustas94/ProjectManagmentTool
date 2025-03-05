using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagmentTool.Data
{
    public class ProjectUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserID { get; set; }

        [Required]
        public int ProjectID { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }

        [ForeignKey("ProjectID")]
        public Project Project { get; set; }
    }
}
