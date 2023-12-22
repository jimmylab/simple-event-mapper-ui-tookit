using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;
using UnityEditor.UIElements;
using DomUtil;
using static DomUtil.NodeUtil;
using Object = UnityEngine.Object;
using System;

[CustomEditor(typeof(SimpleEventMapper))]
public class SimpleEventMapperEditor : Editor {
    SerializedProperty mappings;
    SerializedProperty eventContainer;
    // const string testStructsArrayKey = nameof(SimpleEventMapper.testStructs) + ".Array";
    // SerializedProperty testStructsArray {
    //     get => serializedObject.FindProperty(testStructsArrayKey);
    // }
    const string mappingsArrayKey = nameof(SimpleEventMapper.mappings) + ".Array";
    SerializedProperty mappingsArray {
        get => serializedObject.FindProperty(mappingsArrayKey);
    }
    void OnEnable() {
        mappings = serializedObject.FindProperty(nameof(SimpleEventMapper.mappings));
        eventContainer = serializedObject.FindProperty(nameof(SimpleEventMapper.eventContainer));
    }
    [SerializeField] VisualTreeAsset asset;
    [SerializeField] VisualTreeAsset itemAsset;
    static class USSClassNames {
        const string delim = "__";
        public const string root = "event-list";
        public const string overlay       = root + delim + "overlay";
        public const string itemWrapper   = root + delim + "wrapper";
        public const string item = "event-item";
        public const string itemControls  = item + delim + "controls";
        public const string reorderHandle = "reorder-handler";
        public const string itemDisabled = item + delim + "disabled";
        public const string btnAdd    = "unity-list-view__add-button";
        public const string btnRemove = "unity-list-view__remove-button";
        public const string eventName = item + delim + "event-name";
        public const string targetObj    = "targetObj";
        public const string funcDropdown  = "func-dropdown";
    }
    VisualElement listContainer;
    class ListReorderManipulator : PointerManipulator {
        public ListReorderManipulator() {
            activators.Add(new ManipulatorActivationFilter() {button = MouseButton.LeftMouse});
        }
        VisualElement wrapper;
        VisualElement overlay;
        public int pointerId;
        protected override void RegisterCallbacksOnTarget() {
            target.On<PointerDownEvent>(OnStartDrag);
            overlay = target.Q(USSClassNames.overlay);
            // Debug.Log(overlay);
            wrapper = target.Q(USSClassNames.itemWrapper);
        }
        protected override void UnregisterCallbacksFromTarget() {
            OnRelease();
            target.Off<PointerDownEvent>(OnStartDrag);
        }
        public Vector3 cursorStartPos;
        public Vector3 cursorPos;
        public Vector3 startPos;
        Vector3 delta {
            get => cursorPos - cursorStartPos;
        }
        VisualElement currentItem;
        int indexBefore = -1;
        int indexAfter = -1;
        float maxHeight = float.PositiveInfinity;
        void OnStartDrag(PointerDownEvent ev) {
            var el = (VisualElement)ev.target;
            if (el.name != USSClassNames.reorderHandle) return;

            currentItem = el.Closest(name: USSClassNames.item);
            pointerId = ev.pointerId;
            PointerCaptureHelper.CapturePointer(target, pointerId);
            cursorStartPos = ev.position;
            target.On<PointerMoveEvent>(OnMove);
            target.On<PointerUpEvent>(OnRelease);
            maxHeight = target.layout.height - currentItem.layout.height;
            indexBefore = wrapper.IndexOf(currentItem);
            FillOverlay();
        }
        void OnMove(PointerMoveEvent ev) {
            cursorPos = ev.position;
            overlay.transform.position =
                Mathf.Clamp(startPos.y + delta.y, 0, maxHeight) * Vector3.up;
            FindClosestItem();
        }
        void FindClosestItem() {
            var overlayRect = overlay.worldBound;
            VisualElement selected = null;
            int index = 0;
            for (int i = 0; i < wrapper.childCount; i++) {
                var item = wrapper[i];
                if (item.worldBound.Overlaps(overlayRect)) {
                    if (selected == null) {
                        index = i;
                        selected = item;
                    } else if (IsElemNearer(overlayRect, selected, item)) {
                        index = i;
                        selected = item;
                        break;
                    }
                }
            }
            if (selected != null && selected != currentItem) {
                wrapper.Insert(index, currentItem);
                indexAfter = index;
            }
        }
        public delegate void IndexChangeCallback(int before, int after);
        public event IndexChangeCallback OnIndexChange;
        void OnRelease(PointerUpEvent ev = null) {
            PointerCaptureHelper.ReleasePointer(target, pointerId);
            target.Off<PointerMoveEvent>(OnMove);
            target.Off<PointerUpEvent>(OnRelease);
            RestoreItem();
            if (indexAfter < 0 || indexAfter == indexBefore) return;
            OnIndexChange?.Invoke(indexBefore, indexAfter);
        }
        void FillOverlay() {
            var rect = currentItem.layout;
            // make overlay same as target
            startPos = new Vector3(rect.x, rect.y);
            overlay.transform.position = startPos;
            overlay.style.height = rect.height;
            // move children to overlay
            overlay.AddElements(currentItem.Children());
            // keep blanked target height
            currentItem.style.height = rect.height;
            overlay.style.display = DisplayStyle.Flex;
        }
        void RestoreItem() {
            if (currentItem != null) {
                currentItem.style.height = Length.Auto();
                currentItem.AddElements(overlay.Children());
            }
            currentItem = null;

            overlay.transform.position = Vector3.zero;
            overlay.style.display = DisplayStyle.None;
        }
        static bool IsElemNearer(Rect rect, VisualElement A, VisualElement B) {
            var O = rect.center;
            var OA = (A.worldBound.center - O).magnitude;
            var OB = (B.worldBound.center - O).magnitude;
            return OB <= OA;
        }
    }
    public override VisualElement CreateInspectorGUI() {
        // Create a new VisualElement to be the root of our inspector UI
        asset ??= Resources.Load<VisualTreeAsset>("SimpleEventMapper");
        itemAsset ??= Resources.Load<VisualTreeAsset>("SimpleEventMapperItem");
        var inspector = asset.CloneTree();

        var list = inspector.Q(USSClassNames.root);

        listContainer = inspector.Q(USSClassNames.itemWrapper);
        RepaintList();
        var arrProp = mappingsArray;
        arrProp.Next(true);
        list.TrackPropertyValue(arrProp, RepaintList);
        inspector.Q<Button>(USSClassNames.btnAdd).clicked += () => {
            mappingsArray.InsertArrayElementAtIndex(mappings.arraySize);
            Debug.Log("arraySize = " + mappings.arraySize);
            var origin = target as SimpleEventMapper;
            origin.mappings.Add(new ());
        };
        inspector.Q<Button>(USSClassNames.btnRemove).clicked += () => {
            var size = mappings.arraySize;
            if (size > 0) {
                Debug.Log($"Item {mappings.arraySize - 1} deleted");
                // mappingsArray.DeleteArrayElementAtIndex(mappings.arraySize - 1);
                mappingsArray.arraySize -= 1;
                serializedObject.ApplyModifiedProperties();
                Debug.Log($"Now has {mappings.arraySize} items");
            }
        };
        inspector.Add(new PropertyField(eventContainer));
        InspectorElement.FillDefaultInspector(
            inspector.Q<Foldout>("default-inspector"),
            serializedObject, this
        );
        list.AddManipulator(new ListReorderManipulator());
        return inspector;
    }

    BindableElement BuildItem(SerializedProperty _ = null) {
        var listItem = itemAsset.CloneTree().Q<BindableElement>(USSClassNames.item);
        listItem.Q<Toggle>(nameof(EventMapperItem.enabled)).OnChange(ev => {
            listItem.EnableInClassList(USSClassNames.itemDisabled, !ev.newValue);
        });
        listItem.Q<DropdownField>(USSClassNames.eventName)
            .formatSelectedValueCallback = origStr => origStr.LastTokenOf('/');

        var dropdown = listItem.Q<XDropdown>();
        dropdown.formatCallback = s => s.Replace('/', '.');
        dropdown.AddItem(DO_NOTHING);
        dropdown.value = DO_NOTHING;
        listItem.Q<ObjectField>(USSClassNames.targetObj)
            .RegisterValueChangedCallback(ev => {
                FillDropdown(dropdown, ev.newValue);
            });
        return listItem;
    }
    const string DO_NOTHING = "Do nothing";
    void FillDropdown(XDropdown dropdown, Object o) {
        var original = dropdown.value;
        dropdown.ClearEntries();
        dropdown.AddItem(DO_NOTHING);
        dropdown.value = DO_NOTHING;
        dropdown.AddSeparator("/");
        if (o == null) return;
        foreach (var func in GetAllMenuItems(o)) {
            if (func.staticMethods.Length > 0) {
                dropdown.AddHeader($"{func.componentName}/Static");
                dropdown.AddItem(func.staticMethods);
            }
            if (func.instanceMethods.Length > 0) {
                dropdown.AddHeader($"{func.componentName}/Non-Static");
                dropdown.AddItem(func.instanceMethods);
            }
        }
        dropdown.value = original;
    }
    void RepaintList(SerializedProperty _ = null) {
        Debug.Log("RepaintList");
        var prop = mappingsArray;
        var endProperty = prop.GetEndProperty();
        prop.NextVisible(true); // Expand the first child.
        var childIndex = 0;
        // Iterate each property under the array, and populate the container with preview elements
        do {
            // Stop if you reach the end of the array
            if (SerializedProperty.EqualContents(prop, endProperty))
                break;
            // Skip the array size property
            if (prop.propertyType == SerializedPropertyType.ArraySize)
                continue;
            BindableElement element;
            // Find an existing element or create one
            if (childIndex < listContainer.childCount) {
                element = (BindableElement)listContainer[childIndex];
            } else {
                element = BuildItem(prop);
                listContainer.Add(element);
            }
            element.BindProperty(prop);

            ++childIndex;
        } while (prop.NextVisible(false));   // Never expand children.

        // Remove excess elements if the array is now smaller
        while (childIndex < listContainer.childCount) {
            listContainer.RemoveAt(listContainer.childCount - 1);
        }
    }
    struct FuncList {
        public string componentName;
        public string[] staticMethods;
        public string[] instanceMethods;
    }
    static FuncList[] GetAllMenuItems(Object o) {
        if (o is GameObject obj) {
            return GetAllMenuItems(obj);
        } else if (o is Component go) {
            return GetAllMenuItems(go.gameObject);
        } else {
            throw new System.Exception();
        }
    }
    static FuncList[] GetAllMenuItems(GameObject go) {
        var components = go.GetComponents<Component>();
        var list = (
            from comp in components
            select GetMenuItems(comp)
        ).ToArray();
        return list;
    }
    static FuncList GetMenuItems(Component o) {
        var type = o.GetType();
        var componentName = type.Name;
        var result = (
            from m in o.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            where !m.IsGenericMethod && !m.IsSpecialName && !m.IsAssembly
            where !m.Name.StartsWith("internal_")
            where m.GetParameters().Length == 0
            select m
        );
        return new FuncList() {
            componentName = componentName,
            staticMethods   = result.Where(m => m.IsStatic) .Select(StringifyMethod).ToArray(),
            instanceMethods = result.Where(m => !m.IsStatic).Select(StringifyMethod).ToArray(),
        };
        string StringifyMethod(MethodInfo m) {
            var Params = m.GetParameters();
            var paraStr = Params.Length == 0 ? "" : string.Join(", ", Params.Select(p => p.ParameterType.Name));
            var methodStr = $"{componentName}/{m.Name}({paraStr})";
            return methodStr;
        }
    }

}
