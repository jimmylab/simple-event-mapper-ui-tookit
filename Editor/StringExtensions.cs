public static class StringExtensions {
    public static string LastTokenOf(this string str, string needle) {
        if (string.IsNullOrEmpty(str))
            return str;
        var index = str.LastIndexOf(needle) + 1;
        return str[index..];
    }
    public static string LastTokenOf(this string str, char needle) {
        if (string.IsNullOrEmpty(str))
            return str;
        var index = str.LastIndexOf(needle) + 1;
        return str[index..];
    }
    public static string TruncateFrom(this string str, string needle) {
        if (string.IsNullOrEmpty(str))
            return str;
        var index = str.LastIndexOf(needle) + 1;
        return str[..index];
    }
    public static string TruncateFrom(this string str, char needle) {
        if (string.IsNullOrEmpty(str))
            return str;
        var index = str.LastIndexOf(needle) + 1;
        return str[..index];
    }
    public static string TruncateAfter(this string str, string needle) {
        if (string.IsNullOrEmpty(str))
            return str;
        var index = str.LastIndexOf(needle);
        return index < 0 ? str : str[..index];
    }
    public static string TruncateAfter(this string str, char needle) {
        if (string.IsNullOrEmpty(str))
            return str;
        var index = str.LastIndexOf(needle);
        return index < 0 ? str : str[..index];
    }
}
