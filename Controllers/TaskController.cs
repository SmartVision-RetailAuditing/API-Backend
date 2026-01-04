using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiBackend.Models;
using ApiBackend.Models.Context;

namespace ApiBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly TestContext _context;

        public TaskController(TestContext context)
        {
            _context = context;
        }
        
        // GET: api/task/getasks [RBAC: Personnel]
        [HttpGet]
        [Route("gettasks/{personnelId}")]
        public async Task<ActionResult<IEnumerable<TestItem>>> GetAssignedTasks(long personnelId)
        {
            return await _context.TestItems.ToListAsync();
        }

        // POST: api/task/assigntask [RBAC: Supervisor]
        [HttpPost]
        [Route("assigntask")]
        public async Task<ActionResult<TestItem>> PostAssignTask(TestItem task)
        {
            _context.TestItems.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTestItem", new { id = task.Id }, task);
        }

    }
}
