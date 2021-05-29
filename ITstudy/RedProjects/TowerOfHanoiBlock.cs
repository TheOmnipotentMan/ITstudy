using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Windows.UI.Xaml.Media;



namespace ITstudy.RedProjects
{

    // Base for all blocks in TowerOfHanoi
    public class TowerOfHanoiBlock
    {
        // Base variables, should not change generally
        public string BlockName;
        public double Height;
        public double WidthBase;

        // Block values
        public double BlockWidth;
        public double BlockOpacity;

        /// <summary>
        /// Is this block clickable by user
        /// </summary>
        public bool IsClickable;

        // Colour values
        public SolidColorBrush BlockColour;
        public SolidColorBrush BorderColour;




        // Event, when a property/value changes, prompting an update for binded values
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Create a tower block for Tower of Hanoi
        /// </summary>
        /// <param name="name">Name, used to identify the block when it is clicked</param>
        /// <param name="height">The height of the block, same for all blocks</param>
        /// <param name="widthBase">The width of the UI element, the maximum width the block will ever be</param>
        public TowerOfHanoiBlock(string name, double height, double widthBase)
        {
            this.BlockName = name;
            this.Height = height;
            this.WidthBase = widthBase;

            this.BlockWidth = 0;
            this.BlockOpacity = 0;

            this.IsClickable = false;

            this.BlockColour = new SolidColorBrush();
            this.BorderColour = new SolidColorBrush();
            BorderColour.Opacity = 0;

            NotifyPropertyChanged();
        }



        // https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=net-5.0
        private void NotifyPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }


        /// <summary>
        /// Show the block on screen
        /// </summary>
        /// <param name="width">The desired width of this block</param>
        /// <param name="colour">The desired colour of this block</param>
        public void ShowBlock(double width, Windows.UI.Color colour)
        {
            BlockWidth = Math.Clamp(width, 0, WidthBase);
            BlockOpacity = 1;
            BlockColour.Color = colour;
            NotifyPropertyChanged();
        }

        /// <summary>
        /// Set all widths of 0, making elements no longer visible
        /// </summary>
        public void HideBlock()
        {
            BlockOpacity = 0;
            NotifyPropertyChanged();
        }

        /// <summary>
        /// Show an outline at this position, used to show the currently selected block, and possible moves
        /// </summary>
        /// <param name="width">The desired width of the outline, should be identical to that of the block it is representing</param>
        /// <param name="colour">The desired colour of the outline</param>
        /// <param name="thickness">The desired BorderThickness of the outline</param>
        public void ShowOutline(Windows.UI.Color colour)
        {
            BorderColour.Opacity = 1;
            BorderColour.Color = colour;
            NotifyPropertyChanged();
        }

        /// <summary>
        /// Hide the outline at this position
        /// </summary>
        public void HideOutline()
        {
            BorderColour.Opacity = 0;
            NotifyPropertyChanged();
        }



    }
}
