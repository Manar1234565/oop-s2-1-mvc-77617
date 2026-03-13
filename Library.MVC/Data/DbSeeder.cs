using Bogus;
using Library.MVC.Models;
using Microsoft.AspNetCore.Identity;

namespace Library.MVC.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Si ya hay datos, no volver a sembrar
            if (context.Books.Any()) return;

            // Seed Admin Role + User
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            var adminEmail = "admin@library.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Seed Books (20)
            var bookFaker = new Faker<Book>()
                .RuleFor(b => b.Title, f => f.Lorem.Sentence(3))
                .RuleFor(b => b.Author, f => f.Name.FullName())
                .RuleFor(b => b.Isbn, f => f.Commerce.Ean13())
                .RuleFor(b => b.Category, f => f.PickRandom(new[] { "Fiction", "Science", "History", "Technology", "Art" }))
                .RuleFor(b => b.IsAvailable, true);

            var books = bookFaker.Generate(20);
            context.Books.AddRange(books);

            // Seed Members (10)
            var memberFaker = new Faker<Member>()
                .RuleFor(m => m.FullName, f => f.Name.FullName())
                .RuleFor(m => m.Email, f => f.Internet.Email())
                .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber());

            var members = memberFaker.Generate(10);
            context.Members.AddRange(members);

            await context.SaveChangesAsync();

            // Seed Loans (15)
            var random = new Random();
            var loans = new List<Loan>();

            for (int i = 0; i < 15; i++)
            {
                var book = books[i % books.Count];
                var member = members[random.Next(members.Count)];
                var loanDate = DateTime.Now.AddDays(-random.Next(1, 60));
                var dueDate = loanDate.AddDays(14);

                DateTime? returnedDate = null;

                if (i < 5) // 5 returned
                {
                    returnedDate = dueDate.AddDays(-random.Next(1, 5));
                    book.IsAvailable = true;
                }
                else if (i < 10) // 5 active
                {
                    book.IsAvailable = false;
                }
                else // 5 overdue (DueDate < Today, not returned)
                {
                    dueDate = DateTime.Now.AddDays(-random.Next(1, 15));
                    book.IsAvailable = false;
                }

                loans.Add(new Loan
                {
                    Book = book,
                    Member = member,
                    UserId = "",
                    LoanDate = loanDate,
                    DueDate = dueDate,
                    ReturnedDate = returnedDate
                });
            }

            context.Loans.AddRange(loans);
            await context.SaveChangesAsync();
        }
    }
}