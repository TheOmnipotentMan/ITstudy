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

    
    public static class Extensions
    {
        // Extension for enums that returns the next value in the enum
        // Copied from https://stackoverflow.com/questions/642542/how-to-get-next-or-previous-enum-value-in-c-sharp
        public static T Next<T>(this T src) where T : Enum
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
    }





    /// <summary>
    /// The Main Page of ITstudy, shows general info and handles all navigation related tasks
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // The name of the button in the projects-ListView (navigation list on the left side in mainpage), which is equal to the actual project name, thereby determining which project to select      
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
        // As far as I can see, the memory usage goes up when you spam new pages, though this might also just be because it takes a while before garbage collection starts
        private void NewProjectSelected_Click(object sender, RoutedEventArgs e)
        {
            // get the triggering button and take its name
            Button selectedProjectButton = sender as Button;
            if (selectedProjectButton != null) { SelectedProjectName = selectedProjectButton.Name; }


            SelectNewProject(SelectedProjectName);
        }

        private bool SelectNewProject(string projectName)
        {
            // Make sure we actually have a project name
            if (string.IsNullOrEmpty(SelectedProjectName)) { return false; }

            // Clear current content, should trigger auto garbage collection (I think & hope)
            ContentFrame.Content = null;

            // Fill the ContentFrame with the selected project (add any new projects/pages here to make them selectable)
            switch (SelectedProjectName)
            {
                // RED
                case "Calculator": { ContentFrame.Content = new RedProjects.Calculator(); return true; }
                case "NameTheImage": { ContentFrame.Content = new RedProjects.NameTheImage(); return true; }
                case "TextEncryption": { ContentFrame.Content = new RedProjects.TextEncryption(); return true; }
                case "LetterFrequency": { ContentFrame.Content = new RedProjects.LetterFrequency(); return true; }
                case "RomanCalculator": { ContentFrame.Content = new RedProjects.RomanCalculator(); return true; }
                case "RansomNote": { ContentFrame.Content = new RedProjects.RansomNote(); return true; }
                case "TicTacToe": { ContentFrame.Content = new RedProjects.TicTacToe(); return true; }
                case "MorseCode": { ContentFrame.Content = new RedProjects.MorseCode(); return true; }
                case "TowerOfHanoi": { ContentFrame.Content = new RedProjects.TowerOfHanoi(); return true; }
                case "Yahtzee": { ContentFrame.Content = new RedProjects.Yahtzee(); return true; }

                // GREEN
                case "TaxiService": { ContentFrame.Content = new GreenProjects.TaxiService(); return true; }
                case "Tuition": { ContentFrame.Content = new GreenProjects.Tuition(); return true; }
                case "CarRental": { ContentFrame.Content = new GreenProjects.CarRental(); return true; }
                case "CampingSite": { ContentFrame.Content = new GreenProjects.CampingSite(); return true; }
                case "DumpContainerRenting": { ContentFrame.Content = new GreenProjects.DumpContainerRenting(); return true; }
                case "TransportCompany": { ContentFrame.Content = new GreenProjects.TransportCompany(); return true; }
                case "WaterUsage": { ContentFrame.Content = new GreenProjects.WaterUsage(); return true; }
                case "TaxBrackets": { ContentFrame.Content = new GreenProjects.TaxBrackets(); return true; }

                // EXTRA
                case "TestPage1": { ContentFrame.Content = new TestPage1(); return true; }
                case "TalkToMe": { ContentFrame.Content = new ExtraProjects.TalkToMe(); return true; }

                // Default case, triggers when the given SelectedProjectName does not have a corresponding case in the switch-statement, and writes the situation to the Output
                // Currently selects TestPage1, so something happens on screen that can warn of an error.
                default: 
                    { 
                        ContentFrame.Content = new TestPage1(); 
                        Debug.WriteLine(string.Format("SelectNewProject couldn't detect a valid SelectedProjectName, make sure the Button.Name is the desired ProjectName.\n", "SelectedProjectName was: {0}", SelectedProjectName)); 
                        return false; 
                    }
            }
        }




    }





}
