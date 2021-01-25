using System;
using EPiServer.Azure.StorageExplorer.Sample;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EPiServer.Azure.StorageExplorer.Sample.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class RegisterFirstAdminWithLocalRequestAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (AdministratorRegistrationPageMiddleware.IsEnabled == false)
            {
                context.Result = new NotFoundResult();
                return;
            }
        }
    }
}
