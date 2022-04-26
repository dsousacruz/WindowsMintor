using Newtonsoft.Json;
using System.Diagnostics;
using System.Management;

namespace WindowsMonitor
{
    public class Infos
    {
        private PerformanceCounter CpuUsage { get; set; }

        private PerformanceCounter CpuClock { get; set; }

        public Infos()
        {
            CpuUsage = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            CpuClock = new PerformanceCounter("Processor Information", "% Processor Performance", "_Total");
        }

        public double GetCpuUsage()
        {
            return Math.Round(CpuUsage.NextValue(), 2);
        }

        public double GetCpuClock()
        {
            double cpuClockValue = CpuClock.NextValue();
            double turboSpeed = 0;

            foreach (ManagementObject obj in new ManagementObjectSearcher("SELECT *, Name FROM Win32_Processor").Get())
            {
                double maxSpeed = Convert.ToDouble(obj["MaxClockSpeed"]) / 1000;
                turboSpeed = maxSpeed * cpuClockValue / 100;
            }

            return Math.Round(turboSpeed * 10, 2);

        }

        public double GetMemory()
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

            var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new
            {
                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
            }).FirstOrDefault();

            if (memoryValues != null)
            {
                return Math.Round(((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100, 2);
            }

            return 0;
        }

        public object GetBattery()
        {
            var batteryStatus = "";
            var designVoltage = "";
            double estimatedChargeRemaining = 0;

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * FROM Win32_Battery");

            foreach (ManagementObject obj in searcher.Get())
            {
                foreach (PropertyData property in obj.Properties)
                {
                    //Console.WriteLine($"Name: {property.Name} | Value: {property.Value}");

                    switch (property.Name)
                    {

                        case "BatteryStatus":
                            batteryStatus = GetBaterryStatus(property.Value);
                            break;

                        case "DesignVoltage":
                            designVoltage = property.Value.ToString();
                            break;

                        case "EstimatedChargeRemaining":
                            estimatedChargeRemaining = Convert.ToDouble(property.Value);
                            break;

                        default:
                            break;
                    }
                }
            }

            return new
            {
                BatteryStatus = batteryStatus,
                DesignVoltage = designVoltage,
                EstimatedChargeRemaining = estimatedChargeRemaining
            };
        }

        private string GetBaterryStatus(object value)
        {
            switch (Convert.ToInt16(value))
            {
                case 1:
                    return "Discharging";

                case 2:
                    return "AC";

                default:
                    return "";
            }
        }
    }
}