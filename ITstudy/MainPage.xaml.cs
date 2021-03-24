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

using System.Collections.ObjectModel;

// TODO remove if the app is ever finished
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ITstudy
{
    

    /// <summary>
    /// The Main Page of ITstudy, shows general info and handles all navigation related tasks
    /// </summary>
    public sealed partial class MainPage : Page
    {
                
        string SelectedProjectName;
        

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
            /*  var CurrentTheme = myITstudy.ActualTheme;
                myITstudy.RequestedTheme = CurrentTheme++; */

            Debug.WriteLine("Trying to change theme, from {0}.", myITstudy.ActualTheme);



            // if (myITstudy.ActualTheme == (ElementTheme).0) { }
            if (myITstudy.ActualTheme == ElementTheme.Light) { myITstudy.RequestedTheme = ElementTheme.Dark; }
            else { myITstudy.RequestedTheme = ElementTheme.Light; }            

            Debug.WriteLine("Trying to change theme, to {0}.", myITstudy.ActualTheme);

        }







        // Handles the selection of Projects from the Nav-Menu

        /* Select the desired project to be loaded into the ContentFrame
         * This is done by using the name of the clicked/event-sender button as the argument
         * The only problem is that there is no correcting feedback when matching that name with the project (think enum/list with projects),
         * but seeing as this is a solo project, I don't really care
         */

        // TODO figure out if new pages are actually being deleted or not when a new one is made
        // currently setting the Content in the frame to null, and hope for a miracle from the garbage man
        // As far as I can see, the memory usage goes up when you spam new pages, though this might alos just be because it takes a while before garbage collection starts
        private void NewProjectSelected_Click(object sender, RoutedEventArgs e)
        {
            // get the triggering button and take its name
            Button selectedProjectButton = sender as Button;
            if (selectedProjectButton != null) { SelectedProjectName = selectedProjectButton.Name; }

            // clear current content, should trigger garbage collection (I hope)
            ContentFrame.Content = null;
            SelectNewProject();
        }

        private bool SelectNewProject()
        {
            // make sure we actually have a project name
            if (SelectedProjectName == null) { return false; }

            // fill the ContentFrame with the selected project (add any new projects/pages here to make them selectable)
            switch (SelectedProjectName)
            {
                // RED
                case "Calculator": { ContentFrame.Content = new RedProjects.Calculator(); return true; }
                case "NameTheImage": { ContentFrame.Content = new RedProjects.NameTheImage(); return true; }
                case "TextEncryption": { ContentFrame.Content = new RedProjects.TextEncryption(); return true; }
                case "LetterFrequency": { ContentFrame.Content = new RedProjects.LetterFrequency(); return true; }
                case "RomanCalculator": { ContentFrame.Content = new RedProjects.RomanCalculator(); return true; }

                // GREEN
                case "TaxiService": { ContentFrame.Content = new GreenProjects.TaxiService(); return true; }

                // EXTRA
                case "TestPage1": { ContentFrame.Content = new TestPage1(); return true; }
                case "TalkToMe": { ContentFrame.Content = new ExtraProjects.TalkToMe(); return true; }


                default: 
                    { 
                        ContentFrame.Content = new TestPage1(); 
                        Debug.WriteLine(string.Format("SelectNewProject couldn't detect a valid SelectedProjectName, make sure the Button.Name is the desired ProjectName.\n", "SelectedProjectName was: {0}", SelectedProjectName)); 
                        return false; 
                    }
            }
        }



        /* Trying my hand at Datatemplates again, no succes again


        /// <summary>
        /// Custom Class for all sub-projects. It holds their details and is required by MainPage in order to call the project.
        /// </summary>
        public class ITstudyProject
        {
            // general project info
            public string Name { get; set; }
            public string Address { get; set; }
            public bool IsComplete { get; set; }
            public string DisplayName { get; set; }

            // xaml info
            public Uri StatusImageSource { get; }

            public ITstudyProject(string name, bool isComplete, string displayName = null)
            {
                this.Name = name;
                this.IsComplete = isComplete;

                if (displayName == null) { this.DisplayName = name; }
                else { this.DisplayName = displayName; }

                // Might need async storage stuff
                if (isComplete) { this.StatusImageSource = new Uri("ms-appx:///Assets\\StatusOK_16x.png"); }
                else { this.StatusImageSource = new Uri("ms-appx:///Assets\\StatusBlocked_16x.png"); }
            }
        }

        #region ProjectDeclaration

        public class ITstudyRedProjects : List<ITstudyProject>
        {
            public ITstudyRedProjects(string name, bool isComplete, string displayName = null)
            {

                // 3. TextEncryption
                Add(new ITstudyProject(ITstudy.RedProjects.TextEncryption.GetProjectName(), ITstudy.RedProjects.TextEncryption.GetIsProjectCompleted(), ITstudy.RedProjects.TextEncryption.GetProjectDisplayName()));

            }
        }

        public class ITstudyExtraProjects : List<ITstudyProject>
        {
            public ITstudyExtraProjects(string name, bool isComplete, string displayName = null)
            {

                // 2. Talk To Me
                Add(new ITstudyProject(ITstudy.ExtraProjects.TalkToMe.GetProjectName(), ITstudy.ExtraProjects.TalkToMe.GetIsProjectCompleted(), ITstudy.ExtraProjects.TalkToMe.GetProjectDisplayName()));
            }
        }


        #endregion ProjectDeclaration

        */

    }





}
