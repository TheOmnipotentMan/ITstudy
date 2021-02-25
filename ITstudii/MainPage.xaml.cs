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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ITstudii
{






    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

     


       










        // Method based on Microsoft Documentation @ https://docs.microsoft.com/en-us/visualstudio/get-started/csharp/tutorial-uwp?view=vs-2019
        private async void HelloWorldButton_Click(object sender, RoutedEventArgs e)
        {
            // get the text(content) of the button that is calling this event
            Button button = (Button)e.OriginalSource;
            string buttonText = button.Content.ToString();

            // pass the text into the windows SpeechSynthesis and play
            MediaElement mediaElement = new MediaElement();
            var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            Windows.Media.SpeechSynthesis.SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(buttonText);
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }


        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            // This has worked, in one build, im missing something. can it be done without the var 'CurrentTheme'?            
            /*  var CurrentTheme = myITstudii.ActualTheme;
                myITstudii.RequestedTheme = CurrentTheme++; */

            Debug.WriteLine("Trying to change theme, from {0}.", myITstudii.ActualTheme);



            // if (myITstudii.ActualTheme == (ElementTheme).0) { }
            if (myITstudii.ActualTheme == ElementTheme.Light) { myITstudii.RequestedTheme = ElementTheme.Dark; }
            else { myITstudii.RequestedTheme = ElementTheme.Light; }            

            Debug.WriteLine("Trying to change theme, to {0}.", myITstudii.ActualTheme);

        }

        /*
        private void ExtraProjectItem_Clicked(object sender, RoutedEventArgs e)
        {

        }
        */
    }






    /*
    // Data Template for ExtraProject ListView
    public class ExtraProject
    {
        public string ProjectName { get; set; }
        public bool IsFinished { get; set; }

        // TODO maybe add more, like time spend, or the code in plain text


        public ExtraProject(string projectName, bool isFinished)
        {
            ProjectName = projectName;
            IsFinished = isFinished;
        }
    }

    */



}
