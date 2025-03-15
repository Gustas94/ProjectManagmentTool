namespace ProjectManagmentTool.Data
{
    public class ProjectGroup
    {
        public int ProjectID { get; set; }
        public Project Project { get; set; }

        public int GroupID { get; set; }
        public Group Group { get; set; }
    }
}
