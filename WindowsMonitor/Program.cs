using Newtonsoft.Json;

namespace WindowsMonitor
{ 
    internal static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");

            var infos = new Infos();
            var results = new Dictionary<string, List<dynamic>>();
            var timeInMinutes = 60;
            var count = -1;

            while (timeInMinutes > 0)
            {
                var metrics = new
                {
                    Percentage = infos.GetCpuUsage(),
                    Clock = infos.GetCpuClock(),
                    Memory = infos.GetMemory(),
                    Battery = infos.GetBattery()
                };

                var key = DateTime.Now.ToString("HH:mm");

                results.TryGetValue(key, out var resultsBySeconds);

                if (resultsBySeconds == null)
                {
                    resultsBySeconds = new List<dynamic>();
                }

                resultsBySeconds.Add(metrics);
                results[key] = resultsBySeconds;

                count++;

                if (count == 0 || count > 47)
                {
                    timeInMinutes--;
                    count = 0;

                    Console.Write("\rRemaining: {0} minutes", timeInMinutes);
                }

                //Console.WriteLine(JsonConvert.SerializeObject(metrics));
            }

            Console.WriteLine();
            Console.WriteLine("End");
            Console.WriteLine();

            Summary(results);
        }
       
        private static void Summary(Dictionary<string, List<dynamic>> results) 
        {
            var summary = new List<object>();

            foreach (var result in results)
            {
                var percentagem = 0d;
                var clock = 0d;
                var memory = 0d;
                var battery = 0d;

                foreach (var item in result.Value)
                {
                    percentagem += item.Percentage;
                    clock += item.Clock;
                    memory += item.Memory;
                    battery += item.Battery.EstimatedChargeRemaining;
                }

                percentagem /= result.Value.Count;
                clock /= result.Value.Count;
                memory /= result.Value.Count;
                battery /= result.Value.Count;

                summary.Add(new
                {
                    Time = result.Key,
                    Percentage = Math.Round(percentagem, 2),
                    Clock = Math.Round(clock, 2),
                    Memory = Math.Round(memory, 2),
                    Battery = Math.Round(battery, 2)
                });
            }

            Save(summary);
        }

        private static void Save(object values)
        {
            var summaryJson = JsonConvert.SerializeObject(values);
            var fileName = DateTime.Now.ToString("dd-MM-yy-HH-mm").Trim() + "-log.txt";

            File.WriteAllText(fileName, summaryJson);
        }
    }
}