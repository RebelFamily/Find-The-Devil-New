public interface IUIPanel
{
    UIPanelType GetPanelType();
    void Show();
    void Hide();
    void UpdatePanel(); // To refresh content if needed
}