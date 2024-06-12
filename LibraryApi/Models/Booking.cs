namespace LibraryApi.Models
{
    public class Booking
    {
        public int Booking_id { get; set; }
        public int Assortment_id { get; set; }
        public int Customer_id { get; set; }
        public DateTime Booking_date { get; set; }
        public int Booking_length { get; set; }
    }
}
