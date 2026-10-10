using System.Net.Sockets;
using System.Diagnostics;
using System.Text;
using HW_4.Client;

//1
/*
Console.WriteLine("Client");
var clientIp = "192.168.0.200";
var clientPort = 5000;

CancellationTokenSource cancellationToken = new CancellationTokenSource();
var token = cancellationToken.Token;

var udpClient = new UdpClient();
Console.WriteLine("To close programm enter escape...");
Console.WriteLine();

_ = Task.Run(async () =>
{
    while (!token.IsCancellationRequested)
    {
        var messageCpu = "cpu";
        var dataCpu = Encoding.UTF8.GetBytes(messageCpu);
        await udpClient.SendAsync(dataCpu, dataCpu.Length, clientIp, clientPort);

        Console.WriteLine($"Sent message: {messageCpu} to {clientIp}:{clientPort}");

        var clock = new Stopwatch();
        clock.Start();
        var receivedResult = await udpClient.ReceiveAsync();
        clock.Stop();
        var receivedMessage = Encoding.UTF8.GetString(receivedResult.Buffer);
        Console.WriteLine($"Received response: {receivedMessage} from {receivedResult.RemoteEndPoint}");
        Console.WriteLine($"Time: {clock.ElapsedMilliseconds}");
        Console.WriteLine();
        await Task.Delay(2000);
    }
});
_ = Task.Run(async () =>
{
    while (!token.IsCancellationRequested)
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Escape)
            {
                Console.WriteLine("Stopping client");
                cancellationToken.Cancel();
                break;
            }
        }
        await Task.Delay(100, cancellationToken.Token);
    }
});
try
{
    while (!token.IsCancellationRequested)
    {
        await Task.Delay(200, token);
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
    Console.WriteLine("Client stopped.");
}*/


//2

Console.WriteLine("Client");

var clientIp = "192.168.0.200";
var clientPort = 19000;

Random random = new Random();
var info = new Info(clientIp, clientPort);

Console.WriteLine($"Your id: {info.Id}");
Console.WriteLine($"Packet number: {info.PacketNumber}");

var udpClient = new UdpClient();

_ = Task.Run(async () => {
    while (true)
    {
        double changeTemp = random.NextDouble() * 0.2 - 0.1;
        double changePressure = random.NextDouble() * 0.2 - 0.1;
        int changeHumidity = random.Next(-1, 2);
        int changeVoltage = random.Next(-1, 2);

        info.Temp += changeTemp;
        info.Humidity += changeHumidity;
        info.WaterPressure += changePressure;
        info.MainsVoltage += changeVoltage;

        info.Temp = Math.Clamp(info.Temp, 18, 26);
        info.Humidity = Math.Clamp(info.Humidity, 30, 70);
        info.WaterPressure = Math.Clamp(info.WaterPressure, 2.0, 4.5);
        info.MainsVoltage = Math.Clamp(info.MainsVoltage, 207, 253);

        info.Temp = Math.Round(info.Temp, 2);
        info.WaterPressure = Math.Round(info.WaterPressure, 3);

        Console.WriteLine($"Temp: {info.Temp:F2}, Hunidity: {info.Humidity}, Water pressure: {info.WaterPressure:F2}, Mains Voltage: {info.MainsVoltage}");

        await Info.SendDataAsync(udpClient, info);
        info.PacketNumber++;

        await Task.Delay(100);
    }
});
try
{
    while (true)
    {
        await Task.Delay(1000);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
finally
{
    udpClient.Close();
    Console.WriteLine("Client stopped.");
}