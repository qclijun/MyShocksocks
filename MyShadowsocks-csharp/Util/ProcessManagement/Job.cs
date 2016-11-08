﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Shadowsocks.Controller;
using System.Diagnostics;

namespace Shadowsocks.Util.ProcessManagement
{
    class Job : IDisposable
    {
        private IntPtr handle = IntPtr.Zero;

        public Job()
        {
            handle = CreateJobObject(IntPtr.Zero, null);
            var extendedInfoPtr = IntPtr.Zero;
            var info = new JOBOBJECT_BASIC_LIMIT_INFORMATION
            {
                LimitFlags = 0x2000
            };
            var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            {
                BasicLimitInformation = info
            };
            try
            {
                int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
                extendedInfoPtr = Marshal.AllocHGlobal(length);
                Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);
                if (!SetInformationJobObject(handle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr,
                    (uint)length))
                    throw new Exception($"Unable to set information. ERROR: {Marshal.GetLastWin32Error()}");
            }
            finally
            {
                if (extendedInfoPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(extendedInfoPtr);
                    extendedInfoPtr = IntPtr.Zero;
                }
            }
        }

        public bool AddProcess(IntPtr processHandle)
        {
            bool succ = AssignProcessToJobObject(handle, processHandle);
            if (!succ)
            {
                Logging.Error("Failed to call AssignProcessToJobObject! GetLastError=" + Marshal.GetLastWin32Error());
            }
            return succ;
        }

        public bool AddProcess(int processId)
        {
            return AddProcess(Process.GetProcessById(processId).Handle);
        }




        private IntPtr CreateJobObject(IntPtr zero, object p)
        {
            throw new NotImplementedException();
        }

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            disposed = true;
            if (disposing)
            {
                // no managed objects to free.
            }
            if (handle != IntPtr.Zero)
            {
                CloseHandle(handle);
                handle = IntPtr.Zero;
            }
        }

        ~Job()
        {
            Dispose(false);
        }

        #region Interop
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateJobObject(IntPtr a, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType infoType,
            IntPtr lpJobObjectInfo, UInt32 cbJobObjectInfoLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        #endregion
    }


    #region Helper classes

    [StructLayout(LayoutKind.Sequential)]
    struct IO_COUNTERS
    {
        public UInt64 ReadOperationCount;
        public UInt64 WriteOperationCount;
        public UInt64 OtherOperationCount;
        public UInt64 ReadTransferCount;
        public UInt64 WriteTransferCount;
        public UInt64 OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public Int64 PerProcessUserTimeLimit;
        public Int64 PerJobUserTimeLimit;
        public UInt32 LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public UInt32 ActiveProcessLimit;
        public UIntPtr Affinity;
        public UInt32 PriorityClass;
        public UInt32 SchedulingClass;
    }





    [StructLayout(LayoutKind.Sequential)]
    struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoInfo;
        public UIntPtr ProcessMemoryLimit;
        public UIntPtr JobMemoryLimit;
        public UIntPtr PeakProcessMemoryUsed;
        public UIntPtr PeakJobMemoryUsed;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SECURITY_ATTRIBUTES
    {
        public UInt32 Length;
        public IntPtr SecurityDescriptor;
        public Int32 InheritHandle;
    }

    public enum JobObjectInfoType
    {
        AssociateCompletionPortInformation = 7,
        BasicLimitInformation = 2,
        BasicUIRestrictions =4,
        EndOfJobTimeInformation = 6,
        ExtendedLimitInformation = 9,
        SecurityLimitInformation = 5,
        GroupInformation = 11
    }
    #endregion
}
