using System;
using System.Linq;

namespace SoundIOSharp.Example
{
    class MainClass
    {
        public static int Main(string[] args)
        {
            bool watch = false;
            string backend = null;
            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "--watch":
                        watch = true;
                        continue;
                    default:
                        if (arg.StartsWith("--backend:"))
                        {
                            backend = arg.Substring(arg.IndexOf(':') + 1);
                            continue;
                        }
                        break;
                }
                ShowUsageToExit();
                return 1;
            }

            using (var api = new SoundIO())
            {
                SoundIOBackend be = SoundIOBackend.None;
                if (Enum.TryParse(backend, out be))
                {
                    ShowUsageToExit();
                    return 1;
                }
                if (be == SoundIOBackend.None)
                    api.Connect();
                else
                    api.ConnectBackend(be);

                api.FlushEvents();
                if (watch)
                {
                    api.OnDevicesChange = () => OnDeviceChange(api);
                    Console.WriteLine("Type [ENTER] to exit.");
                    Console.ReadLine();
                }
                else
                    DoListDevices(api);
            }


            return 0;
        }

        static void DoListDevices(SoundIO api)
        {
            Console.WriteLine("Inputs");
            for (int i = 0; i < api.InputDeviceCount; i++)
                PrintDevice(api.GetInputDevice(i));
            Console.WriteLine("Outputs");
            for (int i = 0; i < api.OutputDeviceCount; i++)
                PrintDevice(api.GetOutputDevice(i));
        }

        static void PrintChannelLayout(SoundIOChannelLayout layout)
        {
            if (layout.Name == null)
                return;
            if (layout.Name.Length == 0)
                Console.WriteLine("(empty)");
            for (int i = 0; i < layout.ChannelCount; i++)
            {
                Console.Write($"{layout.Channels.ElementAt(i)} ");
            }
            Console.WriteLine();

        }
        static void PrintDevice(SoundIODevice dev)
        {
            Console.WriteLine();
            Console.WriteLine("--------------------");
            Console.Write($"{dev.Name}");
            if (dev.IsRaw)
                Console.Write(" (raw)");
            Console.WriteLine();
            Console.WriteLine("--------------------");

            Console.WriteLine($"{dev.CurrentLayout.ChannelCount} channels (current)");
            Console.Write("Channel layouts: ");
            for (int i = 0; i < dev.LayoutCount; i++)
            {
                PrintChannelLayout(dev.Layouts.ElementAt(i));
            }
            Console.WriteLine();

            Console.WriteLine("Sample Rates:");

            for (int i = 0; i < dev.SampleRateCount; i++)
            {
                var range = dev.SampleRates.ElementAt(i);
                Console.WriteLine($"{range.Min} - {range.Max}");
            }
            Console.WriteLine();

            Console.Write("Formats: ");

            for (int i = 0; i < dev.FormatCount; i++)
            {
                Console.Write($"{dev.Formats.ElementAt(i)} ");
            }
            Console.WriteLine();
            Console.WriteLine($"Current: {dev.CurrentFormat}");

            Console.WriteLine();
            Console.WriteLine($"Latency: {dev.SoftwareLatencyCurrent} ({dev.SoftwareLatencyMin} - {dev.SoftwareLatencyMax})");

        }

        static void OnDeviceChange(SoundIO api)
        {
            DoListDevices(api);
        }

        static void ShowUsageToExit()
        {
            Console.Error.WriteLine(@"Arguments:
--watch		watch devices.
--backend:xxx	specify backend to use.

libsoundio version: {0}

available backends: {1}
",
                                     SoundIO.VersionString,
                                     string.Join(", ", Enum.GetValues(typeof(SoundIOBackend))
                                                  .Cast<SoundIOBackend>()
                                                  .Where(b => SoundIO.HaveBackend(b))));
        }
    }
}
