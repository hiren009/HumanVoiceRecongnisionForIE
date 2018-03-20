# HumanVoiceRecongnisionForIE

Used https://beotiger.com/justrec#usg to record voice.

I am targeting internet explorer, so can not use webRtc or mediaRecorder. We need to use traditional methods such as flash, silverlight, etc.
Hence for this demo using flash.

I am using microsoft cognitive service for detecting human voice. We can use similar cognitive service from google or IBM. 
In this demo i am using bing speech to text convertor.

One more thing by default justrec plugin only converts into mp3 and bing speech translation expects wav format file. So i used NAudio for converting mp3 to wav before passing to it.
