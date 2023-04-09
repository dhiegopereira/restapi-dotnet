using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoList.Data;
using TodoList.Models;

namespace TodoList.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskController(ApplicationDbContext context) {
            _context = context;
        }

        [HttpGet]        
        public IActionResult Index() {
            var tasks = _context.Items.ToList();

            return Ok(tasks);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create([FromBody] Item task) {
            if (task == null) {
                return BadRequest();
            }
            
            task.CreatedDate = DateTime.Now;
            task.IsCompleted = false;

            _context.Items.Add(task);
            _context.SaveChanges();

            return CreatedAtRoute("GetTask", new { id = task.Id }, task);
        }

        [HttpGet("{id}", Name = "GetTask")]
        [Authorize]
        public IActionResult Read(int id) {
            var task = _context.Items.Find(id);

            if (task == null) {
                return NotFound();
            }

            return Ok(task);
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Update(int id, [FromBody] Item task) {
            if (task == null || task.Id != id) {
                return BadRequest();
            }

            var existingTask = _context.Items.Find(id);

            if (existingTask == null) {
                return NotFound();
            }

            existingTask.Name = task.Name;
            existingTask.Description = task.Description;
            existingTask.IsCompleted = task.IsCompleted;
            existingTask.UpdatedDate = DateTime.Now;

            _context.Items.Update(existingTask);
            _context.SaveChanges();

            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id) {
            var task = _context.Items.Find(id);

            if (task == null) {
                return NotFound();
            }

            _context.Items.Remove(task);
            _context.SaveChanges();

            return new NoContentResult();
        }
    }

}
