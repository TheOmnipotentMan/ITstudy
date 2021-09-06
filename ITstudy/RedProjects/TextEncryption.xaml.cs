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


using System.Security.Cryptography;
using Windows.Storage;


// TODO remove if the app is ever finished
using System.Diagnostics;






namespace ITstudy.RedProjects
{

    /// <summary>
    /// A project in Text Encryption.
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





        /*  Source material, resources
         * Heavily based on https://docs.microsoft.com/en-us/dotnet/standard/security/walkthrough-creating-a-cryptographic-application
         * 
         */







        CspParameters CspP = new CspParameters();
        RSACryptoServiceProvider Rsa;

        const string SourceFolder = "ms - appx:///Assets\\TextEncryption\\Source\\";
        const string EncryptedFolder = "ms-appx:///Assets\\TextEncryption\\Encrypted\\";
        const string DecryptedFolder = "ms-appx:///Assets\\TextEncryption\\Decrypted\\";
        const string PublicKeyFile = "ms-appx:///Assets\\TextEncryption\\PublicKey.txt";

        string KeyName = "Key01";

        Windows.Storage.Pickers.FileOpenPicker FilePicker;


        string CurrentFilePath = null;



        /// <summary>
        /// Types of message that can be sent to DisplayInfo
        /// </summary>
        private enum InfoType { Normal, Warning, Error, Succes }
        private InfoType InfoTypeLastUsed = 0;
        private SolidColorBrush InfoBrush = new SolidColorBrush(Windows.UI.Colors.Black);





        public TextEncryption()
        {
            this.InitializeComponent();

            SetFilePicker();
            
        }








        /// <summary>
        /// Set the paths to the folders and files relevant to TextEnryption
        /// </summary>
        private void SetFilePicker()
        {
            FilePicker = new Windows.Storage.Pickers.FileOpenPicker();
            FilePicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            FilePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            FilePicker.FileTypeFilter.Add(".txt");
            FilePicker.FileTypeFilter.Add(".enc");
        }


        private void CreateAsymKeys()
        {
            CspP.KeyContainerName = KeyName;
            Rsa = new RSACryptoServiceProvider(CspP);
            Rsa.PersistKeyInCsp = true;
            DisplayKeyType();
        }


        private async void LocateFile()
        {

            if (Rsa == null)
            {
                DisplayInfo("Please create a key first", InfoType.Warning);
            }
            else
            {
                // Await for the user to pick a file
                Windows.Storage.StorageFile file = await FilePicker.PickSingleFileAsync();
                // If no file was picked
                if (file == null)
                {
                    Debug.WriteLine($"TextEncryption: LocateFileToEncrypt() no file specified, FilePicker canceled");
                }
                else if (file.FileType == ".txt")
                {
                    Debug.WriteLine($"TextEncryption: LocateFileToEncrypt() located {file.Name} at {file.Path}");
                    // Load the contents of the selected file to the input field
                    EncryptionInputTextBox.Text = await FileIO.ReadTextAsync(file);
                    CurrentFilePath = file.Path;
                    EnableEncryption(true);
                    DisplayInfo(file.Name + " opened");
                }
                else if (file.FileType == ".enc")
                {
                    Debug.WriteLine($"TextEncryption: LocateFileToEncrypt() located {file.Name} at {file.Path}");
                    EncryptionInputTextBox.Text = await FileIO.ReadTextAsync(file);
                    CurrentFilePath = file.Path;
                    EnableEncryption(false);
                    DisplayInfo(file.Name + " opened");
                }
                // If the file picked was not a .txt or .enc file
                else
                {
                    DisplayInfo($"Failed to read the selected file {file.Name}, file must be a .txt or .enc file", InfoType.Error);
                }

                // TEST, TODO remove
                string testPath = "not ";
                if (File.Exists(file.Path)) { testPath = ""; }
                Debug.WriteLine($"TextEncryption: LocateFile() File does {testPath}exist");
            }
        }


        /// <summary>
        /// Toggle between Encryption and decryption. Set true for encryption, false for decryption.
        /// Null to disable both.
        /// </summary>
        /// <param name="state"></param>
        private void EnableEncryption(bool? state)
        {
            if (state == true)
            {
                EncryptFileButton.IsEnabled = true;
                DecryptFileButton.IsEnabled = false;
            }
            else if (state == false)
            {
                EncryptFileButton.IsEnabled = false;
                DecryptFileButton.IsEnabled = true;
            }
            else
            {
                EncryptFileButton.IsEnabled = false;
                DecryptFileButton.IsEnabled = false;
            }
        }


        /// <summary>
        /// Encrypt a .txt file with Aes.
        /// Largely copied from https://docs.microsoft.com/en-us/dotnet/standard/security/walkthrough-creating-a-cryptographic-application
        /// </summary>
        /// <param name="inFile"></param>
        private void EncryptAes(string inFile)
        {
            // Ensure the input is valid
            if (string.IsNullOrEmpty(inFile))
            {
                DisplayInfo("Please provide some text", InfoType.Warning);
                return;
            }
            else if (!File.Exists(inFile))
            {
                DisplayInfo("File could not be found", InfoType.Error);
                Debug.WriteLine($"TextEncryption: EncryptAes() file not found, {inFile}");
                return;
            }
            else if (inFile.Substring(inFile.Length - 4, 4) != ".txt")
            {
                DisplayInfo("File type not supported for encryption", InfoType.Error);
                return;
            }

            // TEST return, TODO remove
            else
            {
                Debug.WriteLine($"TextEncryption: EncryptAes() succes");
                return;
            }


            // Create an instance of Aes for symmetric encryption of the data
            Aes aes = Aes.Create();
            ICryptoTransform transform = aes.CreateEncryptor();

            // Use RSACryptoServiceProvider to encrypt the AES key.
            byte[] keyEncrypted = Rsa.Encrypt(aes.Key, false);

            // Create byte arrays that contain the length values of the key and IV
            byte[] lengthKey = new byte[4];
            byte[] lengthIV = new byte[4];
            lengthKey = BitConverter.GetBytes(keyEncrypted.Length);
            lengthIV = BitConverter.GetBytes(aes.IV.Length);
                        
            // Set up the out file with the extension ".enc"
            string outFile = EncryptedFolder + inFile.Substring(0, inFile.LastIndexOf(".")) + ".enc";

            // Write the following to the FileStream for the encrypted file (outFS)
            // - length of the key
            // - length of the IV
            // - enrypted key
            // - the IV
            // - the encrypted cypher content
            using (FileStream outFS = new FileStream(outFile, FileMode.Create))
            {
                outFS.Write(lengthKey, 0, 4);
                outFS.Write(lengthIV, 0, 4);
                outFS.Write(keyEncrypted, 0, keyEncrypted.Length);
                outFS.Write(aes.IV, 0, aes.IV.Length);

                // Write the cipher text using a cryptostream for encrypting
                using (CryptoStream outStreamEncr = new CryptoStream(outFS, transform, CryptoStreamMode.Write))
                {
                    // Read from input, in blockSizeBytes 'bites', and pass that data to outStreamEncr which writes it to the encrypted outFS file.
                    // By encrypting a chunk at a time, you can save memory and accommodate large files

                    int count = 0;
                    int offset = 0;                    
                    // blockSizeBytes can be any arbitrary size
                    int blockSizeBytes = aes.BlockSize / 8;
                    byte[] data = new byte[blockSizeBytes];
                    int bytesRead = 0;

                    using (FileStream  inFS = new FileStream(inFile, FileMode.Open))
                    {
                        do
                        {
                            count = inFS.Read(data, 0, blockSizeBytes);
                            offset += count;
                            outStreamEncr.Write(data, 0, count);
                            bytesRead += blockSizeBytes;
                        }
                        while (count > 0);

                        inFS.Close();
                    }

                    outStreamEncr.FlushFinalBlock();
                    outStreamEncr.Close();
                }

                outFS.Close();
            }
        }


        /// <summary>
        /// Decrypt a text file that was encrypted with Aes.
        /// Largely copied from h
        /// Largely copied from https://docs.microsoft.com/en-us/dotnet/standard/security/walkthrough-creating-a-cryptographic-application
        /// </summary>
        /// <param name="inFile"></param>
        private void DecryptAes(string inFile)
        {
            // Ensure the input is valid
            if (string.IsNullOrEmpty(inFile))
            {
                DisplayInfo("Please provide a .enc file", InfoType.Warning);
                return;
            }
            else if (!File.Exists(inFile))
            {
                DisplayInfo("File could not be found", InfoType.Error);
                return;
            }
            else if (inFile.Substring(inFile.Length - 4, 4) != ".enc")
            {
                DisplayInfo("File type not supported for decryption", InfoType.Error);
                return;
            }

            // Create an instance of Aes for symmetric decryption of the data
            Aes aes = Aes.Create();

            // Create byte arrays to get the length of the encrypted key and IV.
            // These values were stored as 4 bytes each at the beginning of the encrypted message
            byte[] lengthKey = new byte[4];
            byte[] lengthIV = new byte[4];

            // Construct the file name for the decrypted file
            string outFile = DecryptedFolder + inFile.Substring(0, inFile.LastIndexOf(".")) + ".txt";

            // Use FileStream objects to read the encrypted file (inFS) and save to the decrypted file (outFS)
            // Warning, FileStream input fields do not match those used in https://docs.microsoft.com/en-us/dotnet/standard/security/walkthrough-creating-a-cryptographic-application
            using (FileStream inFS = new FileStream(inFile, FileMode.Open))
            {
                // Read the lengths of the key and IV (why is Read() count 3 and not 4?)
                inFS.Seek(0, SeekOrigin.Begin);
                inFS.Read(lengthKey, 0, 3);
                inFS.Seek(4, SeekOrigin.Begin);
                inFS.Read(lengthIV, 0, 3);

                // Convent the lengths to integer values
                int lenKey = BitConverter.ToInt32(lengthKey, 0);
                int lenIV = BitConverter.ToInt32(lengthIV, 0);

                // Determine the start position of the cipher text (startC) and its length (lenC)
                int startC = lenKey + lenIV + 8;
                int lenC = (int)inFS.Length - startC;

                // Create the byte arrays for the encrypted Aes key, the IV and the cipher text
                byte[] keyEncrypted = new byte[lenKey];
                byte[] iV = new byte[lenIV];

                // Extract the key and IV starting from index 8 (ie after the length values)
                inFS.Seek(8, SeekOrigin.Begin);
                inFS.Read(keyEncrypted, 0, lenKey);
                inFS.Seek(8 + lenKey, SeekOrigin.Begin);
                inFS.Read(iV, 0, lenIV);

                // Create a folder for decrypted files, if it does not already exist
                Directory.CreateDirectory(DecryptedFolder);

                // Use RSACryptoServiceProvider to drcrypt the AES key
                byte[] keyDecrypted = Rsa.Decrypt(keyEncrypted, false);

                // Decrypt the key
                ICryptoTransform transform = aes.CreateDecryptor(keyDecrypted, iV);

                // Decrypt the cipher text from the FileStream of the encrypted file (inFS) inot the FileStream for the decrypted file (outFS)
                using (FileStream outFS = new FileStream(outFile, FileMode.Create))
                {
                    int count = 0;
                    int offset = 0;
                    // blockSizeBytes can be any arbitrary size
                    int blockSizeBytes = aes.BlockSize / 8;
                    byte[] data = new byte[blockSizeBytes];

                    // Start at hte beginning of the cipher text
                    inFS.Seek(startC, SeekOrigin.Begin);
                    using (CryptoStream outStreamDecrypted = new CryptoStream(outFS, transform, CryptoStreamMode.Write))
                    {
                        do
                        {
                            count = inFS.Read(data, 0, blockSizeBytes);
                            offset += count;
                            outStreamDecrypted.Write(data, 0, count);
                        }
                        while (count > 0);

                        outStreamDecrypted.FlushFinalBlock();
                        outStreamDecrypted.Close();
                    }

                    outFS.Close();
                }

                inFS.Close();
            }
        }


        /// <summary>
        /// Save the public key created by the RSA to a file.
        /// Caution, persisting the key to a file is a security risk
        /// </summary>
        private void ExportPublicKey()
        {
            Directory.CreateDirectory(EncryptedFolder);
            StreamWriter sw = new StreamWriter(PublicKeyFile, false);
            sw.Write(Rsa.ToXmlString(false));
            sw.Close();
        }


        /// <summary>
        /// Import a public key from a file
        /// This simulates recieving a key from someone else so you could encrypt a file for them
        /// </summary>
        private void ImportPublicKey()
        {
            StreamReader sr = new StreamReader(PublicKeyFile);
            CspP.KeyContainerName = KeyName;
            Rsa = new RSACryptoServiceProvider(CspP);
            string keytxt = sr.ReadToEnd();
            Rsa.FromXmlString(keytxt);
            Rsa.PersistKeyInCsp = true;
            DisplayKeyType();
            sr.Close();
        }


        /// <summary>
        /// Set the KeyContainerName to the KeyName created by CreateAsymKeys()
        /// This simulates using your private key to decrypt files someone else has encrypted for you with your public key
        /// (and is atm just identical to CreateAsymKeys() since KeyName never changes)
        /// </summary>
        private void GetPrivateKey()
        {
            CspP.KeyContainerName = KeyName;

            Rsa = new RSACryptoServiceProvider(CspP);
            Rsa.PersistKeyInCsp = true;

            DisplayKeyType();
        }












        /// <summary>
        /// Display some text to the EncryptionInfoTextBlock to keep the user informed what is happening
        /// </summary>
        /// <param name="text">Text to display</param>
        /// <param name="type">Type of message</param>
        private void DisplayInfo(string text, InfoType type = InfoType.Normal)
        {
            if (type != InfoTypeLastUsed)
            {
                InfoTypeLastUsed = type;
                switch (type)
                {
                    case InfoType.Normal:
                        {
                            InfoBrush.Color = Windows.UI.Colors.Black;
                            break;
                        }
                    case InfoType.Warning:
                        {
                            InfoBrush.Color = Windows.UI.Colors.Orange;
                            break;
                        }
                    case InfoType.Error:
                        {
                            InfoBrush.Color = Windows.UI.Colors.Red;
                            break;
                        }
                    case InfoType.Succes:
                        {
                            InfoBrush.Color = Windows.UI.Colors.Green;
                            break;
                        }
                    default:
                        {
                            Debug.WriteLine($"TextEncryption: DisplayInfo() has no case for InfoType {type}");
                            break;
                        }
                }
            }

            EncryptionInfoTextBlock.Text = text;
        }


        private void DisplayKeyType()
        {
            if (Rsa.PublicOnly)
            {
                KeyStatusTextBlock.Text = "Key: " + CspP.KeyContainerName + " - Public Only";
            }
            else
            {
                KeyStatusTextBlock.Text = "Key: " + CspP.KeyContainerName + " - Full Key Pair";
            }
        }












        private void CreateAsymKeysButton_Click(object sender, RoutedEventArgs e)
        {
            CreateAsymKeys();
        }

        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            LocateFile();
        }

        private void EncryptFileButton_Click(object sender, RoutedEventArgs e)
        {
            EncryptAes(CurrentFilePath);
        }

        private void DecryptFileButton_Click(object sender, RoutedEventArgs e)
        {
            DecryptAes(CurrentFilePath);
        }

        private void ExportPublicKeyButton_Click(object sender, RoutedEventArgs e)
        {
            ExportPublicKey();
        }

        private void ImportPublicKeyButton_Click(object sender, RoutedEventArgs e)
        {
            ImportPublicKey();
        }

        private void GetPrivateKeyButton_Click(object sender, RoutedEventArgs e)
        {
            GetPrivateKey();
        }

        private void ClearInputButton_Click(object sender, RoutedEventArgs e)
        {
            EnableEncryption(null);
            EncryptionInputTextBox.Text = string.Empty;
            CurrentFilePath = null;
            DisplayInfo("Input cleared");
        }
    }
}
