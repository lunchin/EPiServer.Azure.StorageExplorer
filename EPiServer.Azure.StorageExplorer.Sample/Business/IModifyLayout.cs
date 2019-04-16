using EPiServer.Azure.StorageExplorer.Sample.Models.ViewModels;

namespace EPiServer.Azure.StorageExplorer.Sample.Business
{
    /// <summary>
    /// Defines a method which may be invoked by PageContextActionFilter allowing controllers
    /// to modify common layout properties of the view model.
    /// </summary>
    interface IModifyLayout
    {
        void ModifyLayout(LayoutModel layoutModel);
    }
}
