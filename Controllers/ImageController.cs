//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.EntityFrameworkCore;
//using ApiBackend.Models;
//using ApiBackend.Models.Context;

//namespace ApiBackend.Controllers
//{

//    [Authorize(Roles = "Personnel,Admin")]
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ImageController : ControllerBase
//    {
//        private readonly TestContext _context;

//        public ImageController(TestContext context)
//        {
//            _context = context;
//        }
        
//        // POST: api/image/uploadimage [RBAC: Personnel]
//        [Authorize(Roles = "Personnel,Admin")]
//        [HttpPost]
//        [Route("uploadimage")]
//        public async Task<ActionResult<TestItem>> PostUploadImage(TestItem image)
//        {
//            _context.TestItems.Add(image);
//            await _context.SaveChangesAsync();

//            return CreatedAtAction("GetTestItem", new { id = image.Id }, image);
//        }
//    }
//}
