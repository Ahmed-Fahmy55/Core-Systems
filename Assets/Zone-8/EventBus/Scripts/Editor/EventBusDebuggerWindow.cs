#if UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using UnityEditor;

namespace Bltzo.Events.Editor
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
                    var bindings = getBindingsMethod.Invoke(null, null) as IEnumerable<IEventBinding<IEvent>>;
                    if (bindings == null)
                    {
                        UnityEngine.Debug.LogWarning($"No bindings found for EventBus<{eventType.Name}>");
                        continue;
                    }

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