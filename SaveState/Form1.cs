using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SaveState
{

    public partial class Form1 : Form
    {
        private List<Address> addresses;
        private List<Pointer> pointers;
        private String gameExecutable = "sonic2app";
        IntPtr processHandle;
        private List<byte[]> savedAddressValues;
        private List<byte[]> savedPointerValues;
        private Process[] process;
        private XInputController controller;
        Timer timer;

        const int PROCESS_ALL_ACCESS = 0x1F0FFF;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAcces, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        public Form1()
        {
            addresses = new List<Address>();
            pointers = new List<Pointer>();
            InitializeComponent();
            savedAddressValues = new List<byte[]>();
            savedPointerValues = new List<byte[]>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            controller = new XInputController();
            connectToGame();
            getAddresses();
            getPointers();
            timer = new Timer();
            timer.Interval = (10);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();

        }

        public void connectToGame()
        {
            try
            {
                process = Process.GetProcessesByName(gameExecutable);
                processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, process[0].Id);
                lblStatus.Text = "Game was found!";

            }
            catch (System.IndexOutOfRangeException ex)
            {
                Console.WriteLine(ex);
                btnLoad.Hide();
                btnSave.Hide();
                lblStatus.Text = "Did not find game.  Launch the game then click \"Search for game\"";
            }
        }

        public void getAddresses()
        {
            String line;
            StreamReader file = new StreamReader("../../Addresses.txt");
            
            while ((line = file.ReadLine()) != null)
            {
                String[] split = line.Split(',');
                String description = split[0];
                int address = Convert.ToInt32(split[1], 16);
                int numberOfBytes;

                Int32.TryParse(split[2], out numberOfBytes);

                Address temp = new Address(description, numberOfBytes, address);
                addresses.Add(temp);
            }

            file.Close();
            Console.WriteLine("DONE");
        }

        public void getPointers()
        {
            String line;
            StreamReader file = new StreamReader("../../Pointers.txt");

            while ((line = file.ReadLine()) != null)
            {
                String[] split = line.Split(',');
                String description = split[0];

                int numberOfBytes;
                Int32.TryParse(split[1], out numberOfBytes);

                IntPtr ptr = new IntPtr(Convert.ToInt32(split[2], 16));

                List<int> offsets = new List<int>();

                for (int i = 3; i < split.Length; i++)
                {
                    offsets.Add(Convert.ToInt32(split[i], 16));
                }

                Pointer temp = new Pointer(description, numberOfBytes, ptr, offsets.ToArray());

                pointers.Add(temp);
            }

            file.Close();
            Console.Write("Done");
        }

        private int getBaseAddress(Pointer ptr)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[4];
            int[] offsets = ptr.Offsets;

            ReadProcessMemory((int)processHandle, (int)ptr.Ptr, buffer, buffer.Length, ref bytesRead);
            IntPtr baseValue = new IntPtr(BitConverter.ToInt32(buffer, 0));

            for (int i = 0; i < offsets.Length - 1; i++)
            {
                IntPtr offsetAddress = IntPtr.Add(baseValue, offsets[i]);
                ReadProcessMemory((int)processHandle, (int)offsetAddress, buffer, buffer.Length, ref bytesRead);
                baseValue = new IntPtr(BitConverter.ToInt32(buffer, 0));
            }

            baseValue = IntPtr.Add(baseValue, offsets[offsets.Length - 1]);
            return baseValue.ToInt32();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            saveAddresses();
            savePointers();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            loadAddresses();
            loadPointers();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            connectToGame();
        }

        public void saveAddresses()
        {
            savedAddressValues.Clear();
            for (int i = 0; i < addresses.Count(); i++)
            {
                Address currentAddress = addresses.ElementAt(i);
                int bytesRead = 0;
                byte[] buffer = new byte[currentAddress.NumberOfBytes];

                ReadProcessMemory((int)processHandle, currentAddress.Location, buffer, buffer.Length, ref bytesRead);

                savedAddressValues.Add(buffer);
            }
        }

        public void loadAddresses()
        {
            for (int i = 0; i < addresses.Count(); i++)
            {
                Address currentAddress = addresses.ElementAt(i);
                int bytesWritten = 0;
                byte[] buffer = savedAddressValues.ElementAt(i);

                WriteProcessMemory((int)processHandle, currentAddress.Location, buffer, buffer.Length, ref bytesWritten);

            }
        }

        public void savePointers()
        {
            savedPointerValues.Clear();

            for (int i = 0; i < pointers.Count(); i++)
            {
                Pointer currentPointer = pointers.ElementAt(i);

                int bytesRead = 0;
                byte[] buffer = new byte[currentPointer.NumberOfBytes];

                int address = getBaseAddress(currentPointer);

                ReadProcessMemory((int)processHandle, address, buffer, buffer.Length, ref bytesRead);

                savedPointerValues.Add(buffer);
            }

        }

        public void loadPointers()
        {
            for (int i = 0; i < pointers.Count(); i++)
            {
                Pointer currentPointer = pointers.ElementAt(i);

                int bytesRead = 0;
                byte[] buffer = savedPointerValues.ElementAt(i);

                int address = getBaseAddress(currentPointer);

                WriteProcessMemory((int)processHandle, address, buffer, buffer.Length, ref bytesRead);

                savedPointerValues.Add(buffer);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            int button = controller.GetInput();

            /*
             * -1 means nothing
             *  0 means left bumper was pressed
             *  1 means right bumper was pressed
             */

            if (button == -1)
            {
                return;
            }
            else if (button == 0) 
            {
                saveAddresses();
                savePointers();
                lblStatus.Text = "LEFT BUMPER PRESSED";
            }
            else
            {
                loadAddresses();
                loadPointers();
                lblStatus.Text = "RIGHT BUMPER PRESSED";
            }
        }
    }
}
