using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;


// [CustomPropertyDrawer(typeof(EventMapperItem))]
public class EventMapperItemDrawer : PropertyDrawer {
    SerializedProperty property;
    SerializedProperty enabled;
    SerializedProperty monoSelect;
    SerializedProperty selectBy;
    SerializedProperty selector;
    SerializedProperty eventName;
    SerializedProperty targetObject;
    SerializedProperty componentName;
    SerializedProperty funcName;
    readonly static List<string> DefaultFuncChoice;
    public const string NO_FUNC = "Do Nothing";
    public const string DefaultEventName = "PointerDown";
    static EventMapperItemDrawer() {
        DefaultFuncChoice = new List<string>(new []{ NO_FUNC, "/" });
    }
    void InitProps() {
        enabled       = property.FindPropertyRelative("enabled");
        monoSelect    = property.FindPropertyRelative("monoSelect");
        selectBy      = property.FindPropertyRelative("selectBy");
        selector      = property.FindPropertyRelative("selector");
        eventName     = property.FindPropertyRelative("eventName");

        targetObject  = property.FindPropertyRelative("targetObject");
        componentName = property.FindPropertyRelative("componentName");
        funcName      = property.FindPropertyRelative("funcName");
    }
    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        this.property = property;
        InitProps();
        BindableElement container = new();
        // property.displayName.Replace("Element", "Binding")
        var asset = Resources.Load<VisualTreeAsset>("SimpleEventMapperItem");
        asset.CloneTree(container);
        // container.Q<DropdownField>("EventName").choices = SimpleEventMapper.AllEventNames;
        container.Bind(property.serializedObject);

        return container;
    }
}

