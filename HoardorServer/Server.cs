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
        private const int PacketSize = 256000;
        private const int Port = 4242;
        private static string HoardorMasterKey = "b65af5a104f97cfd07c1969aaaaeee8d"; // Change this to your own key Do it for client and server.
        private static string HoardorSecretKey = "d8d50b33"; //This is the secret key for the client
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
                // Read the key length from the client
                byte[] keyLengthBytes = new byte[sizeof(int)];
                stream.Read(keyLengthBytes, 0, keyLengthBytes.Length);
                int keyLength = BitConverter.ToInt32(keyLengthBytes, 0);

                // Read the key from the client
                byte[] keyBytes = new byte[keyLength];
                stream.Read(keyBytes, 0, keyBytes.Length);
                string key = Encoding.UTF8.GetString(keyBytes);
                Console.WriteLine("Encrypted Security key: " + key);
                string dekey = OpSec.Decrypt(key, HoardorMasterKey);
                Console.WriteLine("Decrypted Security key: " + dekey);
               
                /* DO NOT DELETE THIS CODE
                // Send the key to the server
                byte[] serverKeyBytes = Encoding.UTF8.GetBytes(key);
                byte[] serverKeyLengthBytes = BitConverter.GetBytes(serverKeyBytes.Length);
                stream.Write(serverKeyLengthBytes, 0, serverKeyLengthBytes.Length);
                stream.Write(serverKeyBytes, 0, serverKeyBytes.Length);
                */

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
                if (dekey != HoardorSecretKey)
                {
                    Console.WriteLine("Invalid security key. Closing connection.");
                    client.Close();
                    return;
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
    public class Cipher
    {
        private const int Shift = 3;

        public static string Encrypt(string input)
        {
            StringBuilder encrypted = new StringBuilder();

            foreach (char c in input)
            {
                if (char.IsLetter(c))
                {
                    char encryptedChar = (char)(c + Shift);
                    if (!char.IsLetterOrDigit(encryptedChar))
                    {
                        encryptedChar = (char)(encryptedChar - 26);
                    }
                    encrypted.Append(encryptedChar);
                }
                else
                {
                    encrypted.Append(c);
                }
            }

            return encrypted.ToString();
        }

        public static string Decrypt(string input)
        {
            StringBuilder decrypted = new StringBuilder();

            foreach (char c in input)
            {
                if (char.IsLetter(c))
                {
                    char decryptedChar = (char)(c - Shift);
                    if (!char.IsLetterOrDigit(decryptedChar))
                    {
                        decryptedChar = (char)(decryptedChar + 26);
                    }
                    decrypted.Append(decryptedChar);
                }
                else
                {
                    decrypted.Append(c);
                }
            }

            return decrypted.ToString();
        }
    }
    public class OpSec
    {
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

}
