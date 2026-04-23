namespace Zone8.UI.TabSystem
{
    public interface ITabManager
    {
        void AddTab(ITab tab);
        void RemoveTab(ITab tab);
        void SwitchTab(ITab tab);
        void SwitchTabByID(string id);
        ITab GetActiveTab();
    }
}
