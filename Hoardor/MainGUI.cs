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
        private bool fileToUpload = false;
        private string fileToUploadPath = "";
        private bool isUploading = false;

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

        }

        public void UpdateFileToUploadPath(string path)
        {
            fileToUploadPath = path;
        }

        public void CompressFile(string path)
        {
            string compressedFilePath = Path.ChangeExtension(path, ".zip");

            // Use ThreadPool to run the compression and upload tasks
            ThreadPool.SetMaxThreads(48, 48); // Set the maximum number of threads in the ThreadPool
            ThreadPool.QueueUserWorkItem(state =>
            {
                ZipFile.CreateFromDirectory(path, compressedFilePath);
                UploadFileToServer(compressedFilePath);
            });
        }

        public void UploadFileToServer(string filePath)
        {
            if (isUploading)
            {
                return; // Already uploading, ignore the request
            }

            isUploading = true;

            // Read the file into packets
            byte[] fileBytes = File.ReadAllBytes(filePath);
            int packetSize = 8192; // Set the packet size as desired
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

                        // Update progress bar
                        int progressPercentage = (int)(((double)packetIndex + 1) / totalPackets * 100);
                        progressUpload.Invoke((MethodInvoker)(() => progressUpload.Value = progressPercentage));
                    }
                }
            }

            isUploading = false;
        }

        // ...
    }
}