using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PublicSpeechHelper.SpeechApi;

namespace PublicSpeechHelper.Helpers
{
    /// <summary>
    /// a SpeechTuple with all necessary info to execute a command
    /// </summary>
    public class SpeechTuple
    {
        /// <summary>
        /// true: group is enabled, false: not
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// the method
        /// </summary>
        public SpeechMethod Method { get; set; }

        /// <summary>
        /// the instance to invoke the speech method on
        /// </summary>
        public object InvokeInstance { get; set; }

        /// <summary>
        /// creates a new SpeechTuple
        /// </summary>
        /// <param name="isEnabled">true: group is enabled, false: not</param>
        /// <param name="method">the method</param>
        /// <param name="invokeInstance">the invoking instance</param>
        public SpeechTuple(bool isEnabled, SpeechMethod method, object invokeInstance)
        {
            this.IsEnabled = isEnabled;
            this.Method = method;
            this.InvokeInstance = invokeInstance;
        }
    }
}
