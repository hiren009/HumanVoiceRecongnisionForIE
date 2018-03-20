using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace VoiceRecognition.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }


        public void ProcessSpeechResponse()
        {
            var cFileName = SaveMp3();
            var cWavFileName = GetWavFile(cFileName);

            var oResp = MakeRequest(cWavFileName);
            Session["SpeechResp"] = oResp;

            try
            {
                System.IO.File.Delete(cFileName);
                System.IO.File.Delete(cWavFileName);
            }
            catch (Exception)
            {
            }
        }

        public string GetSpeechResponseFromSession()
        {
            if (Session["SpeechResp"] != null)
            {
                var cRet = Convert.ToString(Session["SpeechResp"]);
                Session["SpeechResp"] = null;
                return cRet;
            }

            return "";
        }


        #region Initial Recording

        public ActionResult Index_1()
        {
            return View();
        }


        public ActionResult Index_2()
        {
            return View();
        }


        #endregion


        #region Helper Functions

        public string SaveMp3()
        {
            var cFileName = Server.MapPath("~/recordings/" + DateTime.Now.Ticks.ToString() + ".mp3");

            var receiveStream = Request.InputStream;
            byte[] buffer = new byte[32768];
            using (FileStream fileStream = System.IO.File.Create(cFileName))
            {
                while (true)
                {
                    int read = receiveStream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        break;
                    fileStream.Write(buffer, 0, read);
                }
            }
            return cFileName;
        }

        public string GetSpeechResponse()
        {
            var cFileName = SaveMp3();
            var cWavFileName = GetWavFile(cFileName);

            var oResp = MakeRequest(cWavFileName);

            return oResp;
        }


        private string MakeRequest(string audioFilePath)
        {
            var recognitionMode = "conversation";
            var outputMode = "simple";

            var requestUri = string.Format("https://speech.platform.bing.com/speech/recognition/{0}/cognitiveservices/v1?language=en-US&format={1}", recognitionMode, outputMode);

            HttpWebRequest request = null;
            request = (HttpWebRequest)HttpWebRequest.Create(requestUri);
            request.SendChunked = true;
            request.Accept = @"application/json;text/xml";
            request.Method = "POST";
            request.ProtocolVersion = HttpVersion.Version11;
            request.ContentType = @"audio/wav; codec=audio/pcm; samplerate=16000";
            request.Headers["Ocp-Apim-Subscription-Key"] = "b74cce6fb837456a90fa681f59277c04";

            // Send an audio file by 1024 byte chunks
            using (FileStream fs = new FileStream(audioFilePath, FileMode.Open, FileAccess.Read))
            {

                /*
                * Open a request stream and write 1024 byte chunks in the stream one at a time.
                */
                byte[] buffer = null;
                int bytesRead = 0;
                using (Stream requestStream = request.GetRequestStream())
                {
                    /*
                    * Read 1024 raw bytes from the input audio file.
                    */
                    buffer = new Byte[checked((uint)Math.Min(1024, (int)fs.Length))];
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        requestStream.Write(buffer, 0, bytesRead);
                    }

                    // Flush
                    requestStream.Flush();
                }
            }


            /*
            * Get the response from the service.
            */
            var responseString = "";
            Console.WriteLine("Response:");
            using (WebResponse response = request.GetResponse())
            {
                Console.WriteLine(((HttpWebResponse)response).StatusCode);

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    responseString = sr.ReadToEnd();
                }

                Console.WriteLine(responseString);
                
            }

            return responseString;
        }

        private string GetWavFile(string mp3FilePath)
        {
            var _outPath_ = mp3FilePath.Replace(".mp3", ".wav");

            using (Mp3FileReader mp3 = new Mp3FileReader(mp3FilePath))
            {
                using (WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3))
                {
                    WaveFileWriter.CreateWaveFile(_outPath_, pcm);
                    return _outPath_;
                }
            }
        }

        #endregion

    }
}