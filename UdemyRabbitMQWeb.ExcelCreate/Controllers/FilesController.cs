using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UdemyRabbitMQWeb.ExcelCreate.Models;

namespace UdemyRabbitMQWeb.ExcelCreate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FilesController(AppDbContext context)
        {
            _context = context;
        }

        //WorkerService'den gelecek, excel dosyası, hangi file?
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file,int fileId)
        {
            if(file is not { Length:>0 })return BadRequest();

            var userFile =await _context.UserFiles.FirstAsync(x=>x.Id== fileId);

            var filePath=userFile.FileName+Path.GetExtension(file.FileName);//filename.xlsx olacak.

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);

            using FileStream stream = new(path, FileMode.Create);
            
            await file.CopyToAsync(stream);
            
            userFile.CreatedDate = DateTime.Now;
            userFile.FilePath = filePath;
            userFile.FileStatus = FileStatus.Completed;
            await _context.SaveChangesAsync();
            //SignalR notification oluşturulacak
            return Ok();
        }
    }
}
