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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ITstudy.RedProjects
{

    /// <summary>
    /// A project in Text Encryption.
    /// I could use the build-in encryption via RSA but I would like to create my own
    /// However, that is easier said than done, so I will let it rest for now and return later when I have more experience with C# so that I can focus on the encryption itself
    /// </summary>
    public sealed partial class TextEncryption : Page
    {
        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "00:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "0";
        // Date when this project was finished
        string ProjectDateFinished = "00/00/00";


        public TextEncryption()
        {
            this.InitializeComponent();
            
        }
    }
}
