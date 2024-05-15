﻿using LibraryApi.Data;
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
            if (password != confirmPassword)
                return StatusCode(401, "Passwords do not match");
            Customer? cust = _dapperRead.LoadData<Customer>($"SELECT * FROM customer WHERE email='{email}'")?.FirstOrDefault();
            Staff? admin = _dapperRead.LoadData<Staff>($"SELECT * FROM staff WHERE email='{email}'")?.FirstOrDefault();
            Auth? creds = _dapperRead.LoadData<Auth>($"SELECT * FROM auth WHERE login='{login}'")?.FirstOrDefault();
            if (cust != null || admin != null)
                return StatusCode(401, "Email exists");
            else if (creds != null)
                return StatusCode(401, "Username exists");
            if (_dapperAdd.ExecuteSql($"INSERT INTO auth(login,password) VALUES ('{login}','{password}');"))
            {
                int ind = _dapperRead.LoadDataFirstOrDefault<int>($"SELECT auth_id FROM auth WHERE login='{login}'");

                if (_dapperAdd.ExecuteSql(@$"INSERT INTO customer(first_name,last_name,email,auth_id) VALUES
                                        ('{name}','{lastname}','{email}','{ind}')") == false)
                {
                    return StatusCode(401, "Falied to add customer");
                }
                return Ok();
            }
            return StatusCode(401,"Try again");
        }


        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(string login, string password)
        {
            Auth? auth = _dapperRead.LoadData<Auth>($"SELECT * FROM auth WHERE login='{login}' AND password='{password}'")?.FirstOrDefault();
            if (auth == null)
            {
                return StatusCode(401, "Wrong Credentials");
            }
            Staff? staff = _dapperRead.LoadDataFirstOrDefault<Staff>($"SELECT * FROM staff WHERE auth_id={auth.Auth_id}"); 
            dynamic token = _authHelper.CreateToken(auth.Auth_id, staff != null);
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
