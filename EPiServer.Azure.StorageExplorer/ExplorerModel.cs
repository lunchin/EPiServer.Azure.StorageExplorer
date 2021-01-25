using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Azure.StorageExplorer
{
    public class ExplorerModel
    {
        [Required(ErrorMessage = "Please select file.")]
        [Display(Name = "Browse File")]
        public IFormFile[] Files { get; set; }

        public List<AzureBlob> Results { get; set; }

        public int Page { get; set; }
        public int PageSize { get; set; }
        public string Container { get; set; }
        public string Path { get; set; }

        public Dictionary<string,string> BreadCrumbs { get; set; }
    }
}
