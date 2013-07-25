using System;
using System.Linq;

public class ClassWithStrings
{
    public static string StaticString = "HISSSSSSSSSSS!";

    public const string ConstString = "I am the law!";

    public string FieldString = "If you build it, they will come.";

    public string LocalMethod()
    {
        string localString = "I spy with my little eye, something beginning with S.";
        return localString;
    }

    public string UsesConstantString()
    {
        var local = ConstString;
        return local;
    }
}