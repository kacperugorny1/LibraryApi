namespace LibraryApi.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public int Publication_year { get; set; }
        public int Language { get; set; }
        public string? Category { get; set; }
    }
}
