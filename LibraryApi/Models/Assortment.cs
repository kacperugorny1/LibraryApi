namespace LibraryApi.Models
{
    public class Assortment
    {
        public int Assortment_id { get; set; }
        public int Book_id { get; set; }
        public bool Access {  get; set; }
        public int Library_id { get; set; }
    }
}
