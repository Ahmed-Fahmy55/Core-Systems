namespace Zone8.UI.TabSystem
{
    public interface ITab
    {
        string TabID { get; }

        void ActivateContent();
        void DeactivateContent();

        public virtual void Highlight() { }
        public virtual void Dehighlight() { }
    }
}
