namespace Library.MVC.Models
{
    public class Loan
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;
        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;
        public string UserId { get; set; } = string.Empty;
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
    }
}