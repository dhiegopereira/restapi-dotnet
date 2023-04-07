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
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context) {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Index() {
            var tasks = _context.Users.ToList();

            return Ok(tasks);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create([FromBody] User user) {
            if (user == null) {
                return BadRequest();
            }


            _context.Users.Add(user);
            _context.SaveChanges();

            return CreatedAtRoute("GetUser", new { id = user.Id }, user);
        }

        [HttpGet("{id}", Name = "GetUser")]
        [Authorize]
        public IActionResult Read(int id) {
            var user = _context.Users.Find(id);

            if (user == null) {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPut("{id}")]
        [Authorize]
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
        [Authorize]
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
                return BadRequest(new { message = "Usuário ou senha inválidos" });
            }

            var SecretKey = configuration.GetSection("JWT")["SecretKey"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Name, existingUser.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new { token = tokenHandler.WriteToken(token) });
        }
    }

}
