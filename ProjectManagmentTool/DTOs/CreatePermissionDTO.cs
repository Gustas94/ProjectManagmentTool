namespace ProjectManagmentTool.DTOs
{
    public class CreatePermissionDTO
    {
        public string Name { get; set; }           // e.g., "ADMIN_PANEL_ACCESS", "EDIT_TASK"
        public string Description { get; set; }    // Optional description of the permission
    }
}
