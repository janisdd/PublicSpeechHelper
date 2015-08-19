using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicSpeechHelper.Helpers
{
    /// <summary>
    /// encapsules the fields for the OnAfterSimpleCommandInvoked event
    /// </summary>
    public class AfterSimpleComamndInokedArgs
    {
        /// <summary>
        /// the language
        /// </summary>
        public string Lang { get; private set; }

        /// <summary>
        /// the text of the simple command
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// the speech group key of the simple command
        /// </summary>
        public string SpeechGroupKey { get; private set; }

        /// <summary>
        /// the action that will be executed
        /// </summary>
        public Action Action { get; private set; }

         /// <summary>
        /// creates a new BeforeSimpleCommandInvokedArgs
        /// </summary>
        /// <param name="lang">the language</param>
        /// <param name="text">the simple command text</param>
        /// <param name="speechGroupKey">the speech group key</param>
        /// <param name="action">the action to execute</param>
        public AfterSimpleComamndInokedArgs(string lang, string text, string speechGroupKey, Action action)
        {
            this.Lang = lang;
            this.Text = text;
            this.SpeechGroupKey = speechGroupKey;
            this.Action = action;
        }
    }
}
