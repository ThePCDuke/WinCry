using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;

namespace WinCry.Models
{
    class RunAsProcess
    {
        #region kernel32.dll Data

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CreateProcess(
            string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags,
            IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFOEX lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern UInt32 WaitForSingleObject(IntPtr handle, UInt32 milliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UpdateProcThreadAttribute(
            IntPtr lpAttributeList, uint dwFlags, IntPtr Attribute, IntPtr lpValue,
            IntPtr cbSize, IntPtr lpPreviousValue, IntPtr lpReturnSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool InitializeProcThreadAttributeList(
            IntPtr lpAttributeList, int dwAttributeCount, int dwFlags, ref IntPtr lpSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteProcThreadAttributeList(IntPtr lpAttributeList);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetHandleInformation(IntPtr hObject, HANDLE_FLAGS dwMask,
           HANDLE_FLAGS dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool PeekNamedPipe(IntPtr handle,
            IntPtr buffer, IntPtr nBufferSize, IntPtr bytesRead,
            ref uint bytesAvail, IntPtr BytesLeftThisMessage);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DuplicateHandle(IntPtr hSourceProcessHandle,
           IntPtr hSourceHandle, IntPtr hTargetProcessHandle, ref IntPtr lpTargetHandle,
           uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwOptions);

        [DllImport("kernel32.dll")]
        static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe,
           ref SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize);

        [DllImport("kernel32.dll")]
        public static extern int GetSystemDefaultLCID();

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFOEX
        {
            public STARTUPINFO StartupInfo;
            public IntPtr lpAttributeList;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            [MarshalAs(UnmanagedType.Bool)]
            public bool bInheritHandle;
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        [Flags]
        enum HANDLE_FLAGS : uint
        {
            None = 0,
            INHERIT = 1,
            PROTECT_FROM_CLOSE = 2
        }

        [Flags]
        public enum DuplicateOptions : uint
        {
            DUPLICATE_CLOSE_SOURCE = 0x00000001,
            DUPLICATE_SAME_ACCESS = 0x00000002
        }

        #endregion

        #region Public Properties

        public static bool IsTrustedInstallerServiceRunning
        {
            get
            {
                using (ServiceController controller = new ServiceController("TrustedInstaller"))
                {
                    if (controller.Status == ServiceControllerStatus.Running)
                        return true;
                    else return false;
                }
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Tries to start 'TrustedInstaller' service
        /// </summary>
        public static void StartTrustedInstallerService()
        {
            if (IsTrustedInstallerServiceRunning)
                return;

            using (ServiceController controller = new ServiceController("TrustedInstaller"))
            {
                if (controller.StartType == ServiceStartMode.Disabled)
                    Helpers.RunByCMD($"sc config TrustedInstaller start= auto");

                if (controller.Status == ServiceControllerStatus.Stopped)
                    controller.Start();
            }

            byte tryCount = 0;

            while (!IsTrustedInstallerServiceRunning)
            {
                System.Threading.Thread.Sleep(500);
                tryCount++;

                if (tryCount >= 30)
                {
                    return;
                }
            }
        }

        public static void ExecutePowershellCommand(string command, bool hidden = false, bool waitForExit = true)
        {
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                UseShellExecute = false,
                CreateNoWindow = hidden,
                RedirectStandardOutput = true,
                Arguments = command
            };

            Process process = Process.Start(info);

            if (waitForExit)
            {
                process.WaitForExit();
            }
        }

        /// <summary>
        /// Starts up CMD instance as child of given process
        /// </summary>
        /// <param name="args">CMD Arguments</param>
        /// <param name="hidden">Launch hidden</param>
        /// <param name="waitForExit">Wait for exit</param>
        /// <param name="processName">Parent process' name</param>
        /// <returns></returns>
        public static string CMD(string args, bool hidden = false, bool waitForExit = true, string processName = "TrustedInstaller")
        {
            string _binaryPath = $@"{Path.GetPathRoot(Environment.SystemDirectory)}\Windows\System32\cmd.exe";
            string _args = $@"/c {args} & exit";
            int parentProcessId;

            Process[] explorerproc = Process.GetProcessesByName(processName);
            if (explorerproc.Length == 0 && processName == "TrustedInstaller")
            {
                if (!IsTrustedInstallerServiceRunning)
                    StartTrustedInstallerService();

                explorerproc = Process.GetProcessesByName(processName);
            }

            parentProcessId = explorerproc[0].Id;

            return CreateProcess(parentProcessId, _binaryPath, hidden, waitForExit, true, _args);
        }

        /// <summary>
        /// Starts up any executable as child of given process
        /// </summary>
        /// <param name="binaryPath">Executable's path</param>
        /// <param name="hidden">Launch hidden</param>
        /// <param name="waitForExit">Wait for exit</param>
        /// <param name="processName">Parent process' name</param>
        public static void Binary(string binaryPath, bool hidden = false, bool waitForExit = false, string processName = "TrustedInstaller")
        {
            string _binaryPath = binaryPath;
            int parentProcessId;

            Process[] explorerproc = Process.GetProcessesByName(processName);
            if (explorerproc.Length == 0 && processName == "TrustedInstaller")
            {
                if (!IsTrustedInstallerServiceRunning)
                    StartTrustedInstallerService();

                explorerproc = Process.GetProcessesByName(processName);
            }

            parentProcessId = explorerproc[0].Id;
            CreateProcess(parentProcessId, _binaryPath, hidden, waitForExit);
        }

        /// <summary>
        /// Creates process as a child of given proocess
        /// </summary>
        /// <param name="parentProcessId">ID of parent process</param>
        /// <param name="binaryPath">Path to executable</param>
        /// <param name="hidden">Launch hidden</param>
        /// <param name="waitForExit">Wait for exit</param>
        /// <param name="captureOutput">Capture output</param>
        /// <param name="arguments">Executable arguments</param>
        /// <returns></returns>
        public static string CreateProcess(int parentProcessId, string binaryPath, bool hidden = false, bool waitForExit = true, bool captureOutput = false, string arguments = null)
        {
            // STARTUPINFOEX members
            const int PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;
            const int PROC_THREAD_ATTRIBUTE_MITIGATION_POLICY = 0x00020007;

            // Block non-Microsoft signed DLL's
            const long PROCESS_CREATION_MITIGATION_POLICY_BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON = 0x100000000000;

            // STARTUPINFO members (dwFlags and wShowWindow)
            const int STARTF_USESTDHANDLES = 0x00000100;
            const int STARTF_USESHOWWINDOW = 0x00000001;
            const short SW_HIDE = 0x0000;

            // dwCreationFlags
            const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
            const uint CREATE_NEW_CONSOLE = 0x00000010;

            // DuplicateHandle
            const uint DUPLICATE_CLOSE_SOURCE = 0x00000001;
            const uint DUPLICATE_SAME_ACCESS = 0x00000002;

            // https://msdn.microsoft.com/en-us/library/ms682499(VS.85).aspx
            // Handle stuff
            var saHandles = new SECURITY_ATTRIBUTES();
            saHandles.nLength = Marshal.SizeOf(saHandles);
            saHandles.bInheritHandle = true;
            saHandles.lpSecurityDescriptor = IntPtr.Zero;

            // Duplicate handle created just in case
            IntPtr hDupStdOutWrite = IntPtr.Zero;

            // Create the pipe and make sure read is not inheritable
            CreatePipe(out IntPtr hStdOutRead, out IntPtr hStdOutWrite, ref saHandles, 0);
            SetHandleInformation(hStdOutRead, HANDLE_FLAGS.INHERIT, 0);

            var pInfo = new PROCESS_INFORMATION();
            var siEx = new STARTUPINFOEX();

            // Be sure to set the cb member of the STARTUPINFO structure to sizeof(STARTUPINFOEX).
            siEx.StartupInfo.cb = Marshal.SizeOf(siEx);
            IntPtr lpValueProc = IntPtr.Zero;

            // Values will be overwritten if parentProcessId > 0
            siEx.StartupInfo.hStdError = hStdOutWrite;
            siEx.StartupInfo.hStdOutput = hStdOutWrite;

            try
            {
                if (parentProcessId > 0)
                {
                    var lpSize = IntPtr.Zero;
                    var success = InitializeProcThreadAttributeList(IntPtr.Zero, 2, 0, ref lpSize);
                    if (success || lpSize == IntPtr.Zero)
                    {
                        return "Error";
                    }

                    siEx.lpAttributeList = Marshal.AllocHGlobal(lpSize);
                    success = InitializeProcThreadAttributeList(siEx.lpAttributeList, 2, 0, ref lpSize);
                    if (!success)
                    {
                        return "Error";
                    }

                    IntPtr lpMitigationPolicy = Marshal.AllocHGlobal(IntPtr.Size);
                    Marshal.WriteInt64(lpMitigationPolicy, PROCESS_CREATION_MITIGATION_POLICY_BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON);

                    // Add Microsoft-only DLL protection
                    success = UpdateProcThreadAttribute(siEx.lpAttributeList, 0,
                                                        (IntPtr)PROC_THREAD_ATTRIBUTE_MITIGATION_POLICY,
                                                        lpMitigationPolicy, (IntPtr)IntPtr.Size, IntPtr.Zero,
                                                        IntPtr.Zero);
                    if (!success)
                    {
                        throw new Exception("Failed to set process mitigation policy!");
                    }

                    IntPtr parentHandle = OpenProcess(ProcessAccessFlags.CreateProcess | ProcessAccessFlags.DuplicateHandle, false, parentProcessId);
                    // This value should persist until the attribute list is destroyed using the DeleteProcThreadAttributeList function
                    lpValueProc = Marshal.AllocHGlobal(IntPtr.Size);
                    Marshal.WriteIntPtr(lpValueProc, parentHandle);

                    success = UpdateProcThreadAttribute(siEx.lpAttributeList, 0,
                                                        (IntPtr)PROC_THREAD_ATTRIBUTE_PARENT_PROCESS, lpValueProc,
                                                        (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero);
                    if (!success)
                    {
                        throw new Exception("Could not create process!");
                    }

                    IntPtr hCurrent = Process.GetCurrentProcess().Handle;
                    IntPtr hNewParent = OpenProcess(ProcessAccessFlags.DuplicateHandle, true, parentProcessId);

                    success = DuplicateHandle(hCurrent, hStdOutWrite, hNewParent, ref hDupStdOutWrite, 0, true, DUPLICATE_CLOSE_SOURCE | DUPLICATE_SAME_ACCESS);
                    if (!success)
                    {
                        throw new Exception("Could not create process!");
                    }

                    siEx.StartupInfo.hStdError = hDupStdOutWrite;
                    siEx.StartupInfo.hStdOutput = hDupStdOutWrite;
                }

                if (hidden)
                {
                    siEx.StartupInfo.dwFlags = STARTF_USESHOWWINDOW | STARTF_USESTDHANDLES;
                    siEx.StartupInfo.wShowWindow = SW_HIDE;
                }

                var ps = new SECURITY_ATTRIBUTES();
                var ts = new SECURITY_ATTRIBUTES();
                ps.nLength = Marshal.SizeOf(ps);
                ts.nLength = Marshal.SizeOf(ts);

                bool ret = CreateProcess(binaryPath, arguments, ref ps, ref ts, true, EXTENDED_STARTUPINFO_PRESENT | CREATE_NEW_CONSOLE, IntPtr.Zero, null, ref siEx, out pInfo);
                if (!ret)
                {
                    throw new Exception("Could not create process!");
                }

                int tryCount = 0;

                if (!captureOutput && waitForExit)
                {
                    do
                    {
                        if (WaitForSingleObject(pInfo.hProcess, 100) == 0)
                        {
                            return "Done";
                        }

                        tryCount++;

                        if (tryCount >= 5)
                        {
                            return "Timeout";
                        }
                    }
                    while (true);
                }

                if (!waitForExit)
                    return "Done";

                SafeFileHandle safeHandle = new SafeFileHandle(hStdOutRead, false);

                var cp = System.Globalization.CultureInfo.GetCultureInfo(GetSystemDefaultLCID()).TextInfo.OEMCodePage;
                var encoding = Encoding.GetEncoding(cp);

                var reader = new StreamReader(new FileStream(safeHandle, FileAccess.Read, 4096, false), encoding, true);
                string result = "";
                bool exit = false;
                try
                {
                    do
                    {
                        if (WaitForSingleObject(pInfo.hProcess, 100) == 0)
                        {
                            exit = true;
                        }

                        char[] buf = null;
                        int bytesRead;

                        uint bytesToRead = 0;

                        bool peekRet = PeekNamedPipe(hStdOutRead, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref bytesToRead, IntPtr.Zero);

                        if (peekRet == true && bytesToRead == 0)
                        {
                            if (exit == true)
                            {
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (bytesToRead > 4096)
                            bytesToRead = 4096;

                        buf = new char[bytesToRead];
                        bytesRead = reader.Read(buf, 0, buf.Length);
                        if (bytesRead > 0)
                        {
                            var _string = new string(buf);

                            var _charsToRemove = new string[] { "\n", "\r" };
                            foreach (var c in _charsToRemove)
                            {
                                _string = _string.Replace(c, string.Empty);
                            }

                            result += _string;
                        }

                        tryCount++;

                        if (tryCount >= 5)
                        {
                            result = "Timeout";
                            break;
                        }

                    } while (true);
                    reader.Close();
                }
                finally
                {
                    if (!safeHandle.IsClosed)
                    {
                        safeHandle.Close();
                    }
                }
                if (hStdOutRead != IntPtr.Zero)
                {
                    CloseHandle(hStdOutRead);
                }
                return result;
            }
            finally
            {
                // Free the attribute list
                if (siEx.lpAttributeList != IntPtr.Zero)
                {
                    DeleteProcThreadAttributeList(siEx.lpAttributeList);
                    Marshal.FreeHGlobal(siEx.lpAttributeList);
                }
                Marshal.FreeHGlobal(lpValueProc);

                // Close process and thread handles
                if (pInfo.hProcess != IntPtr.Zero)
                {
                    CloseHandle(pInfo.hProcess);
                }
                if (pInfo.hThread != IntPtr.Zero)
                {
                    CloseHandle(pInfo.hThread);
                }
            }
        }

        #endregion
    }
}