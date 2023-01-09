using EPiServer.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Wangkanai.Detection.Models;
using Wangkanai.Detection.Services;

namespace EPiServer.Azure.StorageExplorer.Sample.Business.Channels
{
    //<summary>
    //Defines the 'Mobile' content channel
    //</summary>
    public class MobileChannel : DisplayChannel
    {
        public const string Name = "mobile";

        public override string ChannelName
        {
            get
            {
                return Name;
            }
        }

        public override string ResolutionId
        {
            get
            {
                return typeof(IphoneVerticalResolution).FullName;
            }
        }

        //CMS-16684: ASPNET Core doesn't natively support checking device, we need to reimplement this
        public override bool IsActive(HttpContext context)
        {
            var detection = context.RequestServices.GetRequiredService<IDetectionService>();
            return detection.Device.Type == Device.Mobile;
        }
}
}
