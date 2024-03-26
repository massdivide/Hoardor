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




namespace Hoardor
{
    public partial class MainGUI : Form
    {
        public bool fileToUpload = false;
        public string fileToUploadPath = "";
        public MainGUI()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += MainGUI_DragEnter;
            this.DragDrop += MainGUI_DragDrop;
            uploadTargetFile.Enabled = false;
        }

        private void MainGUI_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        public void ToggleUploadButtonAccess(bool access)
        {
            uploadTargetFile.Enabled = access;
        }

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

        private void MainGUI_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                ToggleButton();
                ToggleUploadButtonAccess(true);
                fileToUploadPath = files[0];

                if (File.Exists(fileToUploadPath))
                {
                    string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    Directory.CreateDirectory(tempDirectory);
                    File.Copy(fileToUploadPath, Path.Combine(tempDirectory, Path.GetFileName(fileToUploadPath)));
                    fileToUploadPath = tempDirectory;
                }
                CompressFile(fileToUploadPath);

            }
        }

        private void uploadTargetFile_Click(object sender, EventArgs e)
        {
            if(fileToUpload)
            {
                // Abort the upload
                ToggleButton();
                ToggleUploadButtonAccess(false);
                // Your code here
            }
            else
            {
                // Upload the file
                ToggleButton();
                ToggleUploadButtonAccess(false);
                
                // Your code here
            }
        }

        public void UpdateFileToUploadPath(string path)
        {
            fileToUploadPath = path;
        }

        public void CompressFile(string path)
        {
            string compressedFilePath = Path.ChangeExtension(path, ".zip");
            ZipFile.CreateFromDirectory(path, compressedFilePath);
            UploadFileToServer(compressedFilePath);
        }
        public void UploadFileToServer(string filePath)
        {
            // Read the file into packets
            byte[] fileBytes = File.ReadAllBytes(filePath);
            int packetSize = 1024; // Set the packet size as desired
            int totalPackets = (int)Math.Ceiling((double)fileBytes.Length / packetSize);

            // Connect to the server
            using (TcpClient client = new TcpClient("127.0.0.1", 4242))
            {
                using (NetworkStream stream = client.GetStream())
                {
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
                    }
                }
            }
        }
                



        // ...
    }
}