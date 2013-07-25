using System;
using System.Linq;

internal class ClassWithStrings
{
    private static string StaticString = "HISSSSSSSSSSS!";

    private const string ConstString = "I am the law!";

    private string FieldString = "If you build it, they will come.";

    private string LocalMethod()
    {
        string localString = "I spy with my little eye, something beginning with S.";
        return localString;
    }

    private string UsesConstantString()
    {
        var local = ConstString;
        return local;
    }
}