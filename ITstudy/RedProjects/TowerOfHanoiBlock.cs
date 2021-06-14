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
        public int BlockNumber;
        public double Height;
        public double WidthBase;

        // Block state, representation of the enum BlockState in TowerOfHanoi
        public int BlockState;

        // Block values
        public double BlockWidth;
        public double BorderWidth;
        public double BlockOpacity;
        public double BorderSelectedOpacity;
        public double BorderAvailableOpacity;

        /// <summary>
        /// Is this block clickable by user
        /// </summary>
        public bool IsClickable = false;

        // Colour values
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
        public TowerOfHanoiBlock(string name, int blockNumber, double height, double widthBase, int blockState = 0)
        {
            this.BlockName = name;
            this.BlockNumber = blockNumber;
            this.Height = height;
            this.WidthBase = widthBase;

            this.BlockState = blockState;

            this.BlockWidth = 0;
            this.BorderWidth = 0;
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
        public void ShowBlock(double width, Windows.UI.Color color)
        {
            BlockWidth = Math.Clamp(width, 0, WidthBase);
            BlockOpacity = 1;
            BlockColor.Color = color;
            // NotifyPropertyChanged();
        }

        /// <summary>
        /// Set all widths of 0, making elements no longer visible
        /// </summary>
        public void HideBlock()
        {
            BlockOpacity = 0;
            BorderSelectedOpacity = 0;
            BorderAvailableOpacity = 0;
            // NotifyPropertyChanged();
        }


        /// <summary>
        /// Display an outline of the block, showing that the block is currently selected
        /// </summary>
        /// <param name="width">The desired width of the border, will default to BlockWidth if empty</param>
        public void ShowSelected(double width = -1)
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
        public void ShowAvailable(double width)
        {
            BorderWidth = Math.Clamp(width, 0, WidthBase);
            BorderAvailableOpacity = 1;
            // NotifyPropertyChanged();
        }

        /// <summary>
        /// Hide the outline at this position
        /// </summary>
        public void HideBorder()
        {
            BorderSelectedOpacity = 0;
            BorderAvailableOpacity = 0;
            // NotifyPropertyChanged();
        }


        // Change the state of this block based on a given value, which represents the enum BlockState in TowerOfHanoi
        public void SetBlockState(int blockState)
        {
            if (BlockState == blockState) { return; }

            BlockState = blockState;

            // Change the state of the block, this is lined-up by hand to the values of enum BlockState, so any changes there will have to be applied here, manually, as well
            switch (blockState)
            {
                // Empty
                case 0:
                    {
                        HideBlock();
                        break;
                    }
                // Available
                case 1:
                    {
                        // Requires arguments
                        break;
                    }
                // Block
                case 2:
                    {
                        // Requires arguments
                        break;
                    }
                // Selected
                case 3:
                    {
                        ShowSelected();
                        break;
                    }

            }
        }


    }
}
