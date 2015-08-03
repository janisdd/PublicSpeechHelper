using System.Speech.Recognition;

namespace PublicSpeechHelper.SpeechApi
{
    public class SpeechRecognitionArgs
    {
        /// <summary>
        /// the original args from the speech api
        /// </summary>
        public readonly SpeechRecognizedEventArgs OriginalArgs;

        /// <summary>
        /// gets the recognized text
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// true: cancel all further command processing, false (default): look for possible commands to execute 
        /// </summary>
        public bool CancelFurtherCommands { get; set; }

        public SpeechRecognitionArgs(string text, SpeechRecognizedEventArgs originalArgs)
        {
            Text = text;
            this.OriginalArgs = originalArgs;
            this.CancelFurtherCommands = false;
        }
    }
}
