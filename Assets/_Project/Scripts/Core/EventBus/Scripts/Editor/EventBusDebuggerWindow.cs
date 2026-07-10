#if UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using UnityEditor;

namespace Zone8.Events.Editor
{
    public class EventBusDebuggerWindow : OdinEditorWindow
    {
        [ShowInInspector, TableList]
        private List<EventBusBindingInfo> eventBindings = new();

        [MenuItem("Tools/EventBus Debugger")]
        private static void OpenWindow()
        {
            GetWindow<EventBusDebuggerWindow>("EventBus Debugger").Show();
        }

        [Button("Refresh Bindings")]
        private void RefreshBindings()
        {
            eventBindings.Clear();

            // Use reflection to get all EventBus<T> types and their bindings
            var eventBusType = typeof(EventBus<>);
            foreach (var eventType in PredefinedAssemblyUtil.GetTypes(typeof(IEvent)))
            {
                var genericBusType = eventBusType.MakeGenericType(eventType);
                var getBindingsMethod = genericBusType.GetMethod("GetBindings",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

                if (getBindingsMethod != null)
                {
                    // IEventBinding<T> is invariant, so the concrete IEventBinding<SomeEvent>[]
                    // can only be enumerated through the non-generic interface.
                    if (getBindingsMethod.Invoke(null, null) is not System.Collections.IEnumerable bindings)
                        continue;

                    foreach (var binding in bindings)
                    {
                        eventBindings.Add(new EventBusBindingInfo
                        {
                            EventType = eventType.Name,
                            BindingDetails = binding.ToString() // Customize this as needed
                        });
                    }
                }
            }
        }

        public class EventBusBindingInfo
        {
            [TableColumnWidth(200)]
            public string EventType;

            [TableColumnWidth(400)]
            public string BindingDetails;
        }
    }

}
#endif