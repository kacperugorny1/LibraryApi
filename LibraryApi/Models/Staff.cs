namespace LibraryApi.Models
{
    public class Staff
    {
        public int Staff_id { get; set; }
        public int Library_id { get; set; }
        public string? First_Name { get; set; }
        public string? Last_Name { get; set; }
        public string? Email { get; set; }
        public int Auth_id { get; set; }

    }
}
