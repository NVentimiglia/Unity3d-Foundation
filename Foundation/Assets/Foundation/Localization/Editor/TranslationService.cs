// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Foundation.Localization;
using FullSerializer;
using UnityEditor;
using UnityEngine;

namespace Assets.Foundation.Localization.Editor
{
    /// <summary>
    /// http://api.yandex.com/translate/doc/dg/concepts/api-overview.xml
    /// </summary>
    public class YandexTranslator : EditorWindow
    {
        public class TranslateResponce
        {
            public int code;
            public string lang;
            public string[] text;
        }

        public class LangOption
        {
            public LanguageInfo lang;
            public bool Translate = true;
        }

        private string _apiKey = string.Empty;
        public string ApiKey
        {
            get { return _apiKey; }
            set
            {
                if (_apiKey == value)
                    return;
                _apiKey = value;

                EditorPrefs.SetString("YandexTranslator.ApiKey", value);
            }
        }
        
        protected bool IsWorking = false;
        protected string FileName;
        protected LanguageInfo Language;
        protected int LangTotal;
        protected int LangDone;
        protected int FileTotal;
        protected int FileDone;
        protected int StringTotal;
        protected int StringDone;
        protected bool IsComplete;
        protected bool Cancel;
        protected LangOption[] Options;
        protected Vector2 Position = Vector2.zero;

        protected string RootPath = Application.dataPath;
        
        [MenuItem("Tools/Foundation/Yandex Translator")]
        public static void ShowWindow()
        {
            GetWindowWithRect<YandexTranslator>(new Rect(0, 0, 640, 450), false, "Storage");
        }
        
        void OnEnable()
        {
            LocalizationInitializer.Startup();

            _apiKey = EditorPrefs.GetString("YandexTranslator.ApiKey", string.Empty);

            //init
           var instance =  LocalizationService.Instance;

           Options = instance.Languages.Select(o => new LangOption
            {
                lang = o,
                Translate = true
            }).ToArray();
        }

        void OnGUI()
        {
            if (IsWorking)
            {
                ShowWorking();
            }
            else
            {
                ShowSetup();
            }
        }

        void ShowSetup()
        {
            GUILayout.BeginHorizontal(GUILayout.MinHeight(64));
            EditorGUILayout.LabelField("Yandex Translator", new GUIStyle
            {
                fontSize = 32,
                padding = new RectOffset(16, 0, 16, 0),
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = Color.white
                }
            });

            GUILayout.EndHorizontal();

            if (IsComplete)
            {
                EditorGUILayout.LabelField("Complete", EditorStyles.boldLabel);
            }
            //
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Yandex Api Key");
            ApiKey = EditorGUILayout.TextField(ApiKey);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Main Language");
            EditorGUILayout.LabelField(LocalizationService.Instance.Language.Name, EditorStyles.boldLabel);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Target Languages");

            Position = EditorGUILayout.BeginScrollView(Position, GUILayout.Height(200));
            foreach (var o in Options)
            {
                o.Translate = EditorGUILayout.Toggle(o.lang.Name + " " + o.lang.Abbreviation, o.Translate);
            }
            EditorGUILayout.EndScrollView();
            

            EditorGUILayout.Separator();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Translate Now"))
            {
                Begin();
            }
            EditorGUILayout.Separator();

            if (GUILayout.Button("Validate"))
            {
                Validate();
            }
            EditorGUILayout.Separator();

            if (GUILayout.Button("Documentation"))
            {
                Documentation();
            }
            GUILayout.EndHorizontal();
        }


        void Validate()
        {
            var words = LocalizationService.Instance.Strings;

            var myList = new List<string>();
            int d = 0;

            foreach (var word in words)
            {
                if (myList.Contains(word.Key))
                {
                    Debug.LogError("Duplicate found : " + word.Key);
                    d++;
                }
                else
                {
                    myList.Add(word.Key);
                }
            }

            Debug.LogWarning("Duplicates found : " + d);
        }

        void ShowWorking()
        {
            GUILayout.BeginHorizontal(GUILayout.MinHeight(64));
            EditorGUILayout.LabelField("Yandex Translator", new GUIStyle
            {
                fontSize = 32,
                padding = new RectOffset(16, 0, 16, 0),
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = Color.white
                }
            });

            GUILayout.EndHorizontal();
            //
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Working...");
            EditorGUILayout.LabelField(ApiKey, EditorStyles.boldLabel);

            if (Language == null)
            {
                return;
            }
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Current Language");
            EditorGUILayout.LabelField(Language.Name + " " + Language.Abbreviation, EditorStyles.boldLabel);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Current File");
            EditorGUILayout.LabelField(FileName, EditorStyles.boldLabel);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Languages");
            EditorGUILayout.LabelField(LangDone + " / " + LangTotal, EditorStyles.boldLabel);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Files");
            EditorGUILayout.LabelField(FileDone + " / " + FileTotal, EditorStyles.boldLabel);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Strings");
            EditorGUILayout.LabelField(StringDone + " / " + StringTotal, EditorStyles.boldLabel);

            EditorGUILayout.Separator();
            if (GUILayout.Button("Stop"))
            {
                Cancel = true;
                IsWorking = false;
            }

            Repaint();
        }

        void Begin()
        {
            Debug.Log("Being Translation");
            IsWorking = true;
            IsComplete = false;
            Cancel = false;

            EditorCoroutine.Start(TranslateAsync());
        }

        IEnumerator TranslateAsync()
        {
            var localLang = LocalizationService.Instance.Language;
            var files = LocalizationService.Instance.Files.ToArray();
            LangTotal = Options.Count(o => o.Translate);
            FileTotal = files.Length * LangTotal;
            StringTotal = LocalizationService.Instance.StringsByFile.Sum(o => o.Value.Count) * LangTotal;
            StringDone = FileDone = LangDone = 0;

            Debug.Log(StringTotal);

            foreach (var language in Options)
            {
                if (Cancel)
                    yield break;

                if (!language.Translate || language.lang.Abbreviation == localLang.Abbreviation)
                {
                    Debug.Log("Skipping language : " + language.lang.Name);
                    continue;
                }

                Debug.Log("Starting language : " + language.lang.Name);
                Language = language.lang;

                foreach (var file in files)
                {
                    if (Cancel)
                        yield break;
                    Debug.Log("Starting file : " + file);
                    FileName = file;

                    var strings = LocalizationService.Instance.GetFile(file);

                    var translated = new Dictionary<string, string>();

                    foreach (var pair in strings)
                    {
                        if (Cancel)
                            yield break;

                        var s = GetTranslation(language.lang, pair.Value);

                        //yield return s;
                        while (!s.isDone)
                        {
                            yield return 1;
                        }

                        if (!string.IsNullOrEmpty(s.error))
                        {
                            Debug.LogError("Error " + pair.Key);
                            Debug.LogError(s.error);
                            yield break;
                        }
                        translated.Add(pair.Key, Deserialize(s));
                        StringDone++;
                    }

                    WriteFile(language.lang, FileName, translated);
                    FileDone++;
                }
                LangDone++;
            }

            Debug.Log("Complete");
            IsWorking = false;
            IsComplete = true;
        }

        WWW GetTranslation(LanguageInfo lang, string s)
        {
            const string pathRaw = "https://translate.yandex.net/api/v1.5/tr.json/translate?key={0}&lang={1}-{2}&text={3}";
            var path = string.Format(pathRaw, ApiKey, LocalizationService.Instance.Language.Abbreviation, lang.Abbreviation, s.Replace(" ", "%20"));
            
            return new WWW(path);
        }

        string Deserialize(WWW www)
        {
            var r = JsonSerializer.Deserialize<TranslateResponce>(www.text);

            var t = r.text[0];

            if (t.StartsWith("\""))
                return t;

            return t.TrimEnd('"');
        }

        void WriteFile(LanguageInfo language, string fileName, Dictionary<string, string> file)
        {

            var csv = new StringBuilder();

            foreach (var f in file)
            {
                csv.AppendLine(string.Format("{0},{1}", f.Key, RemoveNewline(f.Value)));
            }

            var path = RootPath + "/Resources/Localization/" + language.Name;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var fPath = string.Format("{0}/{1}.txt", path, fileName);

            if (File.Exists(fPath))
            {
                File.Delete(fPath);
            }

            File.WriteAllText(fPath, csv.ToString());

            Debug.Log(fPath);
        }

        public static string RemoveNewline(string s)
        {
            return s.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
        }


        void Documentation()
        {
            Application.OpenURL("http://api.yandex.com/translate/doc/dg/concepts/About.xml");
        }

    }
}
