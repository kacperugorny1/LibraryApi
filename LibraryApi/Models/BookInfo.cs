namespace LibraryApi.Models
{
    public class BookInfo
    {
        public int Book_id { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public int Publication_year { get; set; }
        public string? Language { get; set; }
        public string? Url {  get; set; }
        public string? Category_name { get; set; }
    }
}
