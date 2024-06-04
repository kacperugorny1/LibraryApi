using LibraryApi.Data;
using LibraryApi.Models;
using LibraryApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Antiforgery;

namespace LibraryApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapperMaster _dapperAdd;
        private readonly DataContextDapperSlave _dapperRead;
        private readonly AuthHelper _authHelper;
        public AuthController(IConfiguration config)
        {
            _dapperAdd = new DataContextDapperMaster(config);
            _dapperRead = new DataContextDapperSlave(config);
            _authHelper = new AuthHelper(config);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(string name, string lastname, string email, string login, string password, string confirmPassword)
        {
            string sql = $@"CALL add_customer('{login}', '{password}', '{confirmPassword}', '{email}', '{name}', '{lastname}')";
            if(_dapperAdd.ExecuteSql(sql))
                return Ok();
            return StatusCode(401, "MISTAKE");
        }
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("RegisterStaff")]
        public IActionResult RegisterStaff(string name, string lastname, string email, string login, string password, string confirmPassword, int lib_id)
        {
            string sql = $@"CALL register_staff('{name}', '{lastname}', '{email}', '{login}', '{password}', '{confirmPassword}', {lib_id})";
            if (_dapperAdd.ExecuteSql(sql))
                return Ok();
            return StatusCode(401, "MISTAKE");
        }


        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(string login, string password)
        {
            Staff? staff = null;
            bool admin = false;
            string sql = $@"PREPARE auth_query (text, text) AS
                            SELECT * FROM auth WHERE login = $1 AND password = $2;
                            EXECUTE auth_query('{login}', '{password}');
                            DEALLOCATE auth_query;";
            Auth? auth = _dapperRead.LoadData<Auth>(sql)?.FirstOrDefault();
            if (auth == null)
            {
                return StatusCode(401, "Wrong Credentials");
            }
            if (auth.User_type == "librarian")
            {
                staff = _dapperRead.LoadDataFirstOrDefault<Staff>($"SELECT * FROM staff WHERE auth_id={auth.Auth_id}");
            }
            else if (auth.User_type == "admin") admin = true;
            dynamic token = _authHelper.CreateToken(auth.Auth_id, admin, staff != null, staff?.Library_id == null ? -1 : staff.Library_id);
            HttpContext.Response.Cookies.Append("token", token,
            new CookieOptions
            {
                Expires = DateTime.Now.AddDays(1),
                HttpOnly = false,
                Secure = false,
                IsEssential = true,
                SameSite = SameSiteMode.Unspecified
            });
            //Could be just
            //return Ok();
            return Ok(new Dictionary<string, string>{
                    {"token", token}
                });
        }

        [HttpGet("RefreshToken")]
        public string RefreshToken()
        {
            return _authHelper.CreateToken(int.Parse(User.FindFirst("userId")?.Value));
        }

        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Append("token", "",
                new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(1),
                    HttpOnly = false,
                    Secure = false,
                    IsEssential = true,
                    SameSite = SameSiteMode.Unspecified
                });

            return Ok();
        }


        [HttpGet("IsLogged")]
        public IActionResult IsLogged()
        {
            return Ok();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("IsLoggedAdmin")]
        public IActionResult IsLoggedAdmin()
        {
            return Ok();
        }

        [Authorize(Policy = "Librarian")]
        [HttpGet("IsLoggedLibrarian")]
        public IActionResult IsLoggedLibrarian()
        {
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("GetJwtTokenFromCookie")]
        public IActionResult GetJwtTokenFromCookie()
        {
            if (Request.Cookies.TryGetValue("token", out var token))
            {
                return Ok(token);
            }

            return Unauthorized();
        }


    }
}
