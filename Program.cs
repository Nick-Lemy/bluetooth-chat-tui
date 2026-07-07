using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

// Both sides must use the same service GUID.
var serviceId = new Guid("00001101-0000-1000-8000-00805F9B34FB");

Console.Write("[H]ost or [J]oin? ");
var choice = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

Stream stream;

if (choice.Equals("h", StringComparison.CurrentCultureIgnoreCase))
{
    // HOST: wait for one client
    var listener = new BluetoothListener(serviceId);
    var radio = BluetoothRadio.Default;
    Console.WriteLine($"Host name: {radio.Name ?? "Unknown"}");
    Console.WriteLine($"Host address: {radio.LocalAddress}");
    
    listener.Start();
    Console.WriteLine("Waiting for someone to connect...");
    var client = listener.AcceptBluetoothClient(); // blocks until a client connects
    stream = client.GetStream();
    Console.WriteLine("Connected!");
}
else if(choice.Equals("j", StringComparison.CurrentCultureIgnoreCase))
{
    // CLIENT: find the host and connect
    var bt = new BluetoothClient();
    
    Console.WriteLine("Scanning for nearby devices...");
    var devices = bt.PairedDevices.Concat(bt.DiscoverDevices()).ToList();

    for (int i = 0; i < devices.Count; i++)
        Console.WriteLine($"[{i}] {devices.ElementAt(i).DeviceName} ({devices.ElementAt(i).DeviceAddress})");

    Console.Write("Pick the host number: ");
    var isParsed = int.TryParse(Console.ReadLine() ?? "-1", out int pick);
    if (!isParsed || pick < 0 || pick >= devices.Count)
    {
        Console.WriteLine("Invalid choice.");
        return;
    }

    var ep = new BluetoothEndPoint(devices.ElementAt(pick).DeviceAddress, BluetoothService.SerialPort);
    bt.Connect(ep);
    stream = bt.GetStream();
    Console.WriteLine("Connected!");
}
else 
{
    Console.WriteLine("Invalid choice.");
    return;
}

// Wrap the raw stream so we can send/receive lines of text.
var reader = new StreamReader(stream);
var writer = new StreamWriter(stream) { AutoFlush = true };

// Print incoming messages in the background.
_ = Task.Run(async () =>
{
    string? line;
    while ((line = await reader.ReadLineAsync()) != null)
        Console.WriteLine($"\nThem: {line}");
    Console.WriteLine("* They disconnected.");
});

// Send whatever you type.
Console.WriteLine("Type a message and press Enter:\n");
string? input;
while ((input = Console.ReadLine()) != null)
    await writer.WriteLineAsync(input);
