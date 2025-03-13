using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using Entrvo.DAL;
using Entrvo.Models;
using Entrvo.Services;
using Entrvo.Services.Models;

namespace Entrvo.Controllers
{
  public class UploadController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IScheduleService _scheduleService;
    private readonly IEntrvoService _entrvoService;
    private readonly ILogger<UploadController> _logger;

    public UploadController(ApplicationDbContext context,
                            IWebHostEnvironment hostingEnvironment,
                            IEntrvoService entrvoService,
                            IScheduleService scheduleService, 
                            ILogger<UploadController> logger)
    {
      _context = context;
      _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
      _scheduleService = scheduleService ?? throw new ArgumentNullException(nameof(scheduleService));
      _entrvoService = entrvoService ?? throw new ArgumentNullException(nameof(entrvoService));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IActionResult Index()
    {
      var model = new UploadModel() { FirstLineHeader = false, Delimiter = "," };
      return View(model);
    }

    public IActionResult Upload()
    {
      return PartialView();
    }

    public IActionResult Progress(Guid jobId)
    {
      if (_scheduleService.TryGetJob(jobId, out var model))
      {
        return View();
      }
      return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Start(Guid jobId, string hubConnection)
    {
      if (_scheduleService.TryGetJob(jobId, out var model))
      {
        if (model is BatchTransformJobDescriptor files)
        {
          var uploadEvent = new Event()
          {
            Type = JobType.ManualUpload,
            Message = $"Uploaded consumer file '{files.Items.First().FileName}' manually."
          };
          _context.Events.Add(uploadEvent);
          await _context.SaveChangesAsync();
        }
       
        await _scheduleService.StartJob(jobId);

        return Ok();
      }
      return NotFound();
    }

    /// <summary>
    /// upload form post back
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Process(UploadModel model)
    {
      //model.TimeStamp = DateTime.Now;
      //var response = new UploadResponse();
      //try
      //{
      //  var result = await _scheduleService.AddManualFileParsingJob(model);
      //  response.JobId = result.JobId;
      //  response.Success = true;
      //}
      //catch (Exception ex)
      //{
      //  response.Message = ex.Message;
      //}
      //return Ok(response);

      var fileName = Path.Combine(_hostingEnvironment.WebRootPath, "App_Data", model.FileName);

      await _entrvoService.Enqueue(fileName);
      return Ok();
    }

    public async Task<IActionResult> Chunk_Upload_Save(IEnumerable<IFormFile> files, string metaData)
    {
      if (metaData == null)
      {
        return await Save(files);
      }

      MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(metaData));

      JsonSerializer serializer = new JsonSerializer();
      ChunkMetaData chunkData;
      using (StreamReader streamReader = new StreamReader(ms))
      {
        chunkData = (ChunkMetaData)serializer.Deserialize(streamReader, typeof(ChunkMetaData));
      }

      string path = String.Empty;
      // The Name of the Upload component is "files"
      if (files != null)
      {
        foreach (var file in files)
        {
          path = Path.Combine(_hostingEnvironment.WebRootPath, "App_Data", chunkData.FileName);

          AppendToFile(path, file);
        }
      }

      var fileBlob = new Models.FileResult();
      fileBlob.uploaded = chunkData.TotalChunks - 1 <= chunkData.ChunkIndex;
      fileBlob.fileUid = chunkData.UploadUid;

      return Json(fileBlob);
    }

    public IActionResult Chunk_Upload_Remove(string[] fileNames)
    {
      // The parameter of the Remove action must be called "fileNames"

      if (fileNames != null)
      {
        foreach (var fullName in fileNames)
        {
          var fileName = Path.GetFileName(fullName);
          var physicalPath = Path.Combine(_hostingEnvironment.WebRootPath, "App_Data", fileName);

          // TODO: Verify user permissions

          if (System.IO.File.Exists(physicalPath))
          {
            System.IO.File.Delete(physicalPath);
          }
        }
      }

      // Return an empty string to signify success
      return Content("");
    }

    public async Task<IActionResult> Save(IEnumerable<IFormFile> files)
    {
      // The Name of the Upload component is "files"
      if (files != null)
      {
        foreach (var file in files)
        {
          var fileContent = ContentDispositionHeaderValue.Parse(file.ContentDisposition);

          // Some browsers send file names with full path.
          // We are only interested in the file name.
          var fileName = Path.GetFileName(fileContent?.FileName?.ToString().Trim('"')) ?? string.Empty;
          var physicalPath = Path.Combine(_hostingEnvironment.WebRootPath, "App_Data", fileName);

          // The files are not actually saved in this demo
          using (var fileStream = new FileStream(physicalPath, FileMode.Create))
          {
            await file.CopyToAsync(fileStream);
          }
        }
      }

      // Return an empty string to signify success
      return Content("");
    }

    [HttpPost]
    public IActionResult Abort(string fileName)
    {
      if (!_scheduleService.IsFileInUse(fileName))
      {
        fileName = Path.GetFileName(fileName);
        fileName = Path.Combine(_hostingEnvironment.WebRootPath, "App_Data", fileName);

        if (System.IO.File.Exists(fileName))
        {
          try
          {
            System.IO.File.Delete(fileName);
            return Ok();
          }
          catch (Exception ex)
          {
            _logger.LogError(ex.ToString());
            return new OkObjectResult(ex.Message) { StatusCode = 500 };
          }
        }
        return NotFound();
      }

      return BadRequest();
    }

    private void AppendToFile(string fullPath, IFormFile content)
    {
      try
      {
        using (FileStream stream = new FileStream(fullPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
        {
          content.CopyTo(stream);
        }
      }
      catch (IOException ex)
      {
        throw ex;
      }
    }

  }
}
