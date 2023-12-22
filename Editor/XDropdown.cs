using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using UnityEngine;

public class XDropdown : BaseField<string> {
    public new class UxmlFactory : UxmlFactory<XDropdown, UxmlTraits> { }

    static StyleSheet _myStyle;
    static StyleSheet myStyle {
        get {
            if (_myStyle == null)
                _myStyle = Resources.Load<StyleSheet>("XDropdown");
            return _myStyle;
        }
    }
    readonly static Func<string, string> defaultFormatCallback = s => s;
    Func<string, string> _formatCallback;
    public Func<string, string> formatCallback {
        get {
            _formatCallback ??= defaultFormatCallback;
            return _formatCallback;
        }
        set {
            _formatCallback = value ?? defaultFormatCallback;
        }
    }

    VisualElement box;
    Label valueCaption;
    Label triangle;

    public new static readonly string ussClassName              = "x-dropdown";
    public new static readonly string inputUssClassName         = "x-dropdown__input";
    public     static readonly string valueCaptionUssClassName  = "x-dropdown__value-caption";
    public     static readonly string inputTriangleUssClassName = "x-dropdown__triangle";

    public XDropdown() : this(null) { }
    public XDropdown(string label) : base(label, null) {
        styleSheets.Add(myStyle);
        choices = new();
        choiceList = new();

        AddToClassList(ussClassName);
        box = this.Q(className: BaseField<bool>.inputUssClassName);
        box.AddToClassList(inputUssClassName);
        Add(box);

        valueCaption = new("placeholder");
        valueCaption.AddToClassList(valueCaptionUssClassName);
        // Stop bubbling up change event that mess up
        valueCaption.RegisterValueChangedCallback(ev => ev.StopPropagation());
        triangle = new("▼");
        triangle.AddToClassList(inputTriangleUssClassName);

        box.Add(valueCaption);
        box.Add(triangle);
        box.AddManipulator(new Clickable(ShowMenu));
        RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
    }

    GenericMenu _menu;
    GenericMenu menu {
        get {
            _menu ??= new();
            return _menu;
        }
        set => _menu = value;
    }

    public enum EntryType {
        normal, header, separator
    }
    struct ChoiceEntry : IEquatable<ChoiceEntry> {
        public EntryType type;
        public string path;
        public bool Equals(ChoiceEntry other) {
            return type == other.type && path == other.path;
        }
        public override string ToString() {
            return $"type = {type}, path = \"{path}\"";
        }
    }

    Dictionary<string, bool> choices;
    List<ChoiceEntry> choiceList;
    // bool hasChoice = false;

    public void AddItem(params string[] paths) {
        foreach (var path in paths) {
            var choice = new ChoiceEntry() { type = EntryType.normal, path = path };
            if (!choices.TryAdd(path, false)) continue;
            choiceList.Add(choice);
            DrawItem(choice, false);
            // hasChoice = true;
        }
    }
    public void AddHeader(string path) {
        var entry = new ChoiceEntry() { type = EntryType.header, path = path };
        choiceList.Add(entry);
        DrawHeader(entry);
    }
    public void AddSeparator(string path) {
        choiceList.Add(new ChoiceEntry() { type = EntryType.separator, path = path });
        menu.AddSeparator(path);
    }
    public void ClearEntries() {
        choices.Clear();
        choiceList.Clear();
        menu = null;
        // hasChoice = false;
    }
    public void ShowMenu() {
        menu.DropDown(worldBound);
    }
    void OnNavigationSubmit(NavigationSubmitEvent evt) {
        ShowMenu();
        evt.StopPropagation();
    }

    void MenuClick(object data) {
        if (data == null) return;
        var path = (string)data;
        value = path;
    }
    bool OnPickValue(string currentPath) {
        if (!choices.TryGetValue(currentPath, out var chosen)) return false;
        if (chosen) return false;
        menu = null;
        foreach (var entry in choiceList) {
            switch (entry.type) {
                case EntryType.normal: {
                    var on = currentPath == entry.path;
                    choices[entry.path] = on;
                    DrawItem(entry, on);
                    break;
                }
                case EntryType.header:    DrawHeader(entry);    break;
                case EntryType.separator: DrawSeparator(entry); break;
                default: break;
            }
        }
        return true;
    }
    public override void SetValueWithoutNotify(string newPath) {
        base.SetValueWithoutNotify(newPath);
        OnPickValue(newPath);
    }

    void DrawItem(ChoiceEntry entry, bool on) {
        menu.AddItem(new GUIContent(entry.path), on, MenuClick, entry.path);
        if (on) {
            valueCaption.text = formatCallback(entry.path);
        }
    }
    void DrawHeader(ChoiceEntry entry) {
        menu.AddDisabledItem(new GUIContent(entry.path));
        // menu.AddSeparator(entry.path.TruncateFrom('/'));
    }
    void DrawSeparator(ChoiceEntry entry) {
        menu.AddSeparator(entry.path);
    }
    // bool isCommitted = false;
    // public void CommitChoice() {
    //     //
    // }

    // public new class UxmlTraits : BaseFieldTraits<string, UxmlStringAttributeDescription> { }
    public new class UxmlTraits : BaseField<string>.UxmlTraits {
        // UxmlIntAttributeDescription AttrIndex = new UxmlIntAttributeDescription { name = "index" };
        // UxmlStringAttributeDescription AttrChoices = new() { name = "choices" };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
            base.Init(ve, bag, cc);
            // var el = (XDropdown)ve;
            // var choiceStr = AttrChoices.GetValueFromBag(bag, cc);
        }
    }
}
