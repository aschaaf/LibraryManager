using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryManager.Models;

namespace LibraryManager.Interfaces
{
    public interface IDynamoDBAdapter
    {
        Task<Book> GetBookById(string id);
        Task<Transaction> GetTransactionById(string id);
        Task<Student> GetStudentById(string id);
        Task<Profile> GetProfileByStudentId(string id);
        Task<List<Book>> GetBooksByStudentId(string id);
        Task<List<Book>> GetBooksByAvailability(string available);
        Task<List<Transaction>> GetTransactionsByStatus(string status);
        Task<bool> InsertBook(Book book);
        Task<bool> InsertTransaction(Transaction transaction);
        Task<bool> UpdateBookAvailability(string id, string available);
        Task<bool> CloseTransaction(Transaction transaction);
    }
}
