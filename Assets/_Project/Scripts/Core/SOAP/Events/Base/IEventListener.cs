namespace Zone8.SOAP.Events
{
    public interface IEventListener<T>
    {
        void OnEventRaised(T item);
    }
}
