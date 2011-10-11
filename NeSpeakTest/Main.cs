using System;
using NeSpeak;

namespace NeSpeakTest
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Creating an NeSpeak lib object");
//			ESpeakLibrary lib = new ESpeakLibrary (); //failsafe defaults
//			ESpeakLibrary lib = new ESpeakLibrary (espeak_AUDIO_OUTPUT.AUDIO_OUTPUT_PLAYBACK); //Use for live synthesis
//			ESpeakLibrary lib = new ESpeakLibrary (espeak_AUDIO_OUTPUT.AUDIO_OUTPUT_RETRIEVAL);
			ESpeakLibrary lib = new ESpeakLibrary (espeak_AUDIO_OUTPUT.AUDIO_OUTPUT_SYNCHRONOUS); //Use for recording to wav
			
			Console.WriteLine ("Created, calling a method...");
			bool isplaying = lib.IsPlaying ();
			
			Console.WriteLine ("Method called, isPlaying is " + isplaying);
			
			Console.WriteLine ("Speaking chars:");
			lib.SpeakChar ('a');
			lib.SpeakChar ('b');
			lib.SpeakChar ('c');
			lib.SpeakChar ('d');
			
			Console.WriteLine ("Speaking a  sentence");
			lib.Synthesize ("This is a test.");
			
			Console.WriteLine ("Speaking a long sentence");
			lib.Synthesize ("This is a test of a long sentence that will be cut short.");
			
			
			System.Threading.Thread.Sleep (2500);
			Console.WriteLine ("Cancelling playback.");
			lib.Cancel ();
			
			Console.WriteLine ("Speaking a long sentence, syncing first");
			lib.Synchronize ();
			System.Threading.Thread.Sleep (500);
			lib.Synthesize ("This is another test of a longer sentence that might take a while to complete.");
//			lib.Synthesize ("Christina, you are very beautiful and I like you!");
			
			Console.WriteLine ("Spoken, synchronizing and shutting down (In async mode speech and callbacks will come after this)");
			lib.Terminate ();
			
			Console.WriteLine ("Checking that it has been terminated");
			try {
				lib.SpeakChar('a');
			} catch (InvalidOperationException ex) {
				Console.WriteLine ("Caught exeption with message: " + ex.Message);
			}
		}
	}
}

