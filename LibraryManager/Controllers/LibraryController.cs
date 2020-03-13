using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryManager.Interfaces;
using LibraryManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private IDynamoDBAdapter dynamoDBAdapter;

        public LibraryController(IDynamoDBAdapter dynamoDBAdapter)
        {
            this.dynamoDBAdapter = dynamoDBAdapter;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Book>>> Get()
        {
            var books = await dynamoDBAdapter.GetBooksByAvailability("true");
            return books;
        }
        
        [HttpGet("book/{id}")]
        public async Task<ActionResult<Book>> GetBook(string id)
        {
            var book = await dynamoDBAdapter.GetBookById(id);
            return book;
        }

        [HttpGet("student/{id}")]
        public async Task<ActionResult<Student>> GetStudent(string id)
        {
            var student = await dynamoDBAdapter.GetStudentById(id);
            return student;
        }

        [HttpGet("transaction/{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(string id)
        {
            var transaction = await dynamoDBAdapter.GetTransactionById(id);
            return transaction;
        }

        [HttpGet("student/{id}/profile")]
        public async Task<ActionResult<Profile>> GetProfileByStudentId(string id)
        {
            var profile = await dynamoDBAdapter.GetProfileByStudentId(id);
            return profile;
        }

        [HttpGet("student/{id}/transactions")]
        public async Task<ActionResult<List<Book>>> GetBooksByStudentId(string id)
        {
            var books = await dynamoDBAdapter.GetBooksByStudentId(id);
            return books;
        }
        
        [HttpGet("transactions")]
        public async Task<ActionResult<List<Transaction>>> GetActiveTransactions()
        {
            var transactions = await dynamoDBAdapter.GetTransactionsByStatus("true");
            return transactions;
        }
        
        [HttpPost("insert")]
        public async Task<ActionResult<string>> InsertBook([FromBody] Book book)
        {
            var result = await dynamoDBAdapter.InsertBook(book);

            if (result)
            {
                return $"Successfully added {book.Title} to database";
            }
            else
            {
                return $"Failed to add {book.Title} to database";
            }
        }
        
        [HttpPost("checkout")]
        public async Task<ActionResult<string>> CheckOutBook([FromBody] Transaction transaction)
        {
            var result = await dynamoDBAdapter.InsertTransaction(transaction);

            if (result)
            {
                return $"Successfully processed transaction";
            }
            else
            {
                return $"Failed to process transaction";
            }
        }
        
        [HttpPost("checkin")]
        public async Task<ActionResult<string>> CheckInBook([FromBody] Transaction transaction)
        {
            var result = await dynamoDBAdapter.CloseTransaction(transaction);

            if (result)
            {
                return $"Successfully processed transaction";
            }
            else
            {
                return $"Failed to process transaction";
            }
        }
        
        [HttpPost("update")]
        public async Task<ActionResult<string>> UpdateBookStatus([FromBody] Book book)
        {
            var result = await dynamoDBAdapter.UpdateBookAvailability(book.Id, book.Available);

            if (result)
            {
                return $"Successfully updated book Id: {book.Id}";
            }
            else
            {
                return $"Failed to update book Id: {book.Id}";
            }
        }

        [HttpGet("ping")]
        public ActionResult<string> Ping()
        {
            return $"{nameof(LibraryController)} is currently running...";
        }
    }
}
