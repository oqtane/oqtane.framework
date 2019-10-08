using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class FileController : Controller
    {
        private readonly IWebHostEnvironment environment;
        
        public FileController(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        // GET: api/<controller>?folder=x
        [HttpGet]
        public IEnumerable<string> Get(string folder)
        {
            List<string> files = new List<string>();
            folder = folder.Replace("/", "\\");
            if (folder.StartsWith("\\")) folder = folder.Substring(1);
            folder = Path.Combine(environment.WebRootPath, folder);
            if (Directory.Exists(folder))
            {
                foreach(string file in Directory.GetFiles(folder))
                {
                    files.Add(file);
                }
            }
            return files;
        }

        // POST api/<controller>/upload
        [HttpPost("upload")]
        public async Task UploadFile(string folder, IFormFile file)
        {
            if (file.Length > 0)
            {
                if (!folder.Contains(":\\"))
                {
                    folder = folder.Replace("/", "\\");
                    if (folder.StartsWith("\\")) folder = folder.Substring(1);
                    folder = Path.Combine(environment.WebRootPath, folder);
                }
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                using (var stream = new FileStream(Path.Combine(folder, file.FileName), FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                await MergeFile(folder, file.FileName);
            }
        }

        private async Task MergeFile(string folder, string filename)
        {
            // parse the filename which is in the format of filename.ext.part_x_y 
            string token = ".part_";
            string parts = Path.GetExtension(filename).Replace(token, ""); // returns "x_y"
            int totalparts = int.Parse(parts.Substring(parts.IndexOf("_") + 1));
            filename = filename.Substring(0, filename.IndexOf(token)); // base filename
            string[] fileparts = Directory.GetFiles(folder, filename + token + "*"); // list of all file parts

            // if all of the file parts exist ( note that file parts can arrive out of order )
            if (fileparts.Length == totalparts)
            {
                // merge file parts
                bool success = true;
                using (var stream = new FileStream(Path.Combine(folder, filename), FileMode.Create))
                {
                    foreach (string filepart in fileparts)
                    {
                        try
                        {
                            using (FileStream chunk = new FileStream(filepart, FileMode.Open))
                            {
                                await chunk.CopyToAsync(stream);
                            }
                        }
                        catch 
                        {
                            success = false;
                        }
                    }
                }

                // delete file parts
                if (success)
                {
                    foreach (string filepart in fileparts)
                    {
                        System.IO.File.Delete(filepart);
                    }
                }
            }

            // clean up file parts which are more than 2 hours old ( which can happen if a file upload failed )
            fileparts = Directory.GetFiles(folder, "*" + token + "*");
            foreach (string filepart in fileparts)
            {
                DateTime createddate = System.IO.File.GetCreationTime(filepart);
                if (createddate < DateTime.Now.AddHours(-2))
                {
                    System.IO.File.Delete(filepart);
                }
            }
        }
    }
}
