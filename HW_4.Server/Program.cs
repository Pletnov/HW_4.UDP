using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using HW_4.Server;

//1
/*
Console.WriteLine("Server");
var port = 5000;
var udpClient = new UdpClient(port);
Console.WriteLine($"Listening for UDP messages on port {port}...");
try
{
    using var cpuCounter = new PerformanceCounter(
    "Processor",
    "% Processor Time",
    "_Total");
    cpuCounter.NextValue();
    while (true)
    {
        var receivedResult = await udpClient.ReceiveAsync();
        var receivedMessage = Encoding.UTF8.GetString(receivedResult.Buffer);
        Console.WriteLine($"Recived message: {receivedMessage} from {receivedResult.RemoteEndPoint}");
        if (receivedMessage.Contains("cpu"))
        {
            double cpuUsage = cpuCounter.NextValue();
            Console.WriteLine($"CPU usage: {cpuUsage:F2}%");
            var responseMessageCpu = $"{cpuUsage:F2}%";
            var responseDataCpu = Encoding.UTF8.GetBytes(responseMessageCpu);
            await udpClient.SendAsync(responseDataCpu, responseDataCpu.Length, receivedResult.RemoteEndPoint);
            Console.WriteLine($"Sent CPU usage: {responseMessageCpu} " + $"to {receivedResult.RemoteEndPoint}");
        }
        else
        {
            var responseMessage = $"Received your message: {receivedMessage}";
            var responseData = Encoding.UTF8.GetBytes(responseMessage);
            await udpClient.SendAsync(responseData, responseData.Length, receivedResult.RemoteEndPoint);
            Console.WriteLine($"Sent response: {responseMessage} to {receivedResult.RemoteEndPoint}");
        }

    }
}
catch (SocketException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
finally
{
    udpClient.Close();
    Console.WriteLine("Server stopped.");
}
*/


//2

Console.WriteLine("Server");
var port = 19000;

var udpClient = new UdpClient(port);
var sensors = new ConcurrentDictionary<uint, Sensor>();

_ = Task.Run(async () =>
{
    int incorrectCount = 0;
    int lateCount = 0;
    while (true)
    {
        var receiveData = await udpClient.ReceiveAsync();
        byte[] buffer = receiveData.Buffer;
        if (receiveData.Buffer.Length != 14)
        {
            Interlocked.Increment(ref incorrectCount);
            Console.WriteLine($"Wrong packet length! Length: {receiveData.Buffer.Length} byte. All wrongs: {incorrectCount}");
            continue;
        }
        UInt32 id = BitConverter.ToUInt32(buffer, 0);
        ushort packetNumber = BitConverter.ToUInt16(buffer, 4);

        double temp = BitConverter.ToInt16(buffer, 6) / 100.0;
        int humidity = BitConverter.ToUInt16(buffer, 8) / 100;
        double waterPressure = BitConverter.ToUInt16(buffer, 10) / 1000.0;
        int mainsVoltage = BitConverter.ToUInt16(buffer, 12) / 10;

        sensors.AddOrUpdate(id,
            new Sensor
            {
                Id = id,
                LastPacketNumber = packetNumber,
                Temp = temp,
                Humidity = humidity,
                WaterPressure = waterPressure,
                MainsVoltage = mainsVoltage,
                LastSeen = DateTime.Now
            },
            (key, existing) =>
            {
                ushort diff = (ushort)(packetNumber - existing.LastPacketNumber);
                if (diff == 0 || diff >= 32768)
                {
                    Interlocked.Increment(ref lateCount);
                    Console.WriteLine(lateCount);
                    return existing;
                }
                existing.LastPacketNumber = packetNumber;
                existing.Temp = temp;
                existing.Humidity = humidity;
                existing.WaterPressure = waterPressure;
                existing.MainsVoltage = mainsVoltage;
                existing.LastSeen = DateTime.Now;
                return existing;
            }
        );
    }
});


_ = Task.Run(async () =>
{
    Console.Clear();
    while(true)
    {
        Console.SetCursorPosition(0, 0);
        var activeSensors = sensors.Values
            .Where(s => (DateTime.Now - s.LastSeen).TotalSeconds <= 5)
            .ToList();
        int connectedCount = activeSensors.Count;
        Console.WriteLine("======================= Sensors =======================");
        Console.WriteLine("ID          | Temp   | Hum  | Press   | Volt  | Ms ago ");
        Console.WriteLine(new string('-', 55));
        foreach (var sensor in activeSensors)
        {
            double msAgo = (DateTime.Now - sensor.LastSeen).TotalMilliseconds;
            Console.WriteLine($"{sensor.Id,-11} | {sensor.Temp,6:F2} | {sensor.Humidity,4} | {sensor.WaterPressure,7:F3} | {sensor.MainsVoltage,5} | {msAgo,5:F0}ms");
        }
        for (int i = 0; i < (20 - activeSensors.Count); i++)
        {
            Console.WriteLine(new string(' ', 55));
        }
        Console.WriteLine(new string('=', 55));
        Console.WriteLine($"Active sensors: {activeSensors.Count}");
        await Task.Delay(250);
    }
});
while(true)
{
    await Task.Delay(1000);
}