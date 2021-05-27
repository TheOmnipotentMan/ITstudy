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
        public string BlockName;
        public double WidthBase;
        public double Height;


        public double BlockWidth;
        public double BorderWidth;
        public double BorderThickness;
        public SolidColorBrush BlockColour;

        // Tower block colours, create a number of block-colours, used to better distinguish between blocks with adjacent/similar sizes
        Windows.UI.Color[] BlockColours = new Windows.UI.Color[]
        {
                // CadetBlue, #FF5F9EA0
                Windows.UI.Color.FromArgb(95, 158, 160, 100),
                // Chocolate, #FFD2691E
                Windows.UI.Color.FromArgb(210, 105, 30, 100),
                // DarkKhaki, #FFBDB76B
                Windows.UI.Color.FromArgb(189, 183, 107, 100),
                // OliveDrab, #FF6B8E23
                Windows.UI.Color.FromArgb(107, 142, 35, 100)
        };



    // Event, when a property/value changes, prompting an update for binded values
    public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Create a tower block for Tower of Hanoi
        /// </summary>
        /// <param name="name">Name, can be used eg as a number in the center of the block to show its width more clearly</param>
        /// <param name="widthBase">The base width of the UI element, must be the max width the block will ever be</param>
        /// <param name="height">The height of the block, same for all blocks</param>
        /// <param name="width">The current width of this block, can be 0 and thereby hiding the element</param>
        /// <param name="isShowing">Optional, </param>
        public TowerOfHanoiBlock(string name, double widthBase, double height, double width = 0, bool isShowing = false)
        {
            this.BlockName = name;
            this.WidthBase = widthBase;
            this.Height = height;

            this.BlockWidth = isShowing ? width : 0;
            this.BorderWidth = 0;
            this.BorderThickness = 0;
            this.BlockColour = new SolidColorBrush();

            NotifyPropertyChanged();
        }



        // https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=net-5.0
        private void NotifyPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }


        /// <summary>
        /// Show the block on screen.
        /// </summary>
        /// <param name="width">The width of the block on this position</param>
        public void PlaceBlock(double width, int blockLevel)
        {
            width = Math.Clamp(width, 0, WidthBase);
            BlockWidth = width;
            BorderWidth = 0;
            BlockColour.Color = BlockColours[blockLevel % BlockColours.Length];
            NotifyPropertyChanged();
        }

        /// <summary>
        /// Show an outline at this position, used to show possible moves
        /// </summary>
        /// <param name="width">The desired width of the outline</param>
        public void ShowOutline(double width)
        {
            BorderWidth = width;
            BorderThickness = 4;
            NotifyPropertyChanged();
        }

        /// <summary>
        /// Set all widths of 0, making elements no longer visible
        /// </summary>
        public void RemoveBlock()
        {
            BlockWidth = 0;
            BorderWidth = 0;
            BorderThickness = 0;
            NotifyPropertyChanged();
        }

        /// <summary>
        /// Is the current block shown on screen.
        /// </summary>
        /// <returns></returns>
        public bool IsShowing()
        {
            return (BlockWidth > 0);
        }

        /// <summary>
        /// Is the current block showing its outline, suggesting that it is a position a block can be moved to.
        /// </summary>
        /// <returns></returns>
        public bool IsAvailable()
        {
            return (BorderWidth > 0);
        }

    }
}
