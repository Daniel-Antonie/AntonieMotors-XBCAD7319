namespace AntonieMotors_XBCAD7319.Models
{
    public class CustomerModel
    {
        public string CustomerID { get; set; }
        public string BusinessID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerSurname { get; set; }
        public string CustomerMobileNum { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerType { get; set; }
        public string CustomerAddedDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    }
}
