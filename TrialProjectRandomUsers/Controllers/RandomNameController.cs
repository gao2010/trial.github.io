using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TrialProjectRandomUsers.Models;
using System.IO;
namespace TrialProjectRandomUsers.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public partial class RandomNameController : ControllerBase
    {
        private readonly StatisticsHelper Helper = new();
        [HttpGet]
        [Route("Test/GettingRandomUserData10")]
        public async Task<Person> GetPersonAsync()
        {
            HttpClient client = new() { };
            HttpResponseMessage response;
            response = await client.GetAsync("https://randomuser.me/api/?results=10&nat=us&format=json");
            response.EnsureSuccessStatusCode();
            string stringResult = await response.Content.ReadAsStringAsync();
            Person root = JsonConvert.DeserializeObject<Person>(stringResult);
            return root;
        }

        [HttpGet]
        [Route("Test/BonusReceiveFormatFromRequest")]
        public string ReceiveFormatFromRequest()
        {

            try
            {
                if (Request.Headers.TryGetValue("Accept", out var acceptedTypes))
                {
                    // Get the first accepted type (client might specify preferences)
                    var acceptValue = acceptedTypes.FirstOrDefault();
                    if (acceptValue != null)
                        if (acceptValue.StartsWith("application/"))
                        {
                            var parts = acceptValue.Split('/');
                            if (parts.Length > 1)
                            {
                                return parts[1].ToLower(); // Return suggested format (lowercase)
                            }
                        }
                        // Check for specific extensions based on known patterns
                        else if (acceptValue.Contains(".json"))
                        {
                            return "json";
                        }
                        else if (acceptValue.Contains(".xml"))
                        {
                            return "xml";
                        }
                    return "txt";
                }
                else
                {
                    return null; // Return null if Content-Type header is missing
                }

            }
            catch (Exception)
            {
                return "txt";
            }
        }

        [HttpGet]
        [Route("Test/ViewReport")]
        public IActionResult DownloadReport()
        {
            // Generate report data (e.g., string, byte array)
            string reportData = Helper.GetStatistics(GetPersonAsync().Result, "json");
            var contentEncoding = Encoding.UTF8;
            // Set response headers
            return Content(reportData, "text/plain", contentEncoding);
        }
        [HttpPost]
        [Route("Api/DownloadReport")]
        public IActionResult DownloadReportWithUserInputs(Person root, string fileType)
        {
            // Generate report data (e.g., string, byte array)          
            string data = Helper.GetStatistics(root, fileType);
            if (string.IsNullOrEmpty(data))
            {
                return BadRequest("Please provide data to download");
            }
            // Create a MemoryStream to hold the data in memory
            var memoryStream = new MemoryStream();
            // Convert data to a byte array (assuming string data)
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(data);

            // Write the byte array to the MemoryStream
            memoryStream.Write(dataBytes, 0, dataBytes.Length);
            memoryStream.Seek(0, SeekOrigin.Begin); // Rewind the stream
            var contentType = "";
            var fileName = "";
            // Set response properties
            if (fileType == "txt")
            {
                contentType = "text/plain"; // Adjust based on content type
                fileName = "statistics.txt";
            }
            else if (fileType == "json")
            {
                contentType = "application/json";
                fileName = "statistics.json";
            }
            else
            {
                contentType = "application/xml";
                fileName = "statistics.xml";

            }
            return File(memoryStream, contentType, fileName, true); // Set "true" for download

        }
        [HttpPost]
        [Route("Api/BonusDownloadReport")]
        public IActionResult DownloadReportAccordingToHeader(Person root)
        {
            // Generate report data (e.g., string, byte array)
            var fileType = ReceiveFormatFromRequest();
            string data = Helper.GetStatistics(root, fileType);
            if (string.IsNullOrEmpty(data))
            {
                return BadRequest("Please provide data to download");
            }
            // Create a MemoryStream to hold the data in memory
            var memoryStream = new MemoryStream();
            // Convert data to a byte array (assuming string data)
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(data);

            // Write the byte array to the MemoryStream
            memoryStream.Write(dataBytes, 0, dataBytes.Length);
            memoryStream.Seek(0, SeekOrigin.Begin); // Rewind the stream
            var contentType = "";
            var fileName = "";
            // Set response properties
            if (fileType == "txt")
            {
                contentType = "text/plain"; // Adjust based on content type
                fileName = "statistics.txt";
            } 
            else if(fileType == "json")
            {
                contentType = "application/json";
                fileName = "statistics.json";
            }
            else
            {
                contentType = "application/xml";
                fileName = "statistics.xml";

            }
            return File(memoryStream, contentType, fileName, true); // Set "true" for download

        }
    }
}