using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.CompilerServices;
using System.Collections.Specialized;

using Windows.UI.Xaml.Media;

using System.Diagnostics;

namespace ITstudy.RedProjects
{

    // Base for all blocks in TowerOfHanoi
    public class TowerOfHanoiBlock : INotifyPropertyChanged
    {
        // Base variables, generally should not change
        public string BlockName;
        public double Height;
        public double WidthBase;

        // Block state, representation of the enum BlockState in TowerOfHanoi, can be used to check if you're trying to change to a state this block is already in
        public int BlockState;

        // Block values
        public int BlockNumber;
        public double BlockWidth;
        public double BlockOpacity;
        public double BorderSelectedOpacity;
        public double BorderAvailableOpacity;

        /// <summary>
        /// Is this block clickable by user
        /// </summary>
        public bool IsClickable = false;

        // Color values
        public SolidColorBrush BlockColor = new SolidColorBrush();




        // Event, when a property/value changes, prompting an update for binded values
        public event PropertyChangedEventHandler PropertyChanged;
        public event CollectionChangeEventHandler CollectionChanged;


        /// <summary>
        /// Create a tower block for Tower of Hanoi
        /// </summary>
        /// <param name="name">Name, used to identify the block when it is clicked</param>
        /// <param name="height">The height of the block, same for all blocks</param>
        /// <param name="widthBase">The width of the UI element, the maximum width the block will ever be</param>
        /// <param name="blockNumber">Whole number of this block, ie its 1-based index on the tower</param>
        public TowerOfHanoiBlock(string name, double height, double widthBase, int blockNumber)
        {
            this.BlockName = name;
            this.BlockNumber = blockNumber;
            this.Height = height;
            this.WidthBase = widthBase;

            this.BlockState = 0;

            this.BlockWidth = 0;
            this.BlockOpacity = 0;
            this.BorderSelectedOpacity = 0;
            this.BorderAvailableOpacity = 0;

            this.IsClickable = false;

            this.BlockColor = new SolidColorBrush();
        }



        // https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=net-5.0
        // I can not get this to work, so I have resorted to reloading the entire collection each time something is changed, and left this in if I want to give it another try
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                Debug.WriteLine("TowerOfHanoiBlock: NotifyPropertyChanged() was called, but PropertyChanged was null");
            }

            // PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void NotifyCollectionChanged([CallerMemberName] String name = "")
        {
            CollectionChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Refresh, name));
        }




        /// <summary>
        /// Show the block on screen
        /// </summary>
        /// <param name="width">The desired width of this block</param>
        /// <param name="color">The desired color of this block</param>
        private void ShowBlock(double width, Windows.UI.Color color)
        {
            BlockWidth = Math.Clamp(width, 0, WidthBase);
            BlockOpacity = 1;
            BlockColor.Color = color;
            BorderSelectedOpacity = 0;
            BorderAvailableOpacity = 0;
            // NotifyPropertyChanged();
        }

        /// <summary>
        /// Set all opacities to 0, making elements no longer visible, and set IsClickable to false
        /// </summary>
        private void HideBlock()
        {
            BlockOpacity = 0;
            BorderSelectedOpacity = 0;
            BorderAvailableOpacity = 0;
            IsClickable = false;
            // NotifyPropertyChanged();
        }


        /// <summary>
        /// Display an outline of the block, showing that the block is currently selected
        /// </summary>
        /// <param name="width">The desired width of the border, will default to BlockWidth if empty</param>
        private void ShowSelected(double width = -1)
        {
            if (width >= 0)
            {
                BlockWidth = Math.Clamp(width, 0, WidthBase);
            }
            BorderSelectedOpacity = 1;
            // NotifyPropertyChanged();
        }


        /// <summary>
        /// Display an outline of the block, showing that this position is a valid move for the block the player has currently selected
        /// </summary>
        /// <param name="width"></param>
        private void ShowAvailable(double width)
        {
            BlockWidth = Math.Clamp(width, 0, WidthBase);
            BorderAvailableOpacity = 1;
            IsClickable = true;
            // NotifyPropertyChanged();
        }

        /// <summary>
        /// Hide any outlines at this position
        /// </summary>
        public void HideBorder()
        {
            BorderSelectedOpacity = 0;
            BorderAvailableOpacity = 0;
            // NotifyPropertyChanged();
        }


        // Change the state of this block based on a given value, which represents the enum BlockState in TowerOfHanoi, alignment is manual!
        public void SetStateEmpty()
        {
            HideBlock();
            BlockState = 0;
        }
        public void SetStateAvailable(double width)
        {
            ShowAvailable(width);
            BlockState = 1;
        }
        public void SetStateBlock(double width, Windows.UI.Color color)
        {
            ShowBlock(width, color);
            BlockState = 2;
        }
        public void SetStateSelected()
        {
            ShowSelected();
            BlockState = 3;
        }


    }
}
