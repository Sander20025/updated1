using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFunds.Ui.Webapp.Data;
using SmartFunds.Ui.Webapp.Models;

namespace SmartFunds.Ui.Webapp.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly SmartFundsDbContext _dbContext;

        public TransactionsController(SmartFundsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index(int? id)
        {
            if (id.HasValue)
            {
                var organization = _dbContext.Organizations.SingleOrDefault(o => o.Id == id.Value);
                if (organization is null)
                {
                    return RedirectToAction("Index", "Organization");
                }

                ViewData["Organization"] = organization;
            }

            var query = _dbContext.Transactions.AsQueryable();
            if (id.HasValue)
            {
                query = query.Where(t => t.OrganizationId == id.Value);
            }
            var transactions = query
                .Include(t => t.Organization)
                .ToList();
            

            return View(transactions);
        }

        [HttpGet]
        public IActionResult Create(int? organizationId)
        {
            if (organizationId.HasValue)
            {
                var transaction = new Transaction
                {
                    OrganizationId = organizationId.Value,
                    Owner = string.Empty,
                    Remarks = string.Empty
                };
                return View(transaction);
            }

            var organizations = _dbContext.Organizations.ToList();
            ViewBag.Organizations = organizations;
            
            return View();
        }

        [HttpPost]
        public IActionResult Create(Transaction transaction)
        {
            if (!ModelState.IsValid)
            {
                return View(transaction);
            }

            transaction.TimeStamp = DateTime.UtcNow;

            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();

            return RedirectToAction("Index", new{Id = transaction.OrganizationId});
        }

        [HttpPost("[controller]/Delete/{id:int?}")]
        public IActionResult DeleteConfirmed(int id)
        {
            var transaction = _dbContext
                .Transactions
                .SingleOrDefault(o => o.Id == id);

            if (transaction is null)
            {
                return RedirectToAction("Index", "Organization");
            }

            _dbContext.Transactions.Remove(transaction);

            _dbContext.SaveChanges();

            return RedirectToAction("Index", new{Id = transaction.OrganizationId});
        }
    }
}
