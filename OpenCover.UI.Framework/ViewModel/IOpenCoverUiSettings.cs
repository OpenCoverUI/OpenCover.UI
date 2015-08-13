namespace OpenCover.UI.Framework.ViewModel
{
    public interface IOpenCoverUiSettings
    {
        string NUnitPath { get; set; }

        string OpenCoverPath { get; set; }

        string XUnitPath { get; set; }
    }
}