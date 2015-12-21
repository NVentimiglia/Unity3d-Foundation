// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------


namespace System.ComponentModel.DataAnnotations
{
#if !UNITY_WSA || UNITY_EDITOR
    /// <summary>
    /// Unity3d Comparability
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RequiredAttribute : Attribute
    {

    }
#endif

    /// <summary>
    /// Unity3d Comparability
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MinLength : Attribute
    {
        public int Length { get; set; }

        public MinLength(int length)
        {
            Length = length;
        }
    }

    /// <summary>
    /// Unity3d Comparability
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MaxLength : Attribute
    {
        public int Length { get; set; }

        public MaxLength(int length)
        {
            Length = length;
        }
    }

    /// <summary>
    /// Unity3d Comparability
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EmailAddressAttribute : Attribute
    {

    }

    /// <summary>
    /// Unity3d Comparability
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {

    }
}
namespace System.ComponentModel.DataAnnotations.Schema
{
    /// <summary>
    /// Unity3d Comparability
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string TypeName { get; set; }
    }
}