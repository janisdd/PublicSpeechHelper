using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicSpeechHelper.Helpers
{
    /// <summary>
    /// describes the current state of the speech helper
    /// </summary>
    public enum ExecutingState
    {

        /// <summary>
        /// currently no commands are loaded
        /// </summary>
        NoCommandsLoaded,

        /// <summary>
        /// not listening for commands
        /// </summary>
        NotListening,

        /// <summary>
        /// just listening for commands
        /// </summary>
        ListeningForMethods,


        /// <summary>
        /// just search for parameters now
        /// </summary>
        ListeningForParameters,

        /// <summary>
        /// parameter found, now listening for the value
        /// </summary>
        ListeningForParameterValue

    }
}
