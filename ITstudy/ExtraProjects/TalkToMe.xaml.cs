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



namespace ITstudy.ExtraProjects
{
    /// <summary>
    /// Lets you enter text and can then read it back to you.
    /// </summary>
    public sealed partial class TalkToMe : Page
    {
        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "08:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "5";
        // Date when this project was finished
        string ProjectDateFinished = "26/02/21";


        string TextToSpeak;

        Windows.Media.SpeechSynthesis.SpeechSynthesizer synth;
        MediaElement mediaElement;


        public TalkToMe()
        {
            this.InitializeComponent();
        }


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
