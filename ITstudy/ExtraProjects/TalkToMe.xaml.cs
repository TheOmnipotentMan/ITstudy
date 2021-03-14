using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// TODO remove if the app is ever finished
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ITstudy.ExtraProjects
{
    /// <summary>
    /// Lets you enter text and can then read it back to you.
    /// </summary>
    public sealed partial class TalkToMe : Page
    {


        string TextToSpeak;

        Windows.Media.SpeechSynthesis.SpeechSynthesizer synth;
        MediaElement mediaElement;


        public TalkToMe()
        {
            this.InitializeComponent();
        }



        // could use sender to determine the gender
        private void Voice_SetMale(object sender, RoutedEventArgs e)
        {
            Windows.Media.SpeechSynthesis.VoiceInformation voiceInfo =
                (
                    from voice in Windows.Media.SpeechSynthesis.SpeechSynthesizer.AllVoices
                    where voice.Gender == Windows.Media.SpeechSynthesis.VoiceGender.Male
                    select voice
                ).FirstOrDefault() ?? Windows.Media.SpeechSynthesis.SpeechSynthesizer.DefaultVoice;

            synth.Voice = voiceInfo;
        }


        private void Voice_SetFemale(object sender, RoutedEventArgs e)
        {
            Windows.Media.SpeechSynthesis.VoiceInformation voiceInfo =
                (
                    from voice in Windows.Media.SpeechSynthesis.SpeechSynthesizer.AllVoices
                    where voice.Gender == Windows.Media.SpeechSynthesis.VoiceGender.Female
                    select voice
                ).FirstOrDefault() ?? Windows.Media.SpeechSynthesis.SpeechSynthesizer.DefaultVoice;

            synth.Voice = voiceInfo;
        }






        private async void TextToSpeech(object sender, RoutedEventArgs e)
        {
            // get the text from the input-field
            TalkToMeInput.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out TextToSpeak);
            
            if (TextToSpeak == string.Empty) { TextToSpeak = "Hello World"; }

            if (mediaElement == null) { mediaElement = new MediaElement(); }
            if (synth == null) { synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer(); }

            // pass the text into the windows SpeechSynthesis and play
            Windows.Media.SpeechSynthesis.SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(TextToSpeak);
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();

            Debug.WriteLine(string.Format("Trying to SpeechSynthesis {0}.", TextToSpeak));
        }



    }

}
