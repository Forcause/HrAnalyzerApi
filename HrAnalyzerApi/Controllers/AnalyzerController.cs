using HrAnalyzer.Data.Models;
using HrAnalyzerApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using HrAnalyzer.Data.Constants;
using HrAnalyzer.Data.Services;
using System.Text.Json.Nodes;

namespace HrAnalyzerApi.Controllers
{
    [Route("api")]
    [ApiController]
    public class AnalyzerController : ControllerBase
    {
        private const string searchPattern = "*.json";

        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IAnalyzerService _analyzerService;

        public AnalyzerController(IWebHostEnvironment hostingEnvironment, IAnalyzerService analyzerService)
        {
            _hostingEnvironment = hostingEnvironment;
            _analyzerService = analyzerService;
        }

        [HttpPost("upload-file")]
        public async Task<ActionResult<AnalyzeResult>> LoadFile([FromForm] FileUploadModel model, [FromQuery] User user)
        {
            if (model.File == null || model.File.Length == 0)
                return BadRequest("No file received");

            var realUser = new User(user.Gender, user.Name, user.Age, user.Weight, user.Height);

            var fileName = Path.GetFileName(model.File.FileName);
            var randomFolderName = Guid.NewGuid().ToString();

            var folderPath = Path.Combine(@"F:\Learn\HrAnalyzerApi\HrAnalyzerApi", randomFolderName);

            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.File.CopyToAsync(fileStream);
            }

            RunPythonScript(folderPath);

            PpgFileData ppgFileData = null;
            try
            {
                foreach (var path in Directory.EnumerateFiles(folderPath, searchPattern))
                {
                    var dataFileName = Path.GetFileName(path);
                    Console.WriteLine($"File name: {dataFileName}");

                    // Read the file content
                    var fileContent = await System.IO.File.ReadAllTextAsync(path);

                    // Deserialize the JSON content to an object
                    ppgFileData = JsonConvert.DeserializeObject<PpgFileData>(fileContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            var results = await _analyzerService.AnalyzeData(realUser, ppgFileData);

            return Ok(results);
        }

        private void RunPythonScript(string folderPath)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = @"F:\Learn\HrAnalyzerApi\venv\Scripts\python.exe", // Set the FileName to the path of the Python interpreter executable in the venv folder
                Arguments = $"F:\\Learn\\HrAnalyzerApi\\HrAnalyzerApi\\Scripts\\semi_final_algo.py \"{folderPath}\"", // Set the Arguments to the path of the Python script file
                RedirectStandardOutput = false,
                UseShellExecute = false,
                CreateNoWindow = false,
                WorkingDirectory = @"F:\Learn\HrAnalyzerApi\HrAnalyzerApi\Scripts", // Set the WorkingDirectory to the directory containing the Python script file
            };

            // Set up the Python environment with libraries from the venv folder
            var pythonLibPath = @"F:\Learn\DnnPpg\venv\Lib\site-packages";
            processStartInfo.EnvironmentVariables["PYTHONPATH"] = pythonLibPath;

            var process = new Process
            {
                StartInfo = processStartInfo
            };

            process.Start();
            process.WaitForExit();
        }
    }
}
