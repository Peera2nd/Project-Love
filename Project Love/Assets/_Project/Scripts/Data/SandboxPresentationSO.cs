using TMPro;
using UnityEngine;

namespace ProjectLove.Data
{
    [CreateAssetMenu(menuName = "Project Love/Sandbox Presentation")]
    public sealed class SandboxPresentationSO : IdentifiedDataSO
    {
        public TMP_FontAsset font;
        public string title;
        public string subtitle;
        public string dayFormat;
        public string statsFormat;
        public string characterFormat;
        public string narratorLabel;
        public string readyText;
        public string continueLabel;
        public string nextPhaseLabel;
        public string endDayLabel;
        public string nextDayLabel;
        public string restartLabel;
        public string successLabel;
        public string failureLabel;
        public string activityFormat;
        public string unavailableLabel;
        public string dayEndedFormat;
        public string campaignEndedText;
        public string deferLabel;
        [TextArea] public string blockedText;
        public string saveLabel, loadLabel, loadAutoLabel;
        public string savedText, loadedText, saveFailedText, loadFailedText, busyText;
        public string prototypeEndedText;
        public string[] phaseNames = new string[0];
        [Min(0)] public float charactersPerSecond;
    }
}
