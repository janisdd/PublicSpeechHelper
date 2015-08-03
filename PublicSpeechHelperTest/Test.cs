using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PublicSpeechHelper;

namespace PublicSpeechHelperTest
{

    //[PublicSpeechEnabled]
    public class Test
    {

        [PublicSpeechMethod("de-de", "test")]
        public void TestMethod0()
        {
            Console.WriteLine(@"Das ist nur ein Test!");
        }


        [PublicSpeechMethod("de-de", "test1")]
        public void TestMethod1([PublicSpeechArgument("de-de", "zahl")] int zahl)
        {
            Console.WriteLine(@"Diese Funktion gibt die Zahl: " + zahl + @" aus...");
        }

    }


    class Test2
    {
         
    }
}
