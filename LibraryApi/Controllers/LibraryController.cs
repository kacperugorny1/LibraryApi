using LibraryApi.Models;
using LibraryApi.Data;
using LibraryApi.Dtos;

using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Npgsql.PostgresTypes;
using Npgsql.Replication.PgOutput.Messages;

/*
 * TODO:
 * -SZUKANIE W ENDPOINT GETBOOKSFULL - endpoint
 * -ZWRACANIE ILOŒCI KSI¥¯EK W QUERY - endpoint
 * 
 * 
 */

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

        [HttpGet("GetBooksFull")]
        public IEnumerable<BookFull> GetBooksFull()
        {
            string sql = @$"SELECT b.book_id,title, author, publisher, publication_year, language, category_name FROM Book as b left JOIN category as c on b.book_id=c.book_id
                            left JOIN category_name as cn on cn.category_name_id=c.category_name_id";
            List<BookInfo> books = _dapperRead.LoadData<BookInfo>(sql).ToList();
            List<BookFull> booksFull = new();
            foreach (BookInfo book in books)
            {
                int ind = booksFull.FindIndex(u=> u.Book_id == book.Book_id);
                if(ind == -1)
                {
                    booksFull.Add(new BookFull { Book_id = book.Book_id, Author = book.Author, Language = book.Language, Publication_year = book.Publication_year, Publisher = book.Publisher, Title = book.Title, Url = book.Url, Categories = new() }) ;
                    if (book.Category_name != null)
                        booksFull.Last().Categories.Add(book.Category_name);
                }
                else booksFull[ind].Categories.Add(book.Category_name);
            }
            return booksFull;
        }

        [HttpGet("GetBookFull")]
        public IEnumerable<BookFull> GetBookFull(int index)
        {
            string sql = @$"SELECT b.book_id,title, author, publisher, publication_year, language, category_name FROM Book as b left JOIN category as c on b.book_id=c.book_id
                            left JOIN category_name as cn on cn.category_name_id=c.category_name_id WHERE b.book_id={index}";
            List<BookInfo> books = _dapperRead.LoadData<BookInfo>(sql).ToList();
            List<BookFull> booksFull = new();
            foreach (BookInfo book in books)
            {
                int ind = booksFull.FindIndex(u => u.Book_id == book.Book_id);
                if (ind == -1)
                {
                    booksFull.Add(new BookFull { Book_id = book.Book_id, Author = book.Author, Language = book.Language, Publication_year = book.Publication_year, Publisher = book.Publisher, Title = book.Title, Url = book.Url, Categories = new() });
                    if (book.Category_name != null)
                        booksFull.Last().Categories.Add(book.Category_name);

                }
                else booksFull[ind].Categories.Add(book.Category_name);
            }
            return booksFull;
        }


        [HttpGet("GetAssortment")]
        public IEnumerable<Assortment> GetAssortment(int index)
        {
            string sql = @$"SELECT * FROM ASSORTMENT WHERE book_id={index}";
            return _dapperRead.LoadData<Assortment>(sql);
        }

        [HttpGet("GetCategoryNames")]
        public IEnumerable<CategoryName> GetCategoryNames()
        {
            return _dapperRead.LoadData<CategoryName>("SELECT * FROM category_name");
        }

        [Authorize("AdminOnly")]
        [HttpPost("AddLibrary")]
        public IActionResult AddLibrary(string name, string address)
        {
            string sql = $@"Insert into library(name,address) values ('{name}','{address}')";
            if (_dapperAdd.ExecuteSql(sql))
                return Ok();
            return StatusCode(501, "Sql didn't went through");
        }

        [Authorize(Policy = "Librarian")]
        [HttpPost("AddCategory")]
        public IActionResult addCategory(string category)
        {
            string sql = @$"Insert INTO category_name(category_name) values('{category}')";
            if (_dapperAdd.ExecuteSql(sql))
                return Ok();
            return StatusCode(501, "Sql didn't went through");
        } 

        [Authorize(Policy = "Librarian")]
        [HttpPost("AddBook")]
        public IActionResult addBook(BookDto book)
        {
            string sql = @$"INSERT INTO book(title, author, publisher, publication_year, language, url)
                            VALUES ('{book.Title}', '{book.Author}', '{book.Publisher}', {book.Publication_year},'{book.Language}','{book.Url}')";
            if (_dapperAdd.ExecuteSql(sql))
                return Ok();
            return StatusCode(501, "Sql didn't went through");
        }

        [Authorize(Policy = "Librarian")]
        [HttpPost("AddCategoryToABook")]
        public IActionResult AddCategoryToABook(int category_id, int book_id)
        {
            string sql = @$"INSERT INTO category(category_name_id, book_id) values ({category_id}, {book_id})";
            if (_dapperAdd.ExecuteSql(sql))
                return Ok();
            return StatusCode(501, "Sql didn't went through");
        }

        [Authorize(Policy = "Librarian")]
        [HttpPost("AddBookFull")]
        public IActionResult AddBookFull(BookFullDto book)
        {
            string sql = @$"INSERT INTO book(title, author, publisher, publication_year, language, url)
                            VALUES ('{book.Title}', '{book.Author}', '{book.Publisher}', {book.Publication_year},'{book.Language}','{book.Url}')";
            if (!_dapperAdd.ExecuteSql(sql))
                return StatusCode(501, "Failed to add book");
            int id = _dapperAdd.LoadDataFirstOrDefault<Book>($"SELECT * FROM book WHERE title = '{book.Title}' AND author = '{book.Author}' AND publication_year = {book.Publication_year}").Book_id;
            sql = $@"INSERT INTO category(category_name_id,book_id) values ";
            for(int i = 0; i < book.Categories.Count; i++)
            {
                sql += $"({book.Categories[i]},{id})";
                if (i < book.Categories.Count - 1)
                    sql += ", ";
            }

            if (_dapperAdd.ExecuteSql(sql))
                return Ok();
            return StatusCode(501, "Sql didn't went through");
        }

        [Authorize(Policy = "Librarian")]
        [HttpPost("AddAssortment")]
        public IActionResult addAssortment(int book_id)
        {
            string sql = @$"INSERT INTO assortment(book_id, access, library_id)
                            VALUES ({book_id}, true, {User.FindFirst("library").Value})";
            if(_dapperAdd.ExecuteSql(sql))
                return Ok();
            return StatusCode(501, "Sql didn't went through");
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost("AddAssortmentAdmin")]
        public IActionResult addAssortmentAdmin(int book_id, int Lib_id)
        {
            string sql = @$"INSERT INTO assortment(book_id, access, library_id)
                            VALUES ({book_id}, true, {Lib_id})";
            if (_dapperAdd.ExecuteSql(sql))
                return Ok();
            return StatusCode(501, "Sql didn't went through");
        }
    }
}
