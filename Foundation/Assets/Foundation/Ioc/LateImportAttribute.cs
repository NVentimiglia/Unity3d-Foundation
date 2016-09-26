using System;


[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class LateImportAttribute : Attribute
{
    /// <summary>
    /// Optional lookup key
    /// </summary>
    public string InjectKey { get; private set; }

    /// <summary>
    /// Is there an optional lookup key ?
    /// </summary>
    public bool HasKey
    {
        get
        {
            return !string.IsNullOrEmpty(InjectKey);
        }
    }

    /// <summary>
    /// Uses standard type match lookup
    /// </summary>
    public LateImportAttribute()
    {

    }

    /// <summary>
    /// Uses an Optional lookup key
    /// </summary>
    /// <param name="key"></param>
    public LateImportAttribute(string key)
    {
        InjectKey = key;
    }
}