using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace ASI_POS
{
    static class Program
    {
        private static Mutex singleInstanceMutex;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool createdNew;
            singleInstanceMutex = new Mutex(true,"Global\\BottleCapps_SingleInstance",out createdNew);
            if (!createdNew)
            {
                MessageBox.Show("Application is Already Running.", "Bottlecapps", MessageBoxButtons.OK);
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(args));
            singleInstanceMutex.ReleaseMutex();
        }
    }
}
