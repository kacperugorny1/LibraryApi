using LibraryApi.Models;
using LibraryApi.Data;
using LibraryApi.Dtos;

using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Npgsql.PostgresTypes;
using Npgsql.Replication.PgOutput.Messages;
using System.Diagnostics;
using System.Data.Common;

/*
 * TODO:
 * 
 * -
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
        [HttpGet("GetBorrowings")]
        public IEnumerable<dynamic> GetBorrowings(int user_ind)
        {
            int customerId = _dapperAdd.LoadDataFirstOrDefault<int>($"SELECT customer_id from customer where auth_id = {user_ind}");
            string sql = @$"SELECT * FROM borrowing where customer_id = {customerId}";
            return _dapperRead.LoadData<dynamic>(sql);
        }
        [Authorize]
        [HttpGet("GetMyBorrowings")]
        public IEnumerable<dynamic> GetMyBorrowings()
        {
            string? userId = User.FindFirst("userId")?.Value;
            int customerId = _dapperAdd.LoadDataFirstOrDefault<int>($"SELECT customer_id from customer where auth_id = {userId}");
            string sql = @$"SELECT * FROM borrowing where customer_id = {customerId}";
            return _dapperRead.LoadData<dynamic>(sql);
        }

        [HttpGet("GetBookings")]
        public IEnumerable<dynamic> GetBookings(int user_ind)
        {
            int customerId = _dapperAdd.LoadDataFirstOrDefault<int>($"SELECT customer_id from customer where auth_id = {user_ind}");
            string sql = @$"SELECT * FROM booking where customer_id = {customerId}";
            return _dapperRead.LoadData<dynamic>(sql);
        }
        [Authorize]
        [HttpGet("GetMyBookings")]
        public IEnumerable<dynamic> GetMyBookings()
        {
            string? userId = User.FindFirst("userId")?.Value;
            int customerId = _dapperAdd.LoadDataFirstOrDefault<int>($"SELECT customer_id from customer where auth_id = {userId}");
            string sql = @$"SELECT * FROM booking where customer_id = {customerId}";
            return _dapperRead.LoadData<dynamic>(sql);
        }

        [HttpGet("FindCustomers")]
        public IEnumerable<Customer> FindCustomers(string name, string lastname)
        {
            string sql = @$"SELECT * FROM customer WHERE UPPER(first_name) LIKE UPPER('%{name}%') AND UPPER(last_name) LIKE UPPER('%{name}%')";
            return _dapperRead.LoadData<Customer>(sql);
        }
        [HttpGet("GetAssortmentId")]
        public IEnumerable<dynamic> GetAssortment(int book_id, int lib_id)
        {
            string sql = @$"select access,count(*) from assortment where book_id = {book_id} and library_id = {lib_id} GROUP BY access";
            return _dapperRead.LoadData<dynamic>(sql);

        }
        [HttpGet("GetAssortmentFull")]
        public IEnumerable<dynamic> GetAssortmentFull(int book_id)
        {
            string sql = @$"select access, library_id , count(*) from assortment where book_id = {book_id} GROUP BY access, library_id";
            return _dapperRead.LoadData<dynamic>(sql);
        }

        [HttpGet("GetAssortmentNotGruped")]
        public IEnumerable<dynamic> GetAssortmentNotGruped(int book_id, int lib_id)
        {
            string sql = @$"select assortment_id,access, library_id from assortment where book_id = {book_id} and library_id = {lib_id}";
            return _dapperRead.LoadData<dynamic>(sql);
        }
        [HttpGet("GetAssortmentForLib")]
        public IEnumerable<dynamic> GetAssortmentForLib(int book_id, int lib_id)
        {
            string sql = @$"select a.assortment_id, a.access, b.booking_id, br.borrowing_id from assortment as a left join booking as b ON b.assortment_id=a.assortment_id left join borrowing as br on br.assortment_id=a.assortment_id where a.book_id = {book_id} and a.library_id = {lib_id}";
            return _dapperRead.LoadData<dynamic>(sql);
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
            string sql = @$"SELECT b.book_id,title, author, publisher, publication_year, language, category_name, url FROM Book as b left JOIN category as c on b.book_id=c.book_id
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

        [HttpGet("GetBooksFullQuery")]
        public IEnumerable<BookFull> GetBooksFullQuery(string query, int count)
        {
            string sql = @$"SELECT b.book_id,title, author, publisher, publication_year, language, category_name, url FROM Book as b left JOIN category as c on b.book_id=c.book_id
                            left JOIN category_name as cn on cn.category_name_id=c.category_name_id WHERE UPPER(title) like UPPER('%{query}%') OR UPPER(author) like UPPER('%{query}%')";
            List<BookInfo> books = _dapperRead.LoadData<BookInfo>(sql).ToList();
            List<BookFull> booksFull = new();
            foreach (BookInfo book in books)
            {
                int ind = booksFull.FindIndex(u => u.Book_id == book.Book_id);
                if (ind == -1)
                {
                    if (booksFull.Count > count)
                        break;
                    booksFull.Add(new BookFull { Book_id = book.Book_id, Author = book.Author, Language = book.Language, Publication_year = book.Publication_year, Publisher = book.Publisher, Title = book.Title, Url = book.Url, Categories = new() });
                    if (book.Category_name != null)
                        booksFull.Last().Categories.Add(book.Category_name);
                }
                else booksFull[ind].Categories.Add(book.Category_name);
            }
            return booksFull;
        }

        [HttpGet("GetBooksFullQueryCount")]
        public int GetBooksFullQueryCount(string query)
        {
            string sql = @$"SELECT count(*) FROM Book WHERE UPPER(title) like UPPER('%{query}%') OR UPPER(author) like UPPER('%{query}%')";
            int books = _dapperRead.LoadDataFirstOrDefault<int>(sql);
            return books;
        }
        [HttpGet("GetBookFull")]
        public IEnumerable<BookFull> GetBookFull(int index)
        {
            string sql = @$"SELECT b.book_id,title, author, publisher, publication_year, language, category_name, url FROM Book as b left JOIN category as c on b.book_id=c.book_id
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

        [Authorize]
        [HttpPost("BookABook")]
        public IActionResult BookABook(int assortmentId)
        {
            //BEFORE INSERT TRIGGER TO DO
            string? userId = User.FindFirst("userId")?.Value;
            if (userId == null) StatusCode(501, "unexcepted");
            int customerId = _dapperAdd.LoadDataFirstOrDefault<int>($"SELECT customer_id from customer where auth_id = {userId}");
            string sql = @$"INSERT INTO booking(assortment_id,customer_id,booking_date, booking_length)
                            VALUES ({assortmentId}, {customerId}, '{DateTime.Now:yyyy-MM-dd}', {3})";
            if (!_dapperAdd.ExecuteSql(sql))
                return StatusCode(501, "Sql didn't went through");
            //AFTER INSERT TRIGGER
            if(!_dapperAdd.ExecuteSql($@"
                                        UPDATE assortment 
                                        SET access = false 
                                        WHERE assortment_id = {assortmentId}"))
                return StatusCode(501, "Sql didn't went through");
            return Ok();
        }
        [Authorize]
        [HttpPost("UnBookABook")]
        public IActionResult UnBookABook(int assortmentId)
        {
            string? userId = User.FindFirst("userId")?.Value;
            if (userId == null) StatusCode(501, "unexcepted");
            int customerId = _dapperAdd.LoadDataFirstOrDefault<int>($"SELECT customer_id from customer where auth_id = {userId}");
            string sql = @$"SELECT count(*) FROM booking where customer_id={customerId} and assortment_id = {assortmentId}";
            if (_dapperAdd.LoadDataFirstOrDefault<int>(sql) == 0)
                return StatusCode(501, "NO BOOKINGS");

            string deleteQuery = $@"
                    DELETE FROM booking 
                    WHERE assortment_id = {assortmentId}";
            if (!_dapperAdd.ExecuteSql(deleteQuery))
                return StatusCode(501, "Couldnt delete");

            if (!_dapperAdd.ExecuteSql($@"
                                        UPDATE assortment 
                                        SET access = true 
                                        WHERE assortment_id = {assortmentId}"))
                return StatusCode(501, "CHANGE ACCESS IN ASSORTMENT FAILED");
            return Ok();
        }
        [Authorize(Policy = "Librarian")]
        [HttpPost("BorrowABook")]
        public IActionResult BorrowABook(int assortmentId, int customer_id)
        {
            //BEFORE INSERT TRIGGER TO DO
            string sql = $@"
                INSERT INTO borrowing (assortment_id, customer_id, borrowing_date, borrowing_length) 
                VALUES ({assortmentId}, {customer_id}, '{DateTime.Now:yyyy-MM-dd}', {14})";

            if (!_dapperAdd.ExecuteSql(sql))
                return StatusCode(501, "Sql didn't went through");
            //AFTER INSERT TRIGGERS
            if (!_dapperAdd.ExecuteSql($@"
                                        UPDATE assortment 
                                        SET access = false 
                                        WHERE assortment_id = {assortmentId}"))
                return StatusCode(501, "Sql didn't went through");
            if(_dapperAdd.LoadDataFirstOrDefault<int>($@"
                                                    SELECT COUNT(*) 
                                                    FROM booking 
                                                    WHERE assortment_id = {assortmentId}") > 0)
            {
                string deleteQuery = $@"
                    DELETE FROM booking 
                    WHERE assortment_id = {assortmentId}";

                if (!_dapperAdd.ExecuteSql(deleteQuery))
                    return StatusCode(501, "");
            }
            return Ok();
        }

        [Authorize(Policy = "Librarian")]
        [HttpPost("UnBorrowABook")]
        public IActionResult UnBorrowABook(int borrowingId)
        {
            int assortmentId = _dapperAdd.LoadDataFirstOrDefault<int>($"SELECT assortment_id from borrowing where borrowing_id = {borrowingId}");
            string sql = @$"DELETE FROM borrowing WHERE borrowing_id = {borrowingId}";
            if (!_dapperAdd.ExecuteSql(sql))
                return StatusCode(501, "Sql didn't went through");
            //after insert trigger
            if (!_dapperAdd.ExecuteSql($@"
                                        UPDATE assortment 
                                        SET access = true 
                                        WHERE assortment_id = {assortmentId}"))
                return StatusCode(501, "Sql didn't went through");
            return Ok();
        }
    }
}
