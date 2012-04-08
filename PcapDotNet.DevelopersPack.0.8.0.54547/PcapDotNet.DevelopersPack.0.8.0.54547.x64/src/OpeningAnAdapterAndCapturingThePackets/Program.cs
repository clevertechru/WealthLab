using System;
using System.Collections.Generic;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using System.Text;
namespace OpeningAnAdapterAndCapturingThePackets
{
    class Program
    {
        static AsynchronousSocketListener rayLister;

        static void Main(string[] args)
        {
            // Send anonymous statistics about the usage of Pcap.Net
            PcapDotNet.Analysis.PcapDotNetAnalysis.OptIn = true;

            // Retrieve the device list from the local machine
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

            if (allDevices.Count == 0)
            {
                Console.WriteLine("No interfaces found! Make sure WinPcap is installed.");
                return;
            }

            // Print the list
            for (int i = 0; i != allDevices.Count; ++i)
            {
                LivePacketDevice device = allDevices[i];
                Console.Write((i + 1) + ". " + device.Name);
                if (device.Description != null)
                    Console.WriteLine(" (" + device.Description + ")");
                else
                    Console.WriteLine(" (No description available)");
            }

            int deviceIndex = 0;
            do
            {
                Console.WriteLine("Enter the interface number (1-" + allDevices.Count + "):");
                string deviceIndexString = Console.ReadLine();
                if (!int.TryParse(deviceIndexString, out deviceIndex) ||
                    deviceIndex < 1 || deviceIndex > allDevices.Count)
                {
                    deviceIndex = 0;
                }
            } while (deviceIndex == 0);

            // Take the selected adapter
            PacketDevice selectedDevice = allDevices[deviceIndex - 1];
            rayLister = new AsynchronousSocketListener();
            // Open the device
            using (PacketCommunicator communicator = 
                selectedDevice.Open(443,                                  // portion of the packet to capture
                                                                            // 65536 guarantees that the whole packet will be captured on all the link layers
                                    PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                    1000))                                  // read timeout
            {
                Console.WriteLine("Listening on " + selectedDevice.Description + "...");

                // start the capture
                communicator.ReceivePackets(0, PacketHandler);
            }
        }

        // Callback function invoked by Pcap.Net for every incoming packet
        private static void PacketHandler(Packet packet)
        {
            //Console.WriteLine(packet.Timestamp.ToString("yyyy-MM-dd hh:mm:ss.fff") + " length:" + packet.Length);
            String rayBrand = "EXFD2\t";
            Encoding enc8 = Encoding.ASCII;
            String rayPack = enc8.GetString(packet.Buffer); 
            int rayBegin = rayPack.IndexOf(rayBrand);
            while (rayBegin > 0) //while
            {

                int leftIndex = rayBegin + rayBrand.Length;
                int rightIndex = rayPack.IndexOf('\t', leftIndex);
                Console.WriteLine(" Time:" + rayPack.Substring(leftIndex, rightIndex - leftIndex));
                
                leftIndex = rayPack.IndexOf(' ', rightIndex + 1) + 1;
                rightIndex = rayPack.IndexOf('\t', leftIndex);
                Console.WriteLine(" Strike Price:" + rayPack.Substring(leftIndex, rightIndex - leftIndex));
                
                leftIndex = rayPack.IndexOf("  ", rightIndex + 1) + 2;
                rightIndex = rayPack.IndexOf('\t', leftIndex) ;
                String temp = rayPack.Substring(leftIndex, rightIndex - leftIndex);

                leftIndex = rightIndex + 1;
                rightIndex = rayPack.IndexOf('\t', leftIndex) ;
                String now = rayPack.Substring(leftIndex, rightIndex - leftIndex);
                
                if(now.StartsWith("-"))
                {
                    temp = temp.Insert(0, "-");
                }
                Console.WriteLine(" Differential:" + temp);
                Console.WriteLine(" Range:" + now);

                leftIndex = rayPack.IndexOf(' ', rightIndex + 1) + 1;
                rightIndex = rayPack.IndexOf('\t', leftIndex);
                Console.WriteLine(" Bid:" + rayPack.Substring(leftIndex, rightIndex - leftIndex));

                leftIndex = rayPack.IndexOf(' ', rightIndex + 1) + 1;
                rightIndex = rayPack.IndexOf('\t', leftIndex) ;
                Console.WriteLine(" Ask:" + rayPack.Substring(leftIndex, rightIndex - leftIndex));

                leftIndex = rightIndex + 1;
                rightIndex = rayPack.IndexOf('\t', leftIndex);
                Console.WriteLine(" Volume:" + rayPack.Substring(leftIndex, rightIndex - leftIndex));

                leftIndex = rayPack.IndexOf(' ', rightIndex + 1 ) + 1;
                rightIndex = rayPack.IndexOf('\t', leftIndex) ;
                Console.WriteLine(" Opening Price:" + rayPack.Substring(leftIndex, rightIndex - leftIndex));
                
                leftIndex = rayPack.IndexOf(' ', rightIndex + 1) + 1;
                rightIndex = rayPack.IndexOf('\t', leftIndex);
                Console.WriteLine(" Ceiling Price:" + rayPack.Substring(leftIndex, rightIndex - leftIndex));

                leftIndex = rayPack.IndexOf(' ', rightIndex + 1) + 1;
                rightIndex = rayPack.IndexOf('\t', leftIndex);
                Console.WriteLine(" Floor Price:" + rayPack.Substring(leftIndex, rightIndex - leftIndex));
                /*
                leftIndex = rayPack.IndexOf('\t', rightIndex + 1) + 1;
                leftIndex = rayPack.IndexOf('\t', leftIndex) + 1;
                leftIndex = rayPack.IndexOf('\t', leftIndex) + 1;
                leftIndex = rayPack.IndexOf('\t', leftIndex) + 1;
                leftIndex = rayPack.IndexOf('\t', leftIndex) + 1;
                
                rightIndex = rayPack.IndexOf('\t', leftIndex) ;
                Console.WriteLine("Theory Price/Future Price? " + rayPack.Substring(leftIndex, rightIndex - leftIndex));

                leftIndex = rayPack.IndexOf('%', rightIndex + 1) + 2;
                
                leftIndex = rayPack.IndexOf('\t', leftIndex) + 1;
                rightIndex = rayPack.IndexOf('\t', leftIndex) ;
                Console.WriteLine("Real Basis? " + rayPack.Substring(leftIndex, rightIndex - leftIndex));

                leftIndex = rightIndex + 1;
                rightIndex = rayPack.IndexOf('\t', leftIndex) ;
                Console.WriteLine("Theory Basis? " + rayPack.Substring(leftIndex, rightIndex - leftIndex));
                */
                rayBegin = rayPack.IndexOf(rayBrand, rightIndex + 1);

            }
        }
    }
}
