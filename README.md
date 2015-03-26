# NeSpeak



A Mono wrapper for libespeak, the speech synthesis library. Currently it allows the basic functions: synchronous and asynchronous playback of synthesized text. In the future my hope is to implement the callback handlers needed to save the synthesized sounds as wav files. The project was created because I couldn't find any other ways to do text-to-speech with Mono on Linux. (.Net text-to-speech is included on Windows/MS .Net but not in Mono.)

The project is currently on hiatus because (as usual) I don't have time to work on it, but the basic functions do work if you're interested in using it. (I haven't made any releases yet, so you'll want to download the source and build it.) If you are interested in contributing that would be great, just send me an email!

In order to use the wrapper you will need the espeak library installed on your computer, just use your favourite package manager to install espeak. The wrapper has only been tested on my own computer, so if you find any problems please report them. (Library versions might be a particular issue which I haven't looked closer at.)

##Disclaimer
This code is getting pretty old and hasn't had any love in quite a while.
