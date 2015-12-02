


> Written with [StackEdit](https://stackedit.io/)

Public Speech Helper for c#
=================

*Provides an easy way to implement speech-commands*
<br />

**This library uses the *System.Speech* namespace ... so the speech recognition will be ... surprisingly bad ;)**

### the idea
the basic idea is that you add some attributes to your existing code and just a few lines of extra code and you will have a speech enabled program

### note
when you see "helper." then helper is an instance of SpeechHelper

### basic workflow

first you create an instance of the SpeechHelper 
```csharp
var helper = new new SpeechHelper(CultureInfo.CurrentCulture.Name);
```

after that you add simple commands, (comlex) commands and plain phrases

to finish the set up call

```csharp
helper.RebuildAllCommands(); //to apply the added commands
helper.StartListening(); //to start listening for commands
```


### the crawler (for attributes)

in order to get the speech enabled methods and parameters a crawler is used


### a note on parameter values and plain phrases
since the **System.Speech** namespace only provides the ability to recognize prior provided strings (no real speech-to-text ... the windows integrated speech-to-text is just too bad to be used in my opinion)
you need to provide phrases the user can speak to enter parameter values e.g. numbers from 0 to 20 are automatically added as phrases (see SpeechHelper.NumbersFrom and SpeechHelper.NumbersTo)

to add a phrase call
```csharp
//true is used to automatically call SpeechHelper.RebuildPlainPhrases to add the phrases the the interal recognition engine
//false --> SpeechHelper.RebuildPlainPhrases needs to be called manually (wihtout calling RebuildPlainPhrases the phrases are not recognized)
 
helper.AddPlainPhrase(true, "phrase1", "phrase2", "phrase3", ... , "phraseN");
```

### a note on the SpeechGroups and SpeechGroupKeys
the idea behind the SpeechGroups is that your app has multiple parts/contexts and some commands are only aviable on specific parts/contexts

e.g.
the user is currently in context 1 --> so he can only access the commands that are related to this context the other commands are disabled

the achieve this you can use the SpeechGroupKeys

look for every command if there is a SpeechGroupKeys aviable and set the key to e.g. context1

to disable all commands in the SpeechGroup "context1" just call
```csharp
helper.ChangeSpeechGroup("context1", false)
```
to enable the commands call

```csharp
helper.ChangeSpeechGroup("context1", true)
```

**one thing to mention here is that only the execution is disabled!**
so the recognition enginge will recognize the commands but they are not executed


### command types
* *complex commands*: used via attributes only
* *simple commands*: used via code only


### using *simple commands*
*simple commands* are added to the speech helper through the 
"AddSimpleCommand" method on the **SpeechHelper** class (you will need an instance of this class)

after you added (all) *simple commands* you need to call "helper.RebuildSimpleCommands();" or "helper.RebuildAllCommands();" in  order to make the new command(s) working

Signature:
```csharp
AddSimpleCommand(string lang, string text, string simpleSpeechGroupKey, Action action)
```
**lang:** this the language key e.g. de-de or en-gb ... this needs to be a valid culture string (CultureInfo.GetCultureInfo(lang))
**text:** this is the text the user needs to speak in oder to execute this command
**simpleSpeechGroupKey:** this is the speech group key
**action:** this is the action to perform when the command gets executed (a method without params and wihtout a return value)

e.g.
```csharp
var helper = new SpeechHelper("de-de");

helper.AddSimpleCommand("de-de", "start", "groupKey" () => {
	//do some stuff
});

helper.RebuildSimpleCommands();
```




### using *complex commands*

Here is a list of attributes used with *complex commands*

* ####SpeechEnabled
	 place this attribute on a class to make it visible to the crawler **(only needed when gathering commands/converter through an assembly)**
	  **multiple allowed?** false
	e.g.
	```csharp
	[SpeechEnabled]
	public class SomeClass {
		//members...
	}
	```
	
* ####SpeechMethod(string lang, params string[] synonyms)

	place this attribute on a method to make it speech enabled

   **multiple allowed?** true
   **lang:** the culture string *e.g. de-de, en-gb ...*
   **synonyms:** provide all synonyms that will call this method
 
	 e.g.
 	```csharp
	[SpeechMethod("de-de", "schlieﬂen", "schlieﬂe programm", "exit")]
	[SpeechMethod("en-gb", "close", "close application", "exit")]
	public void CloseApp() {
		//close the program
	}
	```

* ####SpeechParameterConverter(string converterKey)
 
 place this attribute on a method to make it speech enabled
 
 **multiple allowed?** false
 **converterKey:** this key is used to refer to this converter method from a speech parameter
 
 e.g.
 	```csharp
	[SpeechParameterConverter("window converter key")]
	public string WindowKeyConvert(string choice) {
		//return the window key for the choice
	}
	```

* ####SpeechParameter(string lang, string converterKey, params string[] synonyms)
	place this attribute in from of a parameter to make it speech enabled
	
 **multiple allowed?** true
 **lang:** the culture string *e.g. de-de, en-gb ...*
 **converterKey: ** the key of the converter used for this parameter
 **synonyms:** provide all synonyms for this parameter **(can be empty)** synonyms can then be used to switch input for multiple parameters 
 *so this allows the user to input the parameters in his prefered order*
	e.g.
	
 	```csharp
	[SpeechMethod("de-de", "gehe zu")]
	[SpeechMethod("en-gb", "goto")]
	public void GoToWindow(
		[SpeechParameter("de-de", "window converter key")]
		[SpeechParameter("en-gb", "window converter key", "window", "window key")]
		string windowKey 
	) {
		//switch to the window or open a new window via parameter windowKey 
	}
	```
	
### small example for attribute usage

```csharp
public partial class Form1 : Form {
	
	private readonly SpeechHelper helper;
	public Form1()
    {
	    InitializeComponent();
	    helper = new SpeechHelper(CultureInfo.CurrentCulture.Name); 
	    helper.GatherConverters(typeof(Form1), this);
	    helper.GatherCommands(typeof(Form1), this);

		//add some colors to listen for
		//de
        helper.AddPlainPhrase(true, "rot","blau");

        //en
        //helper.AddPlainPhrase(true, "red","blue");
		
		//set up the internal recognition engine
		helper.RebuildAllCommands();
		helper.StartListening();
    }
    
    [SpeechMethod("de-de","hintergrundfarbe")]
    [SpeechMethod("en-gb", "backgroundcolor")]
    //needs to be public
	public void SetBackground(
		[SpeechParameter("de-de", "colorConverterDE")]
        [SpeechParameter("en-gb", "colorConverterEN")]
		Color color)
    {
	    //set the color
	    this.BackColor = color;
    }

	[SpeechParameterConverter("colorConverterDE")]
    //needs to be public
    public Color ConvertColorDE(string color)
    {
        if (color == "rot")
        {
            return Color.FromKnownColor(KnownColor.Red);
        }else if (color == "blau")
        {
            return Color.FromKnownColor(KnownColor.Blue);
        }

        //throw error or return default value
        return Color.FromKnownColor(KnownColor.Gray);
    }
    
    [SpeechParameterConverter("colorConverterEN")]
    //needs to be public
    public Color ConvertColorEN(string color)
    {
        if (color == "red")
        {
            return Color.FromKnownColor(KnownColor.Red);
        }
        else if (color == "blue")
        {
            return Color.FromKnownColor(KnownColor.Blue);
        }

        //thor error or return default value
        return Color.FromKnownColor(KnownColor.Gray);
    }
}
```