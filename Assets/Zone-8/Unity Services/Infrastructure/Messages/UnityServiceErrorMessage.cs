using Zone8.Events;
using System;

namespace Zone8.UnityServices.infastructure
{
    public struct UnityServiceErrorMessage : IEvent
    {
        public enum Service
        {
            Authentication,
            Session,
        }

        public string Title;
        public string Message;
        public Service AffectedService;
        public Exception OriginalException;

        public UnityServiceErrorMessage(string title, string message, Service service, Exception originalException = null)
        {
            Title = title;
            Message = message;
            AffectedService = service;
            OriginalException = originalException;
        }
    }
}
