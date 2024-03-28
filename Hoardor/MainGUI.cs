using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Hoardor.MainGUI;




namespace Hoardor
{
    public partial class MainGUI : Form
    {
        private bool fileToUpload = false;
        private string fileToUploadPath = "";
        private bool isUploading = false;
        private int packetSize = 256000;
        private string HoardorMasterKey = "b65af5a104f97cfd07c1969aaaaeee8d"; // Change this to your own key Do it for client and server.
        private string HoardorSecretKey = "d8d50b33"; //This is the secret key for the client
        private string HoardorServerIP = "127.0.0.1"; // Change this to your server IP
        private bool isCompessing = false;
        public MainGUI()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += MainGUI_DragEnter;
            this.DragDrop += MainGUI_DragDrop;
            uploadTargetFile.Enabled = false;
        }

        // Event handler for when a file is dragged into the form
        private void MainGUI_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        // Event handler for when a file is dropped into the form
        private void MainGUI_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (isUploading)
                {
                    return; // Already uploading, ignore the request
                }
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                ToggleButton();
                ToggleUploadButtonAccess(true);
                fileToUploadPath = files[0];

                if (File.Exists(fileToUploadPath))
                {
                    if (!CheckExistingZipFile(fileToUploadPath))
                    {
                        string compressedFilePath = Path.ChangeExtension(fileToUploadPath, ".zip");
                        CompressFile(fileToUploadPath, compressedFilePath);
                        SystemLog($"SendingFile {fileToUploadPath} compressed to {compressedFilePath}.");
                    }
                }
            }
        }

        // Toggle the state of the upload button
        public void ToggleUploadButtonAccess(bool access)
        {
            uploadTargetFile.Enabled = access;
        }

        // Toggle the state of the file upload flag and update the upload button text
        public void ToggleButton()
        {
            if (fileToUpload)
            {
                fileToUpload = false;
                uploadTargetFile.Text = "Upload";
            }
            else
            {
                fileToUpload = true;
                uploadTargetFile.Text = "Abort";
            }
        }

        // Event handler for the upload button click
        private void uploadTargetFile_Click(object sender, EventArgs e)
        {
            // TODO: Implement upload functionality
        }

        // Update the file path to be uploaded
        public void UpdateFileToUploadPath(string path)
        {
            fileToUploadPath = path;
        }

        // Compress the source file and upload it to the server
        public void CompressFile(string sourceFilePath, string compressedFilePath)
        {
            // Use ThreadPool to run the compression and upload tasks
            ThreadPool.SetMaxThreads(48, 48); // Set the maximum number of threads in the ThreadPool
            ThreadPool.QueueUserWorkItem(state =>
            {
                SystemLog($"Compressing...");
                using (ZipArchive archive = ZipFile.Open(compressedFilePath, ZipArchiveMode.Create))
                {
                    isCompessing = true;
                    archive.CreateEntryFromFile(sourceFilePath, Path.GetFileName(sourceFilePath));

                }
                UploadFileToServer(compressedFilePath);
            });

            
        }

        // Upload the file to the server
        public void UploadFileToServer(string filePath)
        {
            if (isUploading)
            {
                return; // Already uploading, ignore the request
            }

            isCompessing= false;
            isUploading = true;

            // Read the file into packets
            byte[] fileBytes = File.ReadAllBytes(filePath);

            int totalPackets = (int)Math.Ceiling((double)fileBytes.Length / packetSize);

            // Get the file name
            string fileName = Path.GetFileName(filePath);

            // Connect to the server
            try
            {
                using (TcpClient client = new TcpClient(HoardorServerIP, 4242))
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        // Send the key to the server
                        string key = OpSec.Encrypt(HoardorSecretKey, HoardorMasterKey);
                        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                        byte[] keyLengthBytes = BitConverter.GetBytes(keyBytes.Length);
                        stream.Write(keyLengthBytes, 0, keyLengthBytes.Length);
                        stream.Write(keyBytes, 0, keyBytes.Length);

                        // Send the file name to the server
                        byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                        byte[] fileNameLengthBytes = BitConverter.GetBytes(fileNameBytes.Length);
                        stream.Write(fileNameLengthBytes, 0, fileNameLengthBytes.Length);
                        stream.Write(fileNameBytes, 0, fileNameBytes.Length);

                        // Send the total number of packets to the server
                        byte[] totalPacketsBytes = BitConverter.GetBytes(totalPackets);
                        stream.Write(totalPacketsBytes, 0, totalPacketsBytes.Length);

                        // Send the file packets to the server
                        for (int packetIndex = 0; packetIndex < totalPackets; packetIndex++)
                        {
                            int offset = packetIndex * packetSize;
                            int remainingBytes = fileBytes.Length - offset;
                            int packetLength = Math.Min(packetSize, remainingBytes);

                            // Create the packet
                            byte[] packet = new byte[packetLength];
                            Buffer.BlockCopy(fileBytes, offset, packet, 0, packetLength);

                            // Send the packet to the server
                            stream.Write(packet, 0, packet.Length);

                            // Update progress bar
                            int progressPercentage = (int)(((double)packetIndex + 1) / totalPackets * 100);
                            progressUpload.Invoke((MethodInvoker)(() => progressUpload.Value = progressPercentage));

                            // Log the packet send
                            SystemLog($"Sent packet {packetIndex + 1} of {totalPackets} to the server.");
                        }
                    }
                }
                SystemLog("Complete! " + fileName + " has been uploaded to the server.");
                isUploading = false;

                // Delete the zip file after upload is done
                DeleteZipFile(filePath);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("The server is offline.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SystemLog("[!]Error: The server is offline or Not Responding. (TIMED-OUT).");
                isUploading = false;
            }
        }

        // Log a system message to the UI log
        public void SystemLog(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logEntry = $"{timestamp} - {message}";

            if (HoardorLog.InvokeRequired)
            {
                HoardorLog.Invoke(new MethodInvoker(delegate
                {
                    HoardorLog.Items.Add(logEntry);
                    HoardorLog.TopIndex = HoardorLog.Items.Count - 1; // Scroll to the last item
                }));
            }
            else
            {
                HoardorLog.Items.Add(logEntry);
                HoardorLog.TopIndex = HoardorLog.Items.Count - 1; // Scroll to the last item
            }
        }

        // Log a system message to the UI log using Invoke
        public void SystemLogWithInvoke(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logEntry = $"{timestamp} - {message}";

            HoardorLog.Invoke((MethodInvoker)(() =>
            {
                HoardorLog.Items.Add(logEntry);
            }));
        }

        // Check if a zip file with the same name already exists
        private bool CheckExistingZipFile(string filePath)
        {
            string zipFilePath = Path.ChangeExtension(filePath, ".zip");
            if (File.Exists(zipFilePath))
            {
                DialogResult result = MessageBox.Show("A zip file with the same name already exists. Do you want to delete it?", "File Exists", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    File.Delete(zipFilePath);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        // Delete the zip file
        public void DeleteZipFile(string filePath)
        {
            string zipFilePath = Path.ChangeExtension(filePath, ".zip");
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }
        }

        // Class for encryption and decryption operations
        public class OpSec
        {
            // Encrypt a string using XOR operation with a master key
            public static string Encrypt(string input, string masterKey)
            {
                string encrypted = string.Empty;

                for (int i = 0; i < input.Length; i++)
                {
                    char encryptedChar = (char)(input[i] ^ masterKey[i % masterKey.Length]);
                    encrypted += encryptedChar;
                }

                return encrypted;
            }

            // Decrypt a string using XOR operation with a master key
            public static string Decrypt(string input, string masterKey)
            {
                string decrypted = string.Empty;

                for (int i = 0; i < input.Length; i++)
                {
                    char decryptedChar = (char)(input[i] ^ masterKey[i % masterKey.Length]);
                    decrypted += decryptedChar;
                }

                return decrypted;
            }
        }

        // Event handler for the dnD button click
        private void dnD_Click(object sender, EventArgs e)
        {
            // TODO: Implement dnD functionality
        }
        // Event handler for when the enter key is pressed in the commandBox
        private void commandBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string message = commandBox.Text;
                ProcessCommand(message);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
        private bool ProcessCommand(string command)
        {

            if (command.Substring(0, 6) == "!setip")
            {
                if(command.Length == 6)
                {
                    SystemLog("[Help] !setip - Change the default IP");
                    return false;
                }
                command = command.Substring(7);
                if (!IsValidIPv4Address(command))
                {
                    SystemLog("Invalid IP address format.");
                    return false;
                }
                HoardorServerIP = command;
                SystemLog("Server IP set to: '" + HoardorServerIP +"'");
                Invoke((MethodInvoker)(() =>
                {
                    commandBox.Text = "";
                }));
                return true;

            }

            
            commandBox.Invoke((MethodInvoker)(() =>
            {
                commandBox.Text = "";
            }));
            return false;
        }
        public bool IsValidIPv4Address(string ipAddress)
        {
            string[] octets = ipAddress.Split('.');
            if (octets.Length != 4)
            {
                return false;
            }

            foreach (string octet in octets)
            {
                if (!byte.TryParse(octet, out byte result))
                {
                    return false;
                }
            }

            return true;
        }

    }
}