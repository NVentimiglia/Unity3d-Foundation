using Foundation.Tasks;
using UnityEngine;

namespace Assets.Foundation.Example
{
    [AddComponentMenu("Foundation/Examples/TaskTests2")]
    public class TaskTests2 : MonoBehaviour {

        [ContextMenu("Work0")]
        public void Work0()
        {
            // works, animation continues
            UnityTask.Run(RealWork).ContinueWith(t => Debug.Log("Done!"));
        }
        [ContextMenu("Work1")]
        public void Work1()
        {
            // does not work. animation freezes
            UnityTask<bool>.Run<bool, bool>(RealWork, false).ContinueWith(t => Debug.Log("Done! " + t.Result));
        }
        private bool RealWork(bool test)
        {
            RealWork();
            return test;
        }
        private void RealWork()
        {
            const int count = 5;
            for (var y = 0; y < count; y++)
            {
                UnityTask.Delay(1000);
                Debug.Log("Delay "+y);
            }
        }
    }
}
