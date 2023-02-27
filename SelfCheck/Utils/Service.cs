using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.ComponentModel;

namespace SelfCheck.Utils
{
    public class Service
    {

        public static bool Start(string serviceName)
        {
            bool isbn = false;

            try
            {
                if (Exists(serviceName))
                {
                    ServiceController sc = new ServiceController(serviceName);
                    if (sc.Status != ServiceControllerStatus.Running &&
                    sc.Status != ServiceControllerStatus.StartPending)
                    {
                        sc.Start();
                        for (int i = 0; i < 60; i++)
                        {
                            sc.Refresh();
                            System.Threading.Thread.Sleep(1000);
                            if (sc.Status == ServiceControllerStatus.Running)
                            {
                                isbn = true;
                                break;
                            }
                            if (i == 59)
                            {
                                isbn = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return isbn;
        }
        public static bool Exists(string serviceName)
        {
            bool isbn = false;
            //获取所有服务
            ServiceController[] services = ServiceController.GetServices();
            try
            {
                foreach (ServiceController service in services)
                {
                    if (service.ServiceName.ToUpper() == serviceName.ToUpper())
                    {
                        isbn = true;
                        break;
                    }
                }
                return isbn;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Running(string serviceName)
        {
            ServiceController sc;
            try
            {
                sc = new ServiceController(serviceName);
            }
            catch (ArgumentException)
            {
                return false;
            }

            using (sc)
            {
                ServiceControllerStatus status;
                try
                {
                    sc.Refresh();
                    status = sc.Status;
                }
                catch (Win32Exception ex)
                {
                    return false;
                }

                switch (status)
                {
                    case ServiceControllerStatus.Running:
                        return true;
                    case ServiceControllerStatus.Stopped:
                        return false;
                    case ServiceControllerStatus.Paused:
                        return false;
                    case ServiceControllerStatus.StopPending:
                        return false;
                    case ServiceControllerStatus.StartPending:
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}
