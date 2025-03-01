using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Web;
using T2Importer.DAL;
using T2WebApplication.Models;
using T2WebApplication.Services;

namespace T2WebApplication.Controllers
{
  public abstract class EntityController<T> : Controller where T : class, IDataEntity
  {
    protected readonly IWebHostEnvironment _hostingEnvironment;
    protected readonly ILogger _logger;
    protected readonly IImportingService _importService;

    public Type EntityType => typeof(T);

    public EntityController(IWebHostEnvironment hostingEnvironment, ILogger logger, IImportingService importService)
    {
      _hostingEnvironment = hostingEnvironment;
      _logger = logger;
      _importService = importService ?? throw new ArgumentNullException(nameof(importService));
    }


    #region Import
    public IActionResult Import()
    {
      if (_importService.TryGetJob<T>(out var jobDescriptor) && jobDescriptor != null)
      {
        TempData["ActivePage"] = $"Import|{EntityType.Name}s";
        return RedirectToAction("index", "progress", new { jobId = jobDescriptor.Id });
      }
      else
      {
        var model = new ImportSourceModel()
        {
          FirstLineHeader = true,
          Delimiter = ","
        };
        return View(model);
      }
    }

    public IActionResult Upload()
    {
      return PartialView();
    }


    public async Task<IActionResult> Mappings(string fileName, string separator, bool headers)
    {
      var path = Path.Combine(_hostingEnvironment.WebRootPath, "App_Data", fileName);
      if (System.IO.File.Exists(path))
      {
        if (separator.StartsWith("%")) separator = HttpUtility.UrlDecode(separator);

        var properties = EntityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var mappings = properties.Where(p => p.CanWrite && p.Name != "timestamp").Select(p => new SelectListItem
        {
          Text = p.Name,
          Value = p.Name,
        }).ToList();
        mappings.Insert(0, new SelectListItem { Text = "-NotUsed-", Value = "-NotUsed-" });
        ViewBag.Mappings = mappings;

        using (var reader = new StreamReader(path))
        {
          string[] _headers = null;
          string[] values;

          var line = await reader.ReadLineAsync();
          values = line.RemoveQuoteAndSplit(separator).ToArray();

          if (headers)
          {
            _headers = values;
            var line2 = await reader.ReadLineAsync();
            values = line2.RemoveQuoteAndSplit(separator).ToArray();
          }
          else
          {
            _headers = new string[values.Length];
          }

          var columns = new List<ColumnMappingModel>();
          for (var i = 0; i < values.Length; i++)
          {
            var column = new ColumnMappingModel()
            {
              Index = i + 1,
              Value = values[i],
              Header = _headers[i],
              Target = "-NotUsed-"
            };

            var name = column.Header.Replace(" ", string.Empty);
            var property = properties.FirstOrDefault(x => x.Name == name && x.CanWrite);
            if (property != null)
            {
              column.Target = property.Name;
              if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
              {
                column.Format = @"M/d/yyyy h:m:s tt";
              }
            }

            columns.Add(column);
          }


          return PartialView("mappings", columns);
        }
      }
      return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Process(ImportSourceModel model)
    {
      if (model == null) return BadRequest();

      model.TargetType = EntityType;

      var response = new ImportJobResponse();
      try
      {
        var result = await _importService.AddJob(model);
        response.JobId = result.Id;
        response.Success = true;
      }
      catch (Exception ex)
      {
        response.Message = ex.Message;
      }
      return Ok(response);
    }

    public virtual IActionResult Progress(Guid jobId)
    {
      TempData["ActivePage"] =$"Import|{EntityType.Name}s";
      return RedirectToAction("index", "progress", new { jobId });
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

      string tempFile = Path.Combine(_hostingEnvironment.WebRootPath, "App_Data", chunkData.UploadUid);
      string finalFile = Path.Combine(_hostingEnvironment.WebRootPath, "App_Data", chunkData.FileName);

      // The Name of the Upload component is "files"
      if (files != null)
      {
        foreach (var file in files)
        {
          AppendToFile(tempFile, file);
        }
      }

      var fileBlob = new Models.FileResult();
      fileBlob.uploaded = chunkData.TotalChunks - 1 <= chunkData.ChunkIndex;
      fileBlob.fileUid = chunkData.UploadUid;

      if (fileBlob.uploaded)
      {
        if (System.IO.File.Exists(finalFile))
        {
          System.IO.File.Delete(finalFile);
        }
        System.IO.File.Move(tempFile, finalFile);
      }

      return Json(fileBlob);
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
          var fileName = Path.GetFileName(fileContent.FileName.ToString().Trim('"'));
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
      if (!_importService.IsFileInUse(fileName))
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

    #endregion


  }
}
