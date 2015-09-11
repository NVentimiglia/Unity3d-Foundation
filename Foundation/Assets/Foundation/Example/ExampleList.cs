// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Foundation.Databinding;
using UnityEngine;

namespace Assets.Foundation.Example
{
    /// <summary>
    ///     Demonstration of a child view model
    /// </summary>
    [AddComponentMenu("Foundation/Examples/ExampleScore")]
    public class ExampleScore : ObservableObject
    {
        [SerializeField] private bool _isSelf;

        [SerializeField] private int _score;

        [SerializeField] public string Username;

        public int Score
        {
            get { return _score; }
            set
            {
                if (_score == value)
                    return;
                _score = value;
                NotifyProperty("Score", value);
            }
        }

        public bool IsSelf
        {
            get { return _isSelf; }
            set
            {
                if (_isSelf == value)
                    return;
                _isSelf = value;
                NotifyProperty("IsSelf", value);
            }
        }
    }

    /// <summary>
    ///     Example High Score Menu
    /// </summary>
    [AddComponentMenu("Foundation/Databinding/Example/ExampleList")]
    public class ExampleList : ObservableBehaviour
    {
        /// <summary>
        ///     Protip : Use IOC
        /// </summary>
        public ExampleOptions Options;

        private void OnEnable()
        {
            MyScore = new ExampleScore
            {
                IsSelf = true,
                Score = UnityEngine.Random.Range(100, 1000),
                Username = Options.UserName
            };

            HighScores.Add(MyScore);

            StartCoroutine(NewScoreAsync());
        }

        private void OnDisable()
        {
            HighScores.Clear();
            StopCoroutine(NewScoreAsync());
        }

        private IEnumerator NewScoreAsync()
        {
            for (var i = 0; i < 100; i++)
            {
                if (!enabled)
                    yield break;

                var score = new ExampleScore
                {
                    IsSelf = false,
                    Score = UnityEngine.Random.Range(100, 1000),
                    Username = Random(DemoNames)
                };

                HighScores.Add(score);


                yield return new WaitForSeconds(1);
            }
        }

        private T Random<T>(IEnumerable<T> list)
        {
            var count = list.Count();

            if (count == 0)
                return default(T);

            return list.ElementAt(UnityEngine.Random.Range(0, count));
        }

        #region view logic

        private bool _isVisible;

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible == value)
                    return;
                _isVisible = value;
                NotifyProperty("IsVisible", value);
                enabled = value;
            }
        }

        public void Close()
        {
            IsVisible = false;
        }

        #endregion

        #region model

        public string[] DemoNames =
        {
            "Beto",
            "Mac",
            "Velly",
            "Psylon",
            "Yoda",
            "Quark",
            "Torak",
            "Azreal",
            "Ishtar",
            "Itty"
        };

        private ExampleScore _myScore;

        public ExampleScore MyScore
        {
            get { return _myScore; }
            set
            {
                if (_myScore == value)
                    return;
                _myScore = value;
                NotifyProperty("MyScore", value);
            }
        }

        public ObservableCollection<ExampleScore> HighScores = new ObservableCollection<ExampleScore>();

        #endregion
    }
}