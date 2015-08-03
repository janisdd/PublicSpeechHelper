using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicSpeechHelper.Helpers
{
    public class SpeechTupleArgs
    {

        public SpeechTuple SpeechTuple { get; set; }

        /// <summary>
        /// true: cancel command processing, false (default): go executing
        /// </summary>
        public bool CancelCommand { get; set; }


        public SpeechTupleArgs(SpeechTuple speechTuple)
        {
            this.SpeechTuple = speechTuple;
        }

    }
}
