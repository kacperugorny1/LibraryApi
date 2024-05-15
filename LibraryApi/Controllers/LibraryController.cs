using LibraryApi.Models;
using LibraryApi.Data;
using LibraryApi.Dtos;

using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;

namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LibraryController : ControllerBase
    {
        private readonly DataContextDapperMaster _dapperAdd;
        private readonly DataContextDapperSlave _dapperRead;
        public LibraryController(IConfiguration config)
        {
            _dapperAdd = new DataContextDapperMaster(config);
            _dapperRead = new DataContextDapperSlave(config);
        }

        [HttpGet("GetCustomers")]
        public IEnumerable<Customer> GetCustomers()
        {
            string sql = @$"SELECT * FROM customer";
            return _dapperRead.LoadData<Customer>(sql);
        }

        [HttpGet("GetLibraries")]
        public IEnumerable<Library> GetLibraries()
        {
            string sql = @$"SELECT * FROM library";
            return _dapperRead.LoadData<Library>(sql);
        }

        [HttpGet("GetBooks")]
        public IEnumerable<Book> GetBooks()
        {
            string sql = @$"SELECT * FROM Book";
            return _dapperRead.LoadData<Book>(sql);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost("AddBook")]
        public IActionResult addBook(BookDto book)
        {
            string sql = @$"INSERT INTO book(title, author, publisher, publication_year, language, category)
                            VALUES ('{book.Title}', '{book.Author}', '{book.Publisher}', {book.Publication_year},'{book.Language}')";
            _dapperAdd.ExecuteSql(sql);
            return Ok();
        }
    }
}
