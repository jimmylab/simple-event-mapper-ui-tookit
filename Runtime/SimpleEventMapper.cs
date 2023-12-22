using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using System;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
[AddComponentMenu("Event/Simple Event Mapper")]
public class SimpleEventMapper : MonoBehaviour {
    public static readonly Dictionary<string, Type> AllEventTypes;
    const string EVENT_TYPE_NAMESPACE = "UnityEngine.UIElements";
    UIDocument doc;
    public List<EventMapperItem> mappings;
    public UnityEvent<PointerDownEvent> testEvent;
    public UnityEventBase eventContainer;
    static SimpleEventMapper() {
        Type BaseType = typeof(EventBase);
        AllEventTypes = (
            from EVENT in BaseType.Assembly.GetTypes()
            where EVENT.IsSubclassOf(BaseType) || EVENT == BaseType
            where EVENT.Namespace == EVENT_TYPE_NAMESPACE
            where EVENT.Name.EndsWith("Event")
            select EVENT
        ).ToDictionary(EVENT => EVENT.Name[..^5], EVENT => EVENT);
        //     select new { Key = evType.Name[..^5], Value = evType }
        // ).ToDictionary(pair => pair.Key, pair => pair.Value);
    }
    public static Type EventTypeOf(string evName) {
        return AllEventTypes.GetValueOrDefault(evName, null);
    }
    void OnEnable() {
        doc = GetComponent<UIDocument>();
        eventContainer = new UnityEvent<PointerDownEvent>();
        // foreach (var entry in mappings) {
        //     entry.Query(doc.rootVisualElement);
        //     entry.Bind();
        // }
    }
    void OnDisable() {
    }
}

[Serializable]
public class TestStruct {
    public string A = "xxx";
    public int B = 123;
    public TestStruct() {
    }
}
