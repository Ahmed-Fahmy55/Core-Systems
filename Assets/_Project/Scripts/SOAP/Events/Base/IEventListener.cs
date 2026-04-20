namespace Zone8.SOAP.Events
{
    public interface IGameEventListener<T>
    {
        void OnEventRaised(T item);
    }
}
