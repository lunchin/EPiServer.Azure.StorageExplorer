using EPiServer.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EPiServer.Azure.StorageExplorer
{
    public class ExplorerController : Controller
    {
        private readonly IStorageService _storageService;
        private readonly IMimeTypeResolver _mimeTypeResolver;

        public ExplorerController(IStorageService storageService, 
            IMimeTypeResolver mimeTypeResolver)
        {
            _storageService = storageService;
            _mimeTypeResolver = mimeTypeResolver;
        }


        [Authorize(Roles = "CmsAdmins")]
        [HttpGet]
        public async Task<IActionResult> Index(string container = null, string path = null, int page = 1, int pageSize = 100)
        {
            if (!_storageService.IsInitialized)
            {
                return View("Error");
            }

            var blobs = new List<AzureBlob>();
            if (container == null)
            {
                await foreach(var blob in _storageService.GetContainersAsync())
                {
                    blobs.Add(blob.GetAzureBlob());
                }
                return View(new ExplorerModel
                {
                    Results = blobs,
                    Page = page,
                    PageSize = pageSize,
                    Container = container,
                    Path = path,
                    BreadCrumbs = GetBreadCrumbs(container, path)
                });
            }

            var containerClient = await _storageService.GetContainerAsync(container);
            await foreach (var blob in _storageService.GetBlobItemsAsync(container, path))
            {
                blobs.Add(blob.GetAzureBlob(containerClient));
            }


            return View(new ExplorerModel
            {
                Results = blobs,
                Page = page,
                PageSize = pageSize,
                Container = container,
                Path = path,
                BreadCrumbs = GetBreadCrumbs(container, path)
            });
        }

        [Authorize(Roles = "CmsAdmins")]
        [HttpGet]
        public async Task<IActionResult> Delete(string url, string container = null, string path = null, int page = 1, int pageSize = 100)
        {
            await _storageService.DeleteAsync(url);
            return RedirectToAction("Index", new { path, page, container, pageSize});
        }

        [Authorize(Roles = "CmsAdmins")]
        [HttpPost]
        public async Task<IActionResult> Rename(string url, string newName, string container = null, string path = null, int page = 1, int pageSize = 100)
        {
            var reference = await _storageService.GetCloudBlockBlobAsync(url);
            if (reference == null)
            {
                return new EmptyResult();
            }
            
            await _storageService.RenameAsync(reference, newName);
            return RedirectToAction("Index", new { path, page, container, pageSize });
        }

        [Authorize(Roles = "CmsAdmins")]
        [HttpGet]
        public async Task<IActionResult> Download(string url)
        {
            var reference = await _storageService.GetCloudBlockBlobAsync(url);
            if (reference == null)
            {
                return new EmptyResult();
            }

            var memStream = new MemoryStream();
            await reference.DownloadToAsync(memStream);
            string filename = reference.Name.Substring(reference.Name.LastIndexOf('/') == 0 ? 0 : reference.Name.LastIndexOf('/') + 1);
            string contentType = _mimeTypeResolver.GetMimeMapping(url);

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = filename,
                Inline = true,
            };

            Response.Headers.Append("Content-Disposition", cd.ToString());

            return File(memStream, contentType);
        }

        [Authorize(Roles = "CmsAdmins")]
        [HttpPost]
        public async Task<IActionResult> UploadFiles(List<IFormFile> postedFiles, string container, string path)
        {
            foreach (var postedFile in postedFiles)
            {
                if (postedFile != null)
                {
                    string fileName = Path.GetFileName(postedFile.FileName);
                    byte[] bytes = new byte[postedFile.Length];
                    postedFile.OpenReadStream().Read(bytes, 0, bytes.Length);
                    var ms = new MemoryStream(bytes);
                    await _storageService.AddAsync(container, HttpUtility.HtmlDecode(path) + fileName, ms, postedFile.Length);
                    GC.Collect();
                }
            }

            return RedirectToAction("Index", new { container, path });
        }

        [Authorize(Roles = "CmsAdmins")]
        [HttpPost]
        public async Task<ActionResult> CreateContainer(string container)
        {
            await _storageService.GetContainerAsync(container.ToLower());
            return RedirectToAction("Index");
        }

        private Dictionary<string,string> GetBreadCrumbs(string container, string path)
        {
            var breadCrumbs = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(container) && string.IsNullOrEmpty(path))
            {
                return breadCrumbs;
            }

            breadCrumbs.Add("Root", Url.Action("Index"));
            breadCrumbs.Add(container, Url.Action("Index", new { container }));

            if (string.IsNullOrEmpty(path))
            {
                return breadCrumbs;
            }

            var links = HttpUtility.HtmlDecode(path).Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i =0; i < links.Length; i++)
            {
                var prefix = string.Empty;
                
                if (i == links.Length - 1)
                {
                    breadCrumbs.Add(links[i], "");
                    continue;
                }
                else if (i == 0)
                {
                    prefix = links[i] + "/";
                    breadCrumbs.Add(links[i], Url.Action("Index", new { container, path = prefix }));
                    continue;
                }

                for (var j = 0; j < i; j++)
                {
                    prefix += links[j] + "/";
                }
                prefix += links[i] + "/";

                breadCrumbs.Add(links[i], Url.Action("Index", new { container, path = prefix }));
            }
            return breadCrumbs;
        }
    }
}
