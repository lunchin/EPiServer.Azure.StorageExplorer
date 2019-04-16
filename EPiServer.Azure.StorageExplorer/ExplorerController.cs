using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPiServer.Azure.StorageExplorer
{
    public class ExplorerController : Controller
    {
        private readonly IStorageService _storageService;

        public ExplorerController() : this(ServiceLocator.Current.GetInstance<IStorageService>())
        {

        }

        public ExplorerController(IStorageService storageService)
        {
            _storageService = storageService;
        }


        [Authorize(Roles = "CmsAdmins")]
        [HttpGet]
        public ActionResult Index(string container = null, string path = null, int page = 1, int pageSize = 100)
        {
            if (!_storageService.IsInitialized)
            {
                return View("Error");
            }

            var blobs = new List<AzureBlob>();
            if (container == null)
            {
                return View(new ExplorerModel
                {
                    Results = _storageService.GetContainers().Select(x => x.GetAzureBlob()).ToList(),
                    Page = page,
                    PageSize = pageSize,
                    Container = container,
                    Path = path,
                    BreadCrumbs = GetBreadCrumbs(container, path)
                });
            }

            return View(new ExplorerModel
            {
                Results = _storageService.GetBlobItems(container, path, (page -1) * pageSize, pageSize).Select(x => x.GetAzureBlob()).ToList(),
                Page = page,
                PageSize = pageSize,
                Container = container,
                Path = path,
                BreadCrumbs = GetBreadCrumbs(container, path)
            });
        }

        [Authorize(Roles = "CmsAdmins")]
        [HttpGet]
        public ActionResult Delete(string url, string container = null, string path = null, int page = 1, int pageSize = 100)
        {
            _storageService.Delete(url);
            return RedirectToAction("Index", new { path, page, container, pageSize});
        }

        [Authorize(Roles = "CmsAdmins")]
        [HttpPost]
        public ActionResult Rename(string url, string newName, string container = null, string path = null, int page = 1, int pageSize = 100)
        {
            var reference = _storageService.GetCloudBlockBlob(url);
            if (reference == null)
            {
                return new EmptyResult();
            }
            _storageService.Rename(reference, newName);
            return RedirectToAction("Index", new { path, page, container, pageSize });
        }

        [Authorize(Roles = "CmsAdmins")]
        [HttpGet]
        public ActionResult Download(string url)
        {
            var reference = _storageService.GetCloudBlockBlob(url);
            if (reference == null)
            {
                return new EmptyResult();
            }

            var memStream = new byte[reference.Properties.Length];

            reference.DownloadToByteArray(memStream, 0);
            string filename = reference.Name.Substring(reference.Name.LastIndexOf('/') == 0 ? 0 : reference.Name.LastIndexOf('/') + 1);
            string contentType = MimeMapping.GetMimeMapping(url);

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = filename,
                Inline = true,
            };

            Response.AppendHeader("Content-Disposition", cd.ToString());

            return File(memStream, contentType);
        }

        [Authorize(Roles = "CmsAdmins")]
        [HttpPost]
        public ActionResult UploadFiles(List<HttpPostedFileBase> postedFiles, string container, string path)
        {
            foreach (HttpPostedFileBase postedFile in postedFiles)
            {
                if (postedFile != null)
                {
                    string fileName = Path.GetFileName(postedFile.FileName);
                    byte[] bytes = new byte[postedFile.ContentLength];
                    postedFile.InputStream.Read(bytes, 0, bytes.Length);
                    var ms = new MemoryStream(bytes);
                    _storageService.Add(container, HttpUtility.HtmlDecode(path) + fileName, ms, postedFile.ContentLength);
                    GC.Collect();
                }
            }

            return RedirectToAction("Index", new { container, path });
        }

        [Authorize(Roles = "CmsAdmins")]
        [HttpPost]
        public ActionResult CreateContainer(string container)
        {
            _storageService.GetContainer(container.ToLower());
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
