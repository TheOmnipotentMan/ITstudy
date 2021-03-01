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







        // Handles the selection of Projects from the Nav-Menu, is ugly code that nobody needs to see
        #region ProjectSelect

        /* TODO A lot of this stuff, and project selection in general can be done better
         * Templates for xaml and variables in the classes (like IsCompleted) could automate most of the navigation menu
         * Might be implemented in future
         * !! could use button-name as argument !!
         */

        /* At the moment, every project has its own function to load the corresponding page.
         * This is because the selection is done with a button, and a button, afaik, can only call a method, it doens't hold any arguments.
         * An alternative to this would be to use something like a NavigationView, or anyhthing that can hold more information for what it wants to happen.
         * Then you could condense all these methods down to a single one.
         * But seeing as this is an stuy-project in programming and won't ever be perfect, I will leave it in its current state for now.
         */

        // TODO figure out if new pages are actually being deleted or not when a new one is made
        // currently setting the Content in the frame to null, and hope for a miracle from the garbage man




        // ----     EXTRA PROJECTS PAGE CALL      ------------

        private void ProjectSelected_Extra_TestPage(object sender, RoutedEventArgs e)
        {         
            ContentFrame.Content = null;
            ContentFrame.Content = new TestPage1();
        }

        private void ProjectSelected_Extra_TalkToMe(object sender, RoutedEventArgs e)
        {
            ContentFrame.Content = null;
            ContentFrame.Content = new ExtraProjects.TalkToMe();
        }







        // ----     RED PROJECTS PAGE CALL      ------------

        private void ProjectSelected_Red_Calculator(object sender, RoutedEventArgs e)
        {
            ContentFrame.Content = null;
            // TODO figure out why "RedProjects." is present here, it is a folder but apparently this is also a namespace?
            // Is this just for clarification about the location of the project, or is a folder in visual studio actually a namespace?
            ContentFrame.Content = new RedProjects.Calculator();
        }








        #endregion ProjectSelect


    }





}
