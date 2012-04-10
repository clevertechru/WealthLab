﻿using System;
using System.Collections.Generic;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using System.Text;
namespace OpeningAnAdapterAndCapturingThePackets
{
    class Program
    {
        static SynchronousSocketListener rayclient;
        static StringBuilder rayBuilder;

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
            
            rayclient = new SynchronousSocketListener(11000);
            
            // Send test data to the remote device.
            //rayclient.raysend("This is a test<EOF>");
            
            rayBuilder = new StringBuilder();
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
            String rayBrand = "TXFD2\t";
            Encoding enc8 = Encoding.ASCII;
            String rayPack = enc8.GetString(packet.Buffer);
            int rayBegin = rayPack.IndexOf(rayBrand);
            while (rayBegin > 0) //while
            {

                int leftIndex = rayBegin + rayBrand.Length;
                int rightIndex = rayPack.IndexOf('\t', leftIndex);
                rayBuilder.Append("Hour:").Append(rayPack.Substring(leftIndex, 2));
                rayBuilder.Append("Minute:").Append(rayPack.Substring(leftIndex+2, 2));
                rayBuilder.Append("Second:").Append(rayPack.Substring(leftIndex+4, 2));
                
                leftIndex = rightIndex + 2;
                rightIndex = rayPack.IndexOf('\t', leftIndex);
                rayBuilder.Append("~Price:").Append(rayPack.Substring(leftIndex, rightIndex - leftIndex).TrimStart());
                
                leftIndex = rightIndex + 2;
                rightIndex = rayPack.IndexOf('\t', leftIndex) ;
                //String temp = rayPack.Substring(leftIndex, rightIndex - leftIndex);

                leftIndex = rightIndex + 1;
                rightIndex = rayPack.IndexOf('\t', leftIndex) ;
                /*
                String now = rayPack.Substring(leftIndex, rightIndex - leftIndex);
                if(now.StartsWith("-"))
                {
                    temp = temp.Insert(0, "-");
                }
                rayBuilder.Append("!Differential:").Append(temp);
                rayBuilder.Append("@Range:").Append(now);
                */
                leftIndex = rightIndex + 2;
                rightIndex = rayPack.IndexOf('\t', leftIndex);
                rayBuilder.Append("#Bid:").Append(rayPack.Substring(leftIndex, rightIndex - leftIndex).TrimStart());

                leftIndex = rightIndex + 2;
                rightIndex = rayPack.IndexOf('\t', leftIndex) ;
                rayBuilder.Append("$Ask:").Append(rayPack.Substring(leftIndex, rightIndex - leftIndex).TrimStart());

                leftIndex = rightIndex + 1;
                rightIndex = rayPack.IndexOf('\t', leftIndex);
                rayBuilder.Append("%Volume:").Append(rayPack.Substring(leftIndex, rightIndex - leftIndex).TrimStart());

                leftIndex = rightIndex + 1;
                rightIndex = rayPack.IndexOf('\t', leftIndex);

                leftIndex = rightIndex + 2;
                rightIndex = rayPack.IndexOf('\t', leftIndex) ;
                rayBuilder.Append("^Opening:").Append(rayPack.Substring(leftIndex, rightIndex - leftIndex).TrimStart());

                leftIndex = rightIndex + 2;
                rightIndex = rayPack.IndexOf('\t', leftIndex);
                rayBuilder.Append("&Ceiling:").Append(rayPack.Substring(leftIndex, rightIndex - leftIndex).TrimStart());

                leftIndex = rightIndex + 2;
                rightIndex = rayPack.IndexOf('\t', leftIndex);
                rayBuilder.Append("*Floor:").Append(rayPack.Substring(leftIndex, rightIndex - leftIndex).TrimStart());

                rayBuilder.Append("(");
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
                
                // Send test data to the remote device.
                rayclient.raysend(rayBuilder.ToString());
                
                rayBegin = rayPack.IndexOf(rayBrand, rightIndex + 1);

            }

        }
    }
}
