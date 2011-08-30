using System;
using System.Runtime.InteropServices;
using System.Text;
namespace NeSpeak
{
	public class ESpeakLibrary
	{
		/// <summary>
		/// This bool is for internal use and informs if the external lib has been terminated.
		/// </summary>
		private bool terminated = false;
		
		/// <summary>
		/// The type of output currently used by the library
		/// </summary>
		public espeak_AUDIO_OUTPUT AudioOutputType {get; internal set;}
		
		/// <summary>
		/// Events is actually espeak_events enum TODO: Marshallaed as a constant size array, should figure out the corect size.
		/// </summary>
		public delegate int SynthCallBackDelegate (IntPtr wav, int numsamples,[MarshalAs(UnmanagedType.LPArray, SizeConst=50)] int[] events);

		/// <summary>
		/// Constructor that sets up eSpeak w/ default values that normally work for synchronous and
		/// immediate playback.
		/// </summary>
		public ESpeakLibrary ()
		{
			AudioOutputType = espeak_AUDIO_OUTPUT.AUDIO_OUTPUT_SYNCH_PLAYBACK;
			
			espeak_Initialize (espeak_AUDIO_OUTPUT.AUDIO_OUTPUT_SYNCH_PLAYBACK, 100, null, 0);
			espeak_SetSynthCallback (new SynthCallBackDelegate (OnCallBackReceived));
		}
		
//		/// <summary>
//		/// Constructor where you can set the audio output mode.
//		/// </summary>
//		/// <param name="output">
//		/// the audio data can either be played by eSpeak or passed back by the SynthCallback function.
//		/// </param>
		public ESpeakLibrary (espeak_AUDIO_OUTPUT output)
		{
			AudioOutputType = output;
			
			espeak_Initialize (output, 100, null, 0);
			espeak_SetSynthCallback (new SynthCallBackDelegate (OnCallBackReceived));
		}
		
//		/// <summary>
//		/// Constructor with extra options for sound output etc.
//		/// </summary>
//		/// <param name="output">
//		/// the audio data can either be played by eSpeak or passed back by the SynthCallback function.
//		/// </param>
//		/// <param name="buflength">
//		/// The length in mS of sound buffers passed to the SynthCallback function.
//		/// </param>
//		/// <param name="path">
//		/// The directory which contains the espeak-data directory, or <see cref="null"/> for the default location.
//		/// </param>
//		/// <param name="options">
//		/// bit 0:  1=allow espeakEVENT_PHONEME events.
//		/// bit 15: 1=don't exit if espeak_data is not found (used for --help)
//		/// </param>
		public ESpeakLibrary (espeak_AUDIO_OUTPUT output, int buflength, string path, int options)
		{
			AudioOutputType = output;
			
			espeak_Initialize (output, buflength, path, options);
			espeak_SetSynthCallback (new SynthCallBackDelegate (OnCallBackReceived));
		}

		/// <summary>
		/// Is called by the eSpeak library multiple times during synthesis execution and handles
		/// the returned values.
		/// </summary>
		public int OnCallBackReceived (IntPtr wav, int numsamples, [MarshalAs(UnmanagedType.LPArray, SizeConst=50)]int[] events)
		{
			//TODO: Lots of advanced handling here!
			Console.WriteLine ("DEBUG: Callback checking received wav");
			if (wav == IntPtr.Zero)
			{
				Console.WriteLine ("DEBUG: Callback received null value wav!");
				return 0; //TODO: Close Wav file etc
			}
			short managedWav = Marshal.ReadInt16 (wav); //Is a short in native code
			Console.WriteLine ("DEBUG: Callback received wav equal to: " + managedWav);
			return 0; //Continue synthesis
		}

		/// <summary>
		/// Speaks a single char through the configured methods.
		/// </summary>
		public void SpeakChar (char toSpeak)
		{
			CheckIfTerminated ();

			espeak_ERROR result = espeak_Char (toSpeak);
			if (result == espeak_ERROR.EE_INTERNAL_ERROR) {
				throw new InvalidOperationException ("Internal error in the eSpeak library.");
			}
		}

		/// <summary>
		/// Returns true if audio is currently playing.
		/// </summary>
		public bool IsPlaying ()
		{
			CheckIfTerminated ();
			return Convert.ToBoolean(espeak_IsPlaying ());
		}
		
		/// <summary>
		/// Synthesizes the given text, returning it in whatever fashion was set in the constructor.
		/// </summary>
		/// <param name="text">
		/// A <see cref="System.String"/>
		/// </param>
		public void Synthesize (string text)
		{
			CheckIfTerminated ();

			espeak_ERROR result = espeak_Synth (text, text.ToCharArray().Length, 0, 1, 0, 0, IntPtr.Zero, IntPtr.Zero);
			if (result == espeak_ERROR.EE_INTERNAL_ERROR) {
				throw new InvalidOperationException ("Internal error in the eSpeak library.");
			}
		}
		
		/// <summary>
		/// In async playback/retrieval mode this method synchronizes the calling thread with the library
		/// by waiting to return until library is finished with all synthesis. (May take a while!)
		/// </summary>
		public void Synchronize ()
		{
			espeak_Synchronize ();
		}
		
		/// <summary>
		/// Waits with returning until all synthesis is complete and then shuts down the library's
		/// synthesis functionality. (May take a while.) Must be called at the end of the program!
		/// </summary>
		public void Terminate ()
		{
			espeak_Synchronize ();
			espeak_Terminate ();
			terminated = true;
		}
		
		/// <summary>
		/// This method must be called by all public methods to make sure nothing segfaults
		/// because the lib has been terminated.
		/// </summary>
		private void CheckIfTerminated ()
		{
			if (terminated) throw new InvalidOperationException ("The external library has already been terminated.");
		}

		#region libeSpeak_imports
		[DllImport("libespeak")]
		private static extern int espeak_Initialize (espeak_AUDIO_OUTPUT output, int buflength, string path, int options);

		[DllImport("libespeak")]
		private static extern void espeak_SetSynthCallback (SynthCallBackDelegate SynthCallback);

		[DllImport("libespeak")]
		private static extern int espeak_IsPlaying ();

		[DllImport("libespeak")]
		private static extern espeak_ERROR espeak_Char (char character);

		[DllImport("libespeak")]
		private static extern espeak_ERROR espeak_Synchronize ();

		[DllImport("libespeak")]
		private static extern espeak_ERROR espeak_Terminate ();

		[DllImport("libespeak", CharSet=CharSet.Ansi)]
		private static extern espeak_ERROR espeak_Synth (string text, int text_length, int position, int position_type,
		                                                 int end_position, int flags, IntPtr unique_identifier, IntPtr user_data);
		#endregion
		
	}

	#region espeak_enums
	
	/// <summary>
	/// Determines the type of playback mode.
	/// </summary>
	public enum espeak_AUDIO_OUTPUT
	{
		/// <summary>
		/// PLAYBACK mode: plays the audio data, supplies events to the calling program
		/// </summary>
		AUDIO_OUTPUT_PLAYBACK,

		/// <summary>
		/// RETRIEVAL mode: supplies audio data and events to the calling program
		/// </summary>
		AUDIO_OUTPUT_RETRIEVAL,

		/// <summary>
		/// SYNCHRONOUS mode: as RETRIEVAL but doesn't return until synthesis is completed
		/// </summary>
		AUDIO_OUTPUT_SYNCHRONOUS,

		/// <summary>
		/// Synchronous playback
		/// </summary>
		AUDIO_OUTPUT_SYNCH_PLAYBACK
	}

	public enum espeak_EVENT_TYPE
	{
		espeakEVENT_LIST_TERMINATED = 0,
		// Retrieval mode: terminates the event list.
		espeakEVENT_WORD = 1,
		// Start of word
		espeakEVENT_SENTENCE = 2,
		// Start of sentence
		espeakEVENT_MARK = 3,
		// Mark
		espeakEVENT_PLAY = 4,
		// Audio element
		espeakEVENT_END = 5,
		// End of sentence or clause
		espeakEVENT_MSG_TERMINATED = 6,
		// End of message
		espeakEVENT_PHONEME = 7,
		// Phoneme, if enabled in espeak_Initialize()
		espeakEVENT_SAMPLERATE = 8
		// internal use, set sample rate
	}

	public enum espeak_POSITION_TYPE
	{
		POS_CHARACTER = 1,
		POS_WORD,
		POS_SENTENCE 
	}

	public enum espeak_ERROR
	{
		EE_OK = 0,
		EE_INTERNAL_ERROR = -1,
		EE_BUFFER_FULL = 1,
		EE_NOT_FOUND = 2
	}

	public struct espeak_EVENT
	{
		espeak_EVENT_TYPE type;
		int unique_identifier;
		// message identifier (or 0 for key or character)
		int text_position;
		// the number of characters from the start of the text
		int length;
		// word length, in characters (for espeakEVENT_WORD)
		int audio_position;
		// the time in mS within the generated speech output data
		int sample;
		// sample id (internal use)
		IntPtr user_data;
		// pointer supplied by the calling program
		type_id id;
	}
	public struct type_id
	{
		int number;
		// used for WORD and SENTENCE events. For PHONEME events this is the phoneme mnemonic.
		string name;
		// used for MARK and PLAY events.  UTF8 string
	}
	#endregion
}

