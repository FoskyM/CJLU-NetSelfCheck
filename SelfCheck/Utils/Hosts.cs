using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SelfCheck.Utils
{
    public class Hosts
    {
        public static void Set(string domain, string ip)
        {
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\drivers\\etc\\hosts";
            string[] hosts = File.ReadAllLines(path);
            List<string> list = hosts.ToList();
            string temp = hosts.ToList().FirstOrDefault(x => x.Contains(domain));
            if (string.IsNullOrEmpty(temp))
            {
                list.Add($"{ip} {domain}");
            }
            File.WriteAllLines(path, list.ToArray());
        }
        public static void Remove(string hosts_str)
        {
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\drivers\\etc\\hosts";
            string[] hosts = File.ReadAllLines(path);
            List<string> list = hosts.ToList();
            list.RemoveAll(x => x.Contains(hosts_str));
            File.WriteAllLines(path, list.ToArray());
        }
    }
}
