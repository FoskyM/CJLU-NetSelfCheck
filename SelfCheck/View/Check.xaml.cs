using Microsoft.Win32;
using SelfCheck.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SelfCheck.View
{
	/// <summary>
	/// Check.xaml 的交互逻辑
	/// </summary>
	public partial class Check : UserControl
	{
		private bool state_checking = false;
		private bool state_checked = false;
		private Thread thread;

		private bool err_service = false;
		private bool err_safety = false;
		private bool err_cache = false;
		private bool err_status = false;
		private bool err_dns = false;
		private bool err_other = false;

		private string remark_sevice = "正常";
		private string remark_safety = "正常";
		private string remark_cache = "正常";
		private string remark_status = "正常";
        private string remark_dns = "正常";
        private string remark_other = "正常";

		SynchronizationContext _syncContext = null;

		public delegate void Entrust(string str);
		public Check()
		{
			InitializeComponent();
			_syncContext = SynchronizationContext.Current;
		}

		private void Btn_Check_Click(object sender, RoutedEventArgs e)
		{
			if (!state_checking)
			{
				Progress_Check.IsIndeterminate = true;
				Progress_Check.Value = 110;
				Btn_Check.Content = "取消扫描";
				state_checking = true;

				Loading_Check.Visibility = Visibility.Visible;
				Tips_Index.Visibility = Visibility.Collapsed;

				Entrust callback = new Entrust(Check_Thread_CallBack); //把方法赋值给委托
				thread = new Thread(Check_Thread);
				thread.IsBackground = true;
				thread.Start(callback);//将委托传递到子线程中

				/*thread = new Thread(
					new ThreadStart(() => {
						Check_Thread();
					})
				);

				thread.IsBackground = true;
				thread.Start();
				thread.Join();
*/
				/*while ( thread.ThreadState == ThreadState.Running )
				{
					Check_Result_List.Visibility = Visibility.Visible;
				}*/
				

			}
			else
			{
				Progress_Check.IsIndeterminate = false;
				Progress_Check.Value = 0;
				Btn_Check.Content = "立即扫描";
				state_checking = false;

				Tips_Index.Visibility = Visibility.Visible;
				Loading_Check.Visibility = Visibility.Collapsed;

				if (thread.ThreadState == ThreadState.Running)
					thread.Abort();
			}
		}

		private void Btn_Fix_Click(object sender, RoutedEventArgs e)
		{
			Btn_Fix.IsEnabled = false;
			Btn_Fix.Content = "修复中";
			if (err_service)
			{
				if (!Service.Start("RasMan"))
				{
					remark_sevice = "启动相关服务失败";
				} else
				{
					err_service = false;
					remark_sevice = "修复成功";
					Item_Service.Status = "ok";
				}
				Item_Service.Subtitle = remark_sevice;
			}

			if (err_safety)
			{
				Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\RasMan\\Parameters", "AllowL2TPWeakCrypto", 1, RegistryValueKind.DWord);
				Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\RasMan\\Parameters", "ProhibitIpSec", 1, RegistryValueKind.DWord);
				if (
					(int)Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\RasMan\\Parameters", "AllowL2TPWeakCrypto", 0) == 0 
					|| (int)Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\RasMan\\Parameters", "ProhibitIpSec", 0) == 0
					)
				{
					err_safety = true;
					remark_safety = "尝试修复失败";
				}
				else
				{
					err_safety = false;
					remark_safety = "修复成功";
					Item_Safety.Status = "ok";
				}
				Item_Safety.Subtitle = remark_safety;
			}

			if (err_cache)
			{
				Helper.ExecuteInCmd("ipconfig /release");
				Helper.ExecuteInCmd("ipconfig /flushdns");
				Helper.ExecuteInCmd("ipconfig /renew");

				if (!Net.HasCache())
				{
					err_cache = false;
					remark_cache = "修复成功";
					Item_Status.Status = "ok";
				} else
				{
					remark_cache = "清除缓存失败";
				}
				
				Item_Cache.Subtitle = remark_cache;
			}

			if (err_status)
			{
				remark_status = "请自行检查网线、网口是否完好";
				Item_Status.Subtitle = remark_status;
			}

            if (err_dns)
            {
				Hosts.Set("portal2.cjlu.edu.cn", "192.168.100.12");

				for (int i = 0; i < 3; i++)
				{
                    if (!Net.Ping("portal2.cjlu.edu.cn") && Net.Ping("192.168.100.12"))
                    {
                        remark_dns = "DNS错误修复失败";
                    }
                    else
                    {
                        err_dns = false;
                        remark_dns = "修复成功";
                        Item_Dns.Status = "ok";
                        break;
                    }
                }

                Item_Dns.Subtitle = remark_dns;
            }

            Btn_Back.Visibility = Visibility.Visible;
			Btn_Fix.Visibility = Visibility.Collapsed;
			Btn_Fix.Content = "马上修复";

			if (!err_service && !err_safety && !err_cache && !err_status && !err_dns && !err_other)
			{
				MessageBox.Show("修复成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			else
			{
				MessageBox.Show("部分项无法修复，可咨询相关人员", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		private void Btn_Back_Click(object sender, RoutedEventArgs e)
		{
			Tips_Index.Visibility = Visibility.Visible;
			Loading_Check.Visibility = Visibility.Collapsed;
			Check_Result_List.Visibility = Visibility.Collapsed;
			Progress_Check.IsIndeterminate = false;
			Progress_Check.Value = 0;
			Btn_Check.Visibility = Visibility.Visible;
			Btn_Fix.Visibility = Visibility.Collapsed;
			Btn_Back.Visibility = Visibility.Collapsed;
		}

		private void Check_Thread_CallBack_UI(object text)
		{
			Loading_Check.Visibility = Visibility.Collapsed;
			Check_Result_List.Visibility = Visibility.Visible;
			Progress_Check.IsIndeterminate = false;
			Progress_Check.Value = 200;
			Btn_Check.Content = "立即扫描";
			Btn_Check.Visibility = Visibility.Collapsed;

			if (!err_service && !err_safety && !err_cache && !err_status && !err_dns && !err_other)
			{
				Btn_Back.Visibility = Visibility.Visible;
			}
			else
			{
				Btn_Fix.Visibility = Visibility.Visible;
			}

			if (err_service) Item_Service.Status = "error";
			if (err_safety) Item_Safety.Status = "error";
			if (err_status) Item_Status.Status = "error";
			if (err_cache) Item_Cache.Status = "error";
            if (err_dns) Item_Dns.Status = "error";
            if (err_other) Item_Other.Status = "error";

			Item_Service.Subtitle = remark_sevice;
			Item_Safety.Subtitle = remark_safety;
			Item_Status.Subtitle = remark_status;
			Item_Cache.Subtitle = remark_cache;
            Item_Dns.Subtitle = remark_dns;
            Item_Other.Subtitle = remark_other;
		}

		private void Check_Thread_CallBack(string str)
		{
			_syncContext.Post(Check_Thread_CallBack_UI, "success");

			/*            */

			state_checking = false;
			state_checked = true;
			MessageBox.Show("扫描完毕", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void Check_Thread(object obj)
		{
			// Remote Access Connection Manager
			if (Service.Exists("RasMan"))
			{
				if (!Service.Running("RasMan"))
				{
					err_service = true;
					remark_sevice = "相关服务未运行";
				}
			}
			else
			{
				err_service = true;
				remark_sevice = "相关服务缺失";
			}

			if ((int)Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\RasMan\\Parameters", "AllowL2TPWeakCrypto", 0) == 0)
			{
				err_safety = true;
				remark_safety = "安全层错误";
			}

			if ((int)Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\RasMan\\Parameters", "ProhibitIpSec", 0) == 0)
			{
				err_safety = true;
				remark_safety = "安全层错误";
			}

			if (Net.HasCache())
			{
				err_cache = true;
				remark_cache = "存在缓存";
			}

			if (!Net.Ping("192.168.100.12") && !Net.Ping("192.168.200.1") && !Net.Ping("183.157.160.5"))
			{
				err_status = true;
				remark_status = "疑似断网";
			}

            if (!Net.Ping("portal2.cjlu.edu.cn") && Net.Ping("192.168.100.12"))
            {
                err_dns = true;
                remark_dns = "DNS错误";
            }

            /*Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\RasMan\\Parameters", "AllowL2TPWeakCrypto", 0);
			Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\RasMan\\Parameters", "ProhibitIpSec", 0);

			Net.Ping("192.168.100.12");
			Net.Ping("192.168.200.1");
			Net.Ping("183.157.160.5");*/


            Entrust callback = obj as Entrust;//强转为委托
			callback("1");
		}
	}
}
