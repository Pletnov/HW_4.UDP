using System;
using System.Collections.Generic;
using System.Text;

namespace HW_4.Server;

public class Sensor
{
    public UInt32 Id { get; set; }
    public ushort LastPacketNumber { get; set; }
    public double Temp { get; set; }
    public int Humidity { get; set; }
    public double WaterPressure { get; set; }
    public int MainsVoltage { get; set; }
    public DateTime LastSeen { get; set; }
}
