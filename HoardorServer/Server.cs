using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Hoardor
{
    public class Server
    {
        private const int PacketSize = 8192;
        private const int Port = 4242;

        public static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Console.WriteLine("Server started. Listening for connections...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected.");

                ThreadPool.QueueUserWorkItem(HandleClient, client);
            }
        }

        private static void HandleClient(object state)
        {
            TcpClient client = (TcpClient)state;

            using (NetworkStream stream = client.GetStream())
            {
                // Read the file name length from the client
                byte[] fileNameLengthBytes = new byte[sizeof(int)];
                stream.Read(fileNameLengthBytes, 0, fileNameLengthBytes.Length);
                int fileNameLength = BitConverter.ToInt32(fileNameLengthBytes, 0);

                // Read the file name from the client
                byte[] fileNameBytes = new byte[fileNameLength];
                stream.Read(fileNameBytes, 0, fileNameBytes.Length);
                string fileName = Encoding.UTF8.GetString(fileNameBytes);
                string uniqueFileName = GetUniqueFileName(fileName);

                if (fileName != uniqueFileName)
                {
                    Console.WriteLine($"File name already exists. Renaming to {uniqueFileName}");
                    fileName = uniqueFileName;
                }
                Console.WriteLine($"File name: {fileName}");

                // Receive the total number of packets from the client
                byte[] totalPacketsBytes = new byte[sizeof(int)];
                stream.Read(totalPacketsBytes, 0, totalPacketsBytes.Length);
                int totalPackets = BitConverter.ToInt32(totalPacketsBytes, 0);
                Console.WriteLine($"Total packets: {totalPackets}");

                // Receive the file packets from the client
                byte[] fileData = new byte[totalPackets * PacketSize];
                int bytesRead = 0;
                for (int packetIndex = 0; packetIndex < totalPackets; packetIndex++)
                {
                    byte[] packet = new byte[PacketSize];
                    bytesRead = stream.Read(packet, 0, packet.Length);

                    // Write the packet to the file data array
                    Buffer.BlockCopy(packet, 0, fileData, packetIndex * PacketSize, bytesRead);

                    Console.WriteLine($"Received packet {packetIndex + 1}/{totalPackets}");
                }

                Console.WriteLine("File received successfully.");

                // Save the file with the original file name and .zip extension
                string serverDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string hoardingsDirectory = Path.Combine(serverDirectory, "Hoardings");
                Directory.CreateDirectory(hoardingsDirectory);
                string zipFilePath = Path.Combine(hoardingsDirectory, $"{fileName}");

                // Write the file data to the zip file
                File.WriteAllBytes(zipFilePath, fileData);

                Console.WriteLine($"File saved as {zipFilePath}");

                // Delete the packet files
                for (int packetIndex = 0; packetIndex < totalPackets; packetIndex++)
                {
                    string packetFilePath = Path.Combine(Path.GetTempPath(), $"packet_{packetIndex}.bin");
                    File.Delete(packetFilePath);
                }

                Console.WriteLine("Packet files deleted.");
            }

            client.Close();
        }

        private static string GetUniqueFileName(string fileName)
        {
            string uniqueFileName = fileName;
            int counter = 1;

            string serverDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string hoardingsDirectory = Path.Combine(serverDirectory, "Hoardings");

            while (File.Exists(Path.Combine(hoardingsDirectory, uniqueFileName)))
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                string fileExtension = Path.GetExtension(fileName);
                uniqueFileName = $"{fileNameWithoutExtension}_{counter}{fileExtension}";
                counter++;
            }

            return uniqueFileName;
        }
    }
}
