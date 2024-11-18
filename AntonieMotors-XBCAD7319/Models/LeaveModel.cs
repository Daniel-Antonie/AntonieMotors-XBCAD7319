namespace AntonieMotors_XBCAD7319.Models
{
    public class LeaveModel
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public List<LeaveDetail> Leaves { get; set; } = new List<LeaveDetail>();
    }

    public class LeaveDetail
    {
        public string EmployeeName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

}
