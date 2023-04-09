using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoList.Data;
using TodoList.Models;

namespace TodoList.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context) {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index() {
            var users = _context.Users.ToList();

            return Ok(users);
        }

        [HttpPost]
        public IActionResult Create([FromBody] User user) {
            if (user == null) {
                return BadRequest();
            }


            _context.Users.Add(user);
            _context.SaveChanges();

            return CreatedAtRoute("GetUser", new { id = user.Id }, user);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public IActionResult Read(int id) {
            var user = _context.Users.Find(id);

            if (user == null) {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] User user) {
            if (user == null || user.Id != id) {
                return BadRequest();
            }

            var existingTask = _context.Users.Find(id);

            if (existingTask == null) {
                return NotFound();
            }

            existingTask.Email = user.Email;
            existingTask.Password = user.Password;

            _context.Users.Update(existingTask);
            _context.SaveChanges();

            return new NoContentResult();
        }

        [HttpDelete("{id}")]        
        public IActionResult Delete(int id) {
            var user = _context.Users.Find(id);

            if (user == null) {
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return new NoContentResult();
        }

        [HttpPost("auth")]
        [AllowAnonymous]
        public IActionResult Auth([FromBody] User user, [FromServices] IConfiguration configuration) {
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email && u.Password == user.Password);

            if (existingUser == null) {
                return BadRequest(new { message = "User or password invalid" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            
            var SecretKey = configuration.GetSection("JWT")["SecretKey"];
            var audience = configuration.GetSection("JWT")["Audience"];
            var issuer = configuration.GetSection("JWT")["Issuer"];

            if (SecretKey == null) {
                return BadRequest(new { message = "Secret Key is not existing" });
            }

            var key = Encoding.UTF8.GetBytes(SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Email, existingUser.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Audience = audience,
                Issuer = issuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new { token = tokenHandler.WriteToken(token) });
        }

    }

}
