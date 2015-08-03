using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicSpeechHelper
{
    public class PublicSpeechRecognitionArgs
    {

        /// <summary>
        /// gets the recognized text
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// true: cancel all further command processing, false (default): look for possible commands to execute 
        /// </summary>
        public bool CancelFurtherCommands { get; set; }

        public PublicSpeechRecognitionArgs(string text)
        {
            Text = text;
            this.CancelFurtherCommands = false;
        }
    }
}
