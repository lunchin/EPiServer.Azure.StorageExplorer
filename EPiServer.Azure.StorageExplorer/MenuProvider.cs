using EPiServer.Framework.Localization;
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
                AuthorizationPolicy = "episerver:storage"
            };

            return new List<MenuItem>
            {
                storageExpolrerLink
            };
        }
    }
}
