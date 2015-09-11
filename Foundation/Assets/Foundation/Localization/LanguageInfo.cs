// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------
using System;

namespace Foundation.Localization
{
    /// <summary>
    /// Name and two letter Abbreviation
    /// </summary>
    [Serializable]
    public class LanguageInfo : IEquatable<LanguageInfo>
    {
        public string Name;
        public string Abbreviation;

        public LanguageInfo()
        {

        }

        public LanguageInfo(string name, string ab)
        {
            Name = name;
            Abbreviation = ab;
        }

        public static readonly LanguageInfo English = new LanguageInfo("English", "en");
        public static readonly LanguageInfo Chinese = new LanguageInfo("Chinese", "zh");
        public static readonly LanguageInfo Dutch = new LanguageInfo("Dutch", "nl");
        public static readonly LanguageInfo French = new LanguageInfo("French", "fr");
        public static readonly LanguageInfo German = new LanguageInfo("German", "de");
        public static readonly LanguageInfo Greek = new LanguageInfo("Greek", "el");
        public static readonly LanguageInfo Italian = new LanguageInfo("Italian", "it");
        public static readonly LanguageInfo Polish = new LanguageInfo("Polish", "pl");
        public static readonly LanguageInfo Portuguese = new LanguageInfo("Portuguese", "pt");
        public static readonly LanguageInfo Russian = new LanguageInfo("Russian", "ru");
        public static readonly LanguageInfo Spanish = new LanguageInfo("Spanish", "es");
        public static readonly LanguageInfo Turkish = new LanguageInfo("Turkish", "tr");
        public static readonly LanguageInfo Ukrainian = new LanguageInfo("Ukrainian", "uk");

        public static readonly LanguageInfo[] All = {
            English,
            Chinese,
            Dutch,
            French,
            German,
            Greek,
            Italian,
            Polish,
            Portuguese,
            Russian,
            Spanish,
            Turkish,
            Ukrainian,
        };

        public bool Equals(LanguageInfo other)
        {
            return other.Name == Name;
        }
    }
}