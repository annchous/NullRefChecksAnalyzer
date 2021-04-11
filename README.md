# NullRefChecksAnalyzer

The extension provides analysis and code fix to find and remove checks for arguments of reference types on null values.

## Analyzer

The analyzer covers the arguments of syntax nodes such as `method`, `constructor`, and `indexer`.

The analyzer consists of subclasses-analyzers that run analysis for every possible type of expression that contains a null check of reference type parameters.

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

The code fixer works according to the algorithm for finding the "root" of the problem, which is signaled by the analyzer. It could be if-statement (in most cases), equals value clause expression, switch statement, etc. The "root" can also be just the body of the method/constructor.

Examples of covered changes provided by code fix:

Simple if-statements:
```diff
static void Main(string[] args, int c)
{
    if (c == null) { }
-   if (args == null) { }
}
```
Simple if-statements:
```diff
static void Main(string[] args, int c)
{
-   if (args != null) { }
+   if (true) { }
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
    // 1st fix by 1st reported expression
-   if (!(b != null) || !(b == "a" && b == null)) { }
+   if (!(b == "a" && b == null)) { }
    // 2nd fix
-   if (!(b == "a" && b == null)) { }
+   if (!(false)) { }
}
```
One more example with logical expressions:
```diff
static void Main(string[] args, int c)
{
    // 1st fix
-   if (a != null && a == "a") { }
+   if (a == "a") { }
}
```
'Logical not' and the simpliest pattern matching expressions:
```diff
-   if (!!!(args == null)) { }
+   if (true) { }
-   if (!(null == args)) { }
+   if (true) { }
-   if (!(args == default)) { }
+   if (true) { }
-   if (args is null) { }
+   if (false) { }
-   if (args is not null) { }
+   if (true) { }
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
* without any specified parent statement
```diff
-   args?.ToString();
+   args.ToString();
```
* in if-statement
```diff
-   if (args?.ToString() == "fff") { }
+   if (args.ToString() == "fff") { }
```
* with equals value clause expressions
```diff
-   var abc = args?.ToString();
+   var abc = args.ToString();
```
Equals value clause expressions:
```diff
-   var z = args == null;
-   var w = args != null;
+   var z = false;
+   var w = true;
```
Invocation expressions:
* in if-statement
```diff
-   if (!object.ReferenceEquals(args, null)) { }
+   if (true) { }
-   if (ReferenceEquals(null, args)) { }
+   if (false) { }
```
* without any specified parent statement
```diff
-   ReferenceEquals(args, null);
```
* with equals value clause expressions
```diff
-   var x = (!(object.ReferenceEquals(args, null))) ? 1 : 2;
+   if (true) { }
-   var y = ReferenceEquals(args, null) ? 3 : 4;
+   if (false) { }
```
Coalesce expressions:
```diff
-   var a = args ?? new object() ?? new object();
+   var a = args;
-   var b = args ??= (args ??= new string[] { "a", "b", "c" });
+   var b = args;
```
In return statements:
```diff
-   return args ?? null;
+   return args;
```
```diff
-   return args ??= null;
+   return args;
```
```diff
-   return args != null;
+   return true;
```
```diff
-   return args == null;
+   return false;
```
```diff
-   return (!(args == null));
+   return true;
```

## Not implemented

**TL:DR: pattern matching expressions are mostly not implemented**

Examples of not implemented code fixes:
```csharp
if (args is { } || true) { }
if (args is null or not { }) { }
if (args is not null or null || c == 1) { } 
```
```csharp
Func<string[], bool> f2 = (args) => args is null;
Func<string[], bool> f3 = (args) => args is { };
Func<string[], bool> f4 = (args) => args is not null;
```
```csharp
if (args == null)
{
    // some body (once told me...)
}
else
{
    // should be saved, but actually deleted along with the entire if-statement expression
    // same situation with if/else if/else statements
}
```
```csharp
var a = args switch
{
    null => 1,
    _ => 2
};
```