# NullRefChecksAnalyzer

The extension provides analysis and code fix to find and remove checks for arguments of reference types on null values.

## Analyzer

The analyzer covers the arguments of syntax nodes such as `method`, `constructor`, and `indexer`.

Examples of covered checks:

```csharp
class Program
    {
        public bool this[string ss]
        {
            get
            {
                return ss == null; // warning
            }
        }

        public Program(string a, string b)
        {
            if (b == null || !(a == null) && b == "a") { } // warning on the 1st and 2nd expression
            if (a != null || a == "a") { } // warning on the 1st expression
            string[] args = new string[] { };
            if (args == null) { } // no warning
        }
        
        static void Main(string[] args, int c)
        {
            if (c == null) { } // no warning because of non-reference parameter type
            if (args == null) { } // warning
            if (!(args == null)) { } // warning
            else if (c == 1) { } // no warning
            if (!(null == args)) { } // warning
            if (!(args == default)) { } // warning
            if (args is null) { } // warning
            if (args is { }) { } // warning
            if (args is null or { }) { } // warning
            if (args is not null or null || c == 1) { } // warning on the 1st expression
            if (args.Equals(null)) { } // warning
            if (args.Equals(default)) { } // warning
            if (ReferenceEquals(args, null)) { } // warning
            if (ReferenceEquals(null, args)) { } // warning

            var z = args == null; // warning
            var w = args != null; // warning

            Func<string[], bool> f1 = (args) => args == null; // warning
            Func<string[], bool> f2 = (args) => args is null; // warning
            Func<string[], bool> f3 = (args) => args is { }; // warning
            Func<string[], bool> f4 = (args) => args is not null; // warning

            args?.ToString(); // warning
            var x = args == null ? 1 : 2; // warning
            var y = args is null ? 3 : 4; // warning

            var a = args ?? new object(); // warning
            var b = args ??= new string[] { "a", "b", "c" }; // warning

            switch (args)
            {
                case null: // warning
                    break;
                default:
                    break;
            }
        }
    }
```

## Code fix

Examples of covered changes provided by code fix:

Simple if-statement:
```diff
static void Main(string[] args, int c)
{
    if (c == null) { }
-   if (args == null) { }
}
```
Composite if-statement:
```diff
static void Main(string[] args, int c)
{
-   if (args == null || c == 1)
+   if (c == 1) { }
}
```
```diff
static void Main(string[] args, int c)
{
    // 1st fix
-   if (!(args == null) || args == null && c == 1)
+   if (args != null && c == 1) { }
    // 2nd fix
-   if (args == null && c == 1)
+   if (c == 1) { }
}
```
Switch statement:
```diff
switch (args)
{
-    case null:
-        break;
     default:
         break;
}
```
Conditional access expression:
```diff
-   args?.ToString();
+   args.ToString();
```
Equals value clause expressions:
```diff
-   var z = args == null;
-   var w = args != null;
+   var z = false;
+   var w = true;
```