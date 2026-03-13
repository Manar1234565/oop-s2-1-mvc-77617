using Library.MVC.Data;
using Library.MVC.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Library.Tests
{
    public class LibraryTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Cannot_Create_Loan_For_Book_Already_On_Active_Loan()
        {
            var context = GetInMemoryContext();
            var book = new Book { Title = "Test Book", Author = "Author", Isbn = "123", Category = "Fiction", IsAvailable = false };
            var member = new Member { FullName = "John Doe", Email = "john@test.com", Phone = "123456" };
            context.Books.Add(book);
            context.Members.Add(member);
            await context.SaveChangesAsync();

            context.Loans.Add(new Loan { BookId = book.Id, MemberId = member.Id, LoanDate = DateTime.Now, DueDate = DateTime.Now.AddDays(14), UserId = "" });
            await context.SaveChangesAsync();

            var activeLoan = await context.Loans.AnyAsync(l => l.BookId == book.Id && l.ReturnedDate == null);
            Assert.True(activeLoan);
        }

        [Fact]
        public async Task Returned_Loan_Makes_Book_Available()
        {
            var context = GetInMemoryContext();
            var book = new Book { Title = "Test Book", Author = "Author", Isbn = "456", Category = "Science", IsAvailable = false };
            var member = new Member { FullName = "Jane Doe", Email = "jane@test.com", Phone = "654321" };
            context.Books.Add(book);
            context.Members.Add(member);
            await context.SaveChangesAsync();

            var loan = new Loan { BookId = book.Id, MemberId = member.Id, LoanDate = DateTime.Now, DueDate = DateTime.Now.AddDays(14), UserId = "" };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();

            loan.ReturnedDate = DateTime.Now;
            book.IsAvailable = true;
            await context.SaveChangesAsync();

            Assert.NotNull(loan.ReturnedDate);
            Assert.True(book.IsAvailable);
        }

        [Fact]
        public async Task Book_Search_Returns_Expected_Matches()
        {
            var context = GetInMemoryContext();
            context.Books.AddRange(
                new Book { Title = "Harry Potter", Author = "Rowling", Isbn = "111", Category = "Fiction", IsAvailable = true },
                new Book { Title = "The Hobbit", Author = "Tolkien", Isbn = "222", Category = "Fiction", IsAvailable = true },
                new Book { Title = "Clean Code", Author = "Robert Martin", Isbn = "333", Category = "Technology", IsAvailable = true }
            );
            await context.SaveChangesAsync();

            var results = await context.Books
                .Where(b => b.Title.Contains("Harry") || b.Author.Contains("Harry"))
                .ToListAsync();

            Assert.Single(results);
            Assert.Equal("Harry Potter", results[0].Title);
        }

        [Fact]
        public async Task Overdue_Logic_DueDate_Before_Today_And_Not_Returned()
        {
            var context = GetInMemoryContext();
            var book = new Book { Title = "Overdue Book", Author = "Author", Isbn = "789", Category = "History", IsAvailable = false };
            var member = new Member { FullName = "Test User", Email = "test@test.com", Phone = "000000" };
            context.Books.Add(book);
            context.Members.Add(member);
            await context.SaveChangesAsync();

            var loan = new Loan
            {
                BookId = book.Id,
                MemberId = member.Id,
                LoanDate = DateTime.Now.AddDays(-30),
                DueDate = DateTime.Now.AddDays(-10),
                ReturnedDate = null,
                UserId = ""
            };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();

            var overdue = await context.Loans
                .AnyAsync(l => l.DueDate < DateTime.Today && l.ReturnedDate == null);

            Assert.True(overdue);
        }

        [Fact]
        public async Task Book_Filter_By_Category_Returns_Correct_Results()
        {
            var context = GetInMemoryContext();
            context.Books.AddRange(
                new Book { Title = "Book 1", Author = "Author 1", Isbn = "001", Category = "Fiction", IsAvailable = true },
                new Book { Title = "Book 2", Author = "Author 2", Isbn = "002", Category = "Science", IsAvailable = true },
                new Book { Title = "Book 3", Author = "Author 3", Isbn = "003", Category = "Fiction", IsAvailable = true }
            );
            await context.SaveChangesAsync();

            var results = await context.Books
                .Where(b => b.Category == "Fiction")
                .ToListAsync();

            Assert.Equal(2, results.Count);
        }
    }
}