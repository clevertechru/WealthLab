using System;
using System.Collections.Generic;
using PcapDotNet.Core;
using PcapDotNet.Packets;

namespace OpeningAnAdapterAndCapturingThePackets
{
    class Program
    {
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
            String rayBrand = "EXFD2 ";
            String rayPeriod = "..";
            String rayPack = packet.ToString();
            int rayBegin = rayPack.IndexOf(rayBrand);
            if (rayBegin > 0) //while
            {
                int rayEnd = rayPack.IndexOf(rayPeriod,rayBegin);
                String mainPack = rayPack.TrimStart().Substring(rayBegin + rayBrand.Length, rayEnd);
                int mainIndex = mainPack.IndexOf('.');
                Console.WriteLine("Time:" + mainPack.TrimStart().Substring(0,mainIndex));
                mainIndex = mainPack.IndexOf(' ', mainIndex) + 1;
                int dot2 = mainPack.IndexOf('.', mainIndex) + 3;
                Console.WriteLine("Strike Price: ", mainPack.TrimStart().Substring(mainIndex, dot2));
                mainIndex = mainPack.IndexOf(' ', dot2) + 1;
                dot2 = mainPack.IndexOf('.', mainIndex) + 3;
                String temp = mainPack.TrimStart().Substring(mainIndex, dot2);
                mainIndex = mainPack.IndexOf('.', dot2) + 1;
                dot2 = mainPack.IndexOf('%', mainIndex) + 1;
                String now = mainPack.TrimStart().Substring(mainIndex, dot2);
                if(now.StartsWith("-"))
                {
                    temp = temp.Insert(0, "-");
                }
                Console.WriteLine("Differential/Basis: ", temp);
                Console.WriteLine("Range: ", now);
                mainIndex = mainPack.IndexOf(' ', dot2) + 1;
                dot2 = mainPack.IndexOf('.', mainIndex) + 3;
                Console.WriteLine("At the open? ", mainPack.TrimStart().Substring(mainIndex, dot2));
                mainIndex = mainPack.IndexOf('.', dot2) + 2;
                dot2 = mainPack.IndexOf('.', mainIndex) + 3;
                Console.WriteLine("At the close? ", mainPack.TrimStart().Substring(mainIndex, dot2));
                mainIndex = mainPack.IndexOf('.', dot2) + 1;
                dot2 = mainPack.IndexOf('.', mainIndex);
                Console.WriteLine("Volume: ", mainPack.TrimStart().Substring(mainIndex, dot2));
                mainIndex = mainPack.IndexOf(' ', dot2 + 3 ) + 1;
                dot2 = mainPack.IndexOf('.', mainIndex) + 3;
                Console.WriteLine("Opening Price: ", mainPack.TrimStart().Substring(mainIndex, dot2));
                mainIndex = mainPack.IndexOf(' ', dot2) + 1;
                dot2 = mainPack.IndexOf('.', mainIndex) + 3;
                Console.WriteLine("Highest Price? ", mainPack.TrimStart().Substring(mainIndex, dot2));
                mainIndex = mainPack.IndexOf(' ', dot2) + 1;
                dot2 = mainPack.IndexOf('.', mainIndex) + 3;
                Console.WriteLine("Lowest Price? ", mainPack.TrimStart().Substring(mainIndex, dot2));

                mainIndex = mainPack.IndexOf('.', dot2) + 1;
                mainIndex = mainPack.IndexOf('.', mainIndex) + 1;
                mainIndex = mainPack.IndexOf('.', mainIndex) + 1;
                mainIndex = mainPack.IndexOf('.', mainIndex) + 1;
                mainIndex = mainPack.IndexOf('.', mainIndex) + 1;
                
                mainIndex = mainPack.IndexOf('.', mainIndex) + 1;
                dot2 = mainPack.IndexOf('.', mainIndex) + 3;
                Console.WriteLine("Theory Price? ", mainPack.TrimStart().Substring(mainIndex, dot2));

                mainIndex = mainPack.IndexOf('%', dot2) + 1;
                mainIndex = mainPack.IndexOf('.', mainIndex) + 1;
                mainIndex = mainPack.IndexOf('.', mainIndex) + 1;
                
                mainIndex = mainPack.IndexOf('.', mainIndex) + 1;
                dot2 = mainPack.IndexOf('.', mainIndex) + 3;
                Console.WriteLine("Real Basis? ", mainPack.TrimStart().Substring(mainIndex, dot2));

                mainIndex = mainPack.IndexOf('.', mainIndex) + 1;
                dot2 = mainPack.IndexOf('.', mainIndex) + 3;
                Console.WriteLine("Theory Basis? ", mainPack.TrimStart().Substring(mainIndex, dot2));
                
                rayBegin = rayPack.IndexOf(rayBrand, rayEnd + rayPeriod.Length);
            }
        }
    }
}
