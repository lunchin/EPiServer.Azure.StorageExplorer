using EPiServer.Framework.Localization;
using EPiServer.Security;
using EPiServer.Shell;
using EPiServer.Shell.Navigation;
using System.Collections.Generic;

namespace EPiServer.Azure.StorageExplorer
{
    [MenuProvider]
    public class MenuProvider : IMenuProvider
    {
        private readonly LocalizationService _localizationService;

        public MenuProvider(LocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public IEnumerable<MenuItem> GetMenuItems()
        {
            var url = Paths.ToResource(GetType(), "explorer");

            var storageExpolrerLink = new UrlMenuItem(
                _localizationService.GetString("/storageexplorer/menuitem"),
                MenuPaths.Global + "/Cms/storageexplorer",
                url)
            {
                SortIndex = 15,
                Alignment = MenuItemAlignment.Left,
                IsAvailable = _ => HasAccessToStorageExpolrerUI
            };

            var menus = new List<MenuItem>();
            if (HasAccessToStorageExpolrerUI)
            {
                menus.Add(storageExpolrerLink);
            }
            return menus;
        }

        private bool HasAccessToStorageExpolrerUI
        {
            get
            {
                return PrincipalInfo.Current.Principal.IsInRole("CmsAdmins");

            }
        }
    }
}
