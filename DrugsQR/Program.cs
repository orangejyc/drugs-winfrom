using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DrugsQR
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool flag = false;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, "DrugsQR", out flag);
            if (flag)
            { 
                Application.Run(new Form1());
            }
            else
            {
                //Environment.Exit(1);//退出程序  
                MessageBox.Show("另一DrugsQR程序正在运行！");
            }  
        }

        private static BootParameter parseArgs(string[] args)
        {
            BootParameter bootParameter = new BootParameter();
            if (null != args && args.Length>0)
            {
                bootParameter.Host = args[0];
                if (args.Length>1)
                {
                    bootParameter.Port = int.Parse(args[1]);
                }
            }
            return bootParameter;
        }
    }
}
