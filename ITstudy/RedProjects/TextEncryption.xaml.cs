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
    /// This project is not actually 'secure', nor is it meant to. So please do not use it for actual encryption. It is only a learning experience and demonstration
    /// </summary>
    public sealed partial class TextEncryption : Page
    {

        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "20:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "6";
        // Date when this project was finished
        string ProjectDateFinished = "00/00/00";





        /*  Source material, resources
         * Based on https://docs.microsoft.com/en-us/dotnet/standard/security/walkthrough-creating-a-cryptographic-application
         * 
         */







        private CspParameters CspP = new CspParameters();
        private RSACryptoServiceProvider Rsa;

        // Provided, optional, locations for files (relic from source material, can be removed, but provides a nice folder/file location for this projects in- and output, if the user wants to use it) 
        private const string SourcePath = "Assets\\TextEncryption\\Source\\";
        private const string EncryptedPath = "Assets\\TextEncryption\\Encrypted\\";
        private const string DecryptedPath = "Assets\\TextEncryption\\Decrypted\\";
        private const string PublicKeyPath = "Assets\\TextEncryption\\PublicKey.txt";
        private Uri SourceDir = new Uri("ms-appx:///" + "Assets\\TextEncryption\\Source\\");
        private Uri EncryptedDir = new Uri("ms-appx:///" + "Assets\\TextEncryption\\Encrypted\\");
        private Uri DecryptedDir = new Uri("ms-appx:///" + "Assets\\TextEncryption\\Decrypted\\");
        private Uri PublicKeyFile = new Uri("ms-appx:///" + "Assets\\TextEncryption\\PublicKey.txt");

        private string KeyName = "Key01";
                
        private Windows.Storage.Pickers.FileOpenPicker FilePicker;
        private Windows.Storage.Pickers.FileSavePicker FileSaverTxt;
        private Windows.Storage.Pickers.FileSavePicker FileSaverEnc;

        private StorageFile CurrentFile = null;
        
        /// <summary>
        /// File types supported, available
        /// </summary>
        private enum FileType { txt, enc }


        /// <summary>
        /// Types of message that can be sent to DisplayInfo
        /// </summary>
        private enum InfoType { Normal, Warning, Error, Succes }
        private InfoType InfoTypeLastUsed = 0;
        private SolidColorBrush InfoBrush = new SolidColorBrush(Windows.UI.Colors.Black);





        public TextEncryption()
        {
            this.InitializeComponent();

            FinishSetup();
            
        }








        /// <summary>
        /// Set the paths to the folders and files relevant to TextEnryption
        /// </summary>
        private void FinishSetup()
        {
            FilePicker = new Windows.Storage.Pickers.FileOpenPicker();
            FilePicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            FilePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            FilePicker.FileTypeFilter.Add(".txt");
            FilePicker.FileTypeFilter.Add(".enc");

            FileSaverTxt = new Windows.Storage.Pickers.FileSavePicker();
            FileSaverTxt.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            FileSaverTxt.FileTypeChoices.Add("Text", new List<string>() { ".txt" });

            FileSaverEnc = new Windows.Storage.Pickers.FileSavePicker();
            FileSaverEnc.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            FileSaverEnc.FileTypeChoices.Add("Encrypted", new List<string>() { ".enc" });

            Debug.WriteLine($"TextEncryption: FinishSetup() SourceFolder.LocalPath = {SourceDir.LocalPath}");
            string missingItems = "";
            if (!Directory.Exists(SourcePath)) { missingItems += ", Source"; }
            if (!Directory.Exists(EncryptedPath)) { missingItems += ", Encrypted"; }
            if (!Directory.Exists(DecryptedPath)) { missingItems += ", Decrypted"; }
            if (!File.Exists(PublicKeyPath)) { missingItems += ", PublicKey"; }
            Debug.WriteLine($"TextEncryption: FinishSetup() missingItems{missingItems}");

        }


        /// <summary>
        /// Create default asymmetric keys, using the value of KeyName
        /// </summary>
        private void CreateAsymKeys()
        {
            CspP.KeyContainerName = KeyName;
            Rsa = new RSACryptoServiceProvider(CspP);
            Rsa.PersistKeyInCsp = true;
            DisplayKeyType();
        }


        /// <summary>
        /// Let the user locate a file to encrypt or decrypt. File is assigned to the StorageFile CurrentFile
        /// </summary>
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
                    CurrentFile = file;
                    EnableEncryption(true);
                    DisplayInfo(file.Name + " opened");
                }
                else if (file.FileType == ".enc")
                {
                    Debug.WriteLine($"TextEncryption: LocateFileToEncrypt() located {file.Name} at {file.Path}");
                    EncryptionInputTextBox.Text = "Encrypted data...";
                    CurrentFile = file;
                    EnableEncryption(false);
                    DisplayInfo(file.Name + " opened");
                }
                // If the file picked was not a .txt or .enc file
                else
                {
                    DisplayInfo($"Failed to read the selected file {file.Name}, file must be a .txt or .enc file", InfoType.Error);
                }
            }
        }
        

        /// <summary>
        /// Encrypt a .txt file with Aes.
        /// Based on https://docs.microsoft.com/en-us/dotnet/standard/security/walkthrough-creating-a-cryptographic-application
        /// </summary>
        /// <param name="inFile"></param>
        private async void EncryptAes(StorageFile inFile)
        {
            // Ensure a key has been generated, redundant
            if (Rsa == null)
            {
                DisplayInfo("Please create a key first", InfoType.Warning);
                return;
            }

            // Ensure the input is valid
            if (inFile == null)
            {
                DisplayInfo("Please provide some text", InfoType.Warning);
                return;
            }
            else if (inFile.FileType != ".txt")
            {
                DisplayInfo("File type not supported for encryption", InfoType.Error);
                return;
            }
            
            // Determine how and where to save the output file
            StorageFile outFile = null;
            FileSaverEnc.SuggestedFileName = inFile.DisplayName;
            outFile = await FileSaverEnc.PickSaveFileAsync();
            if (outFile == null)
            {
                DisplayInfo("Output location must be chosen before file can be encrypted.", InfoType.Warning);
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

            // Convert the StorageFile outFile to a writable stream https://blog.mzikmund.com/2020/01/how-to-stream-ify-a-uwp-storagefile/
            var outHandle = outFile.CreateSafeFileHandle(options: FileOptions.RandomAccess);
            var inHandle = inFile.CreateSafeFileHandle(options: FileOptions.RandomAccess);

            // Write the following to the FileStream for the encrypted file (outFS)
            // - length of the key
            // - length of the IV
            // - encrypted key
            // - the IV
            // - the encrypted cypher content
            using (FileStream outFS = new FileStream(outHandle, FileAccess.Write))
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

                    using (FileStream inFS = new FileStream(inHandle, FileAccess.Read))
                    {
                        Debug.WriteLine($"TextEncryption: EncryptAes() FileStream inFS started");
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

            // Display the 'result' on screen
            DisplayInfo("File encrypted");
            EncryptionInputTextBox.Text = "Encrypted data...";

            // Disable encrypting and decrypting until a new file is loaded
            EnableEncryption(null);
        }


        /// <summary>
        /// Decrypt a text file that was encrypted with Aes.
        /// Based on https://docs.microsoft.com/en-us/dotnet/standard/security/walkthrough-creating-a-cryptographic-application
        /// </summary>
        /// <param name="inFile"></param>
        private async void DecryptAes(StorageFile inFile)
        {
            // Ensure a key has been generated, redundant
            if (Rsa == null)
            {
                DisplayInfo("Please create a key first", InfoType.Warning);
                return;
            }

            // Ensure the input is valid
            if (inFile == null)
            {
                DisplayInfo("Please provide a .enc file", InfoType.Warning);
                return;
            }
            else if (inFile.FileType != ".enc")
            {
                DisplayInfo("File type not supported for decryption", InfoType.Error);
                return;
            }

            // Determine how and where to save the output file
            StorageFile outFile = null;
            FileSaverTxt.SuggestedFileName = inFile.DisplayName;
            outFile = await FileSaverTxt.PickSaveFileAsync();
            if (outFile == null)
            {
                DisplayInfo("Output location must be chosen before file can be encrypted.", InfoType.Warning);
                return;
            }

            // Create an instance of Aes for symmetric decryption of the data
            Aes aes = Aes.Create();

            // Create byte arrays to get the length of the encrypted key and IV.
            // These values were stored as 4 bytes each at the beginning of the encrypted message
            byte[] lengthKey = new byte[4];
            byte[] lengthIV = new byte[4];

            // Convert the StorageFile outFile to a writable stream
            var inHandle = inFile.CreateSafeFileHandle(options: FileOptions.RandomAccess);
            var outHandle = outFile.CreateSafeFileHandle(options: FileOptions.RandomAccess);

            // Use FileStream objects to read the encrypted file (inFS) and save to the decrypted file (outFS)
            // Warning, FileStream input fields do not match those used in https://docs.microsoft.com/en-us/dotnet/standard/security/walkthrough-creating-a-cryptographic-application
            using (FileStream inFS = new FileStream(inHandle, FileAccess.Read))
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

                // Use RSACryptoServiceProvider to decrypt the AES key
                byte[] keyDecrypted;
                try
                {
                    keyDecrypted = Rsa.Decrypt(keyEncrypted, false);
                }
                // Catch any errors, return if any, decryption is not possible
                catch (Exception e)
                {
                    Debug.WriteLine($"TextEncryption: DecryptAes() failed to obtain keyDecrypted, stopping decryption. {e}");
                    DisplayInfo("Unable to decrypt", InfoType.Error);
                    return;
                }

                // Decrypt the key
                ICryptoTransform transform = aes.CreateDecryptor(keyDecrypted, iV);

                // Decrypt the cipher text from the FileStream of the encrypted file (inFS) to the FileStream for the decrypted file (outFS)
                using (FileStream outFS = new FileStream(outHandle, FileAccess.Write))
                {
                    int count = 0;
                    int offset = 0;
                    // blockSizeBytes can be any arbitrary size
                    int blockSizeBytes = aes.BlockSize / 8;
                    byte[] data = new byte[blockSizeBytes];

                    // Start at the beginning of the cipher text
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

            // Display the result on screen
            DisplayInfo("File decrypted");
            EncryptionInputTextBox.Text = await FileIO.ReadTextAsync(outFile);

            // Disable encrypting and decrypting until a new file is loaded
            EnableEncryption(null);
        }


        /// <summary>
        /// Save the public key created by the RSA to a file.
        /// Persisting the key to a file is a security risk! Here for demonstration purposes
        /// </summary>
        private async void ExportPublicKey()
        {
            // Ensure a key has been generated
            if (Rsa == null)
            {
                DisplayInfo("Please create a key first", InfoType.Warning);
                return;
            }

            FileSaverTxt.SuggestedFileName = "PublicKey";
            StorageFile file = null;
            file = await FileSaverTxt.PickSaveFileAsync();
            if (file == null)
            {
                DisplayInfo("Exporting public key canceled");
            }
            else
            {
                /* Step by step (testing) version of obtaining the string 'key'
                byte[] keyBytes = Rsa.ExportCspBlob(false);
                Debug.WriteLine($"TextEncryption: ExportPublicKey() Rsa CspBlob length = {keyBytes.Length}, dimensions = {keyBytes.Rank}");
                char[] keyChars = keyBytes.Select(b => (char)b).ToArray();
                string key = System.Text.Encoding.UTF8.GetString(keyBytes);
                Debug.WriteLine($"TextEncryption: ExportPublicKey() Rsa CspBlob string = {key}, length = {key.Length}");
                */

                // Get a string representation of the public key
                string key = new string(Rsa.ExportCspBlob(false).Select(b => (char)b).ToArray());

                // https://docs.microsoft.com/en-us/windows/uwp/files/quickstart-reading-and-writing-files
                await FileIO.WriteTextAsync(file, key);

                /* Alternative, write to file using a stream
                var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                using (var outStream = fileStream.GetOutputStreamAt(0))
                {
                    using (var dataWriter = new Windows.Storage.Streams.DataWriter(outStream))
                    {
                        dataWriter.WriteString(key);
                        await dataWriter.StoreAsync();
                        await outStream.FlushAsync();
                    }
                }
                fileStream.Dispose();
                */
            }
        }


        /// <summary>
        /// Import a public key from a file
        /// This simulates recieving a key from someone else so you could encrypt a file for them
        /// </summary>
        private async void ImportPublicKey()
        {
            // Ensure Rsa has been instantiated
            if (Rsa == null)
            {
                Rsa = new RSACryptoServiceProvider(CspP);
            }

            // Get a file
            StorageFile file = await FilePicker.PickSingleFileAsync();

            // Ensure the input is valid
            if (file == null)
            {
                DisplayInfo("Please provide some text", InfoType.Warning);
                return;
            }
            else if (file.FileType != ".txt")
            {
                DisplayInfo("File type not supported for importing key", InfoType.Error);
                return;
            }

            string key = await FileIO.ReadTextAsync(file);
            try
            {
                Rsa.ImportCspBlob(key.ToCharArray().Select(c => (byte)c).ToArray());
            }
            catch (Exception e)
            {
                Debug.WriteLine($"TextEncryption: ImportPublicKey() failed to import public key. {e}");
                DisplayInfo("Failed to import public key", InfoType.Error);
                return;
            }
            
            DisplayKeyType();
        }


        /// <summary>
        /// Set the KeyContainerName to the KeyName created by CreateAsymKeys()
        /// This simulates using your private key to decrypt files someone else has encrypted for you with your public key
        /// (and is atm just identical to CreateAsymKeys() since KeyName never changes)
        /// </summary>
        private void SetCustomKeys(string password)
        {
            CspP.KeyContainerName = password;

            Rsa = new RSACryptoServiceProvider(CspP);
            Rsa.PersistKeyInCsp = true;

            DisplayKeyType();

            Debug.WriteLine($"TextEncryption: SetCustomKeys() new keys set. password = {password}");
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


        /// <summary>
        /// Display the current state of the key(s). Either 'Public only' is only a public key is present, or 'Full pair' if both a public and private key are present
        /// </summary>
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
            EncryptAes(CurrentFile);
        }

        private void DecryptFileButton_Click(object sender, RoutedEventArgs e)
        {
            DecryptAes(CurrentFile);
        }

        private void ExportPublicKeyButton_Click(object sender, RoutedEventArgs e)
        {
            ExportPublicKey();
        }

        private void ImportPublicKeyButton_Click(object sender, RoutedEventArgs e)
        {
            ImportPublicKey();
        }

        private async void GetPrivateKeyButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await CustomKeysContentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(CustomPasswordTextBox.Text))
            {
                SetCustomKeys(CustomPasswordTextBox.Text);
            }
        }

        private void ClearInputButton_Click(object sender, RoutedEventArgs e)
        {
            EnableEncryption(null);
            EncryptionInputTextBox.Text = string.Empty;
            CurrentFile = null;
            DisplayInfo("Input cleared");
        }
    }
}
