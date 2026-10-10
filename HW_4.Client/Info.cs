using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace HW_4.Client;

public class Info
{
    private readonly Random random = new Random();
    public string Ip { get; set; }
    public int Port { get; set; }
    public UInt32 Id { get; set; }
    public UInt16 PacketNumber { get; set; }
    public double Temp { get; set; }
    public int Humidity { get; set; }
    public double WaterPressure { get; set; }
    public int MainsVoltage { get; set; }

    public Info(string ip, int port)
    {
        Ip = ip;
        Port = port;
        Id = (uint)random.Next(0, 1000000);
        PacketNumber = 0;
        Temp = 21.0;
        Humidity = 50;
        WaterPressure = 3.0;
        MainsVoltage = 220;
    }

    public static async Task SendDataAsync(UdpClient client, Info info)
    {
        byte[] bytes = new byte[14];

        BitConverter.TryWriteBytes(bytes.AsSpan(0, 4), info.Id);
        BitConverter.TryWriteBytes(bytes.AsSpan(4, 2), info.PacketNumber);

        BitConverter.TryWriteBytes(bytes.AsSpan(6, 2), (short)(info.Temp * 100));
        BitConverter.TryWriteBytes(bytes.AsSpan(8, 2), (ushort)(info.Humidity * 100));

        BitConverter.TryWriteBytes(bytes.AsSpan(10, 2), (ushort)(info.WaterPressure * 1000));
        BitConverter.TryWriteBytes(bytes.AsSpan(12, 2), (ushort)(info.MainsVoltage * 10));

        await client.SendAsync(bytes, bytes.Length, info.Ip, info.Port);
    }
}
