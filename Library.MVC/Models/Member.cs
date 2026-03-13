namespace Library.MVC.Models
{
    public class Member
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}