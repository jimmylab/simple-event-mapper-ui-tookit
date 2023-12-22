using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

using DomUtil;
using Object = UnityEngine.Object;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;

[Serializable]
public class EventMapperItem {
    public const string NO_FUNC = "Do Nothing";
    public enum SelectorType { Id, Class, Tag, Selector }

    public bool enabled = true;
    public bool monoSelect = true;
    public SelectorType selectBy = SelectorType.Id;
    public string selector;
    public string eventName;

    public Object targetObj;
    public string componentName;
    public string funcName = NO_FUNC;

    // Result arr for multi-select
    UQueryBuilder<VisualElement> elements;
    // Result elem for mono-select
    VisualElement element;
    public EventMapperItem() {
        // Debug.Log("new EventMapperItem");
        Reset(isConstructor: true);
    }
    void Reset(bool isConstructor = false) {
        if (!isConstructor) Debug.Log("EventMapperItem Reset");
        enabled = true;
        monoSelect = true;
        selectBy = SelectorType.Id;
        selector = "";
        eventName = "PointerDown";
        componentName = "Terrain";
        funcName = NO_FUNC;
    }
    public void Query(VisualElement context) {
        if (!enabled || string.IsNullOrEmpty(selector)) return;
        UssDescriptor descriptor;
        switch (selectBy) {
            case SelectorType.Id:       descriptor = new(id: selector);            break;
            case SelectorType.Class:    descriptor = new(@class: new[]{selector}); break;
            case SelectorType.Tag:      descriptor = new(tag: selector);           break;
            default:                    descriptor = new(selector);                break;
        }
        if (monoSelect)
            element  = descriptor.Q(context);
        else
            elements = descriptor.Query(context);
    }
    public void Bind() {
    }
    public void Unbind() {
    }
}

[Serializable]
public class ComponentMethod {
    public Object targetObj;
    public string componentName;
    public string methodName;
    // Currently supports no arg or 1 event arg
    public string[] argTypeNames;
    public MethodInfo method {
        get {
            if (targetObj is Component && targetObj != null) {
                return targetObj.GetType().GetMethod(methodName, argType);
            }
            return null;
        }
        set {
            if (value == null) {
                methodName = null;
                argTypeNames = null;
                return;
            }
            methodName = value.Name;
            argTypeNames = value.GetParameters().Select(p => TidyTypename(p.ParameterType)).ToArray();
        }
    }
    public Type[] argType {
        get {
            if (argTypeNames == null) return new Type[0];
            return argTypeNames.Select(name => Type.GetType(name)).ToArray();
        }
    }
    public bool isSetter;
    public bool isStatic;
    public bool acceptsEventArg;

    static Regex PATTERN = new(", Version=.*|, Culture=.*|, PublicKeyToken=.*|(?=, UnityEngine\\.)\\w+Module", RegexOptions.Compiled);
    // Equivalent to UnityEventTools.TidyAssemblyTypeName
    public static string TidyTypename(Type t) {
        return PATTERN.Replace(t.AssemblyQualifiedName, "");
    }
}

[Serializable]
class ValueWrapper {
    public enum ValueType {
        Float, Vector2, Vector3, Vector4, Color, Quaternion, Rect,
        Double, Long,
        Int, Bool, LayerMask,
        String,
    }
    [SerializeField] ValueType type;
    [SerializeField] string label;

    // For types based on float: float, Vector2, Vector3, Vector4, Color, Quaternion, Rect
    [SerializeField] float f1;
    [SerializeField] float f2;
    [SerializeField] float f3;
    [SerializeField] float f4;

    // For types based on int: bool, int, Enum, LayerMask, uint
    [SerializeField] int i;
    // For types based on string: string char
    string s;
    AnimationCurve curve;
    Gradient grad;
}
