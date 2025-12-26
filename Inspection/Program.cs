using System;
using SteamKit2.CDN;

namespace Inspection
{
    class Program
    {
        static void Main(string[] args)
        {
            var type = typeof(Server);
            var prop = type.GetProperty("Protocol");
            if (prop != null)
            {
                Console.WriteLine($"Protocol Type: {prop.PropertyType.FullName}");
                if (prop.PropertyType.IsEnum)
                {
                    foreach (var name in Enum.GetNames(prop.PropertyType))
                    {
                        Console.WriteLine($"Enum Value: {name}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Protocol property not found.");
            }
        }
    }
}
