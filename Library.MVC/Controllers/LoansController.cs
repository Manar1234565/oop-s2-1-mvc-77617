using Library.MVC.Data;
using Library.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Controllers
{
    public class LoansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Loans
        public async Task<IActionResult> Index()
        {
            var loans = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .ToListAsync();
            return View(loans);
        }

        // GET: Loans/Create
        public IActionResult Create()
        {
            ViewBag.Books = new SelectList(_context.Books.Where(b => b.IsAvailable), "Id", "Title");
            ViewBag.Members = new SelectList(_context.Members, "Id", "FullName");
            return View();
        }

        // POST: Loans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Loan loan)
        {
            // Verificar que el libro no tenga un préstamo activo
            var activeLoan = await _context.Loans
                .AnyAsync(l => l.BookId == loan.BookId && l.ReturnedDate == null);

            if (activeLoan)
            {
                ModelState.AddModelError("", "This book is already on an active loan.");
                ViewBag.Books = new SelectList(_context.Books.Where(b => b.IsAvailable), "Id", "Title");
                ViewBag.Members = new SelectList(_context.Members, "Id", "FullName");
                return View(loan);
            }

            if (ModelState.IsValid)
            {
                loan.LoanDate = DateTime.Now;
                loan.DueDate = DateTime.Now.AddDays(14);
                loan.UserId = User.Identity?.Name ?? "";

                // Marcar el libro como no disponible
                var book = await _context.Books.FindAsync(loan.BookId);
                if (book != null) book.IsAvailable = false;

                _context.Loans.Add(loan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Books = new SelectList(_context.Books.Where(b => b.IsAvailable), "Id", "Title");
            ViewBag.Members = new SelectList(_context.Members, "Id", "FullName");
            return View(loan);
        }

        // POST: Loans/Return/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null) return NotFound();

            loan.ReturnedDate = DateTime.Now;

            if (loan.Book != null)
                loan.Book.IsAvailable = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}