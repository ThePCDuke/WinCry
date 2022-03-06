namespace WinCry.Models
{
    public interface ISelectable
    {
        bool IsChecked { get; set; }
        string Category { get; set; }
    }
}