using System;
using System.Runtime.InteropServices;

namespace Auto_Serial_Downloader
{
    internal static class CyUsbSerialNative
    {
        private const string DllName = "cyusbserial.dll";
        private const CallingConvention CC = CallingConvention.Cdecl; // if needed, change to StdCall

        public const int CY_STRING_DESCRIPTOR_SIZE = 256;
        public const int CY_MAX_DEVICE_INTERFACE = 5;

        public enum CY_RETURN_STATUS : int
        {
            CY_SUCCESS = 0,
            CY_ERROR_ACCESS_DENIED,
            CY_ERROR_DRIVER_INIT_FAILED,
            CY_ERROR_DEVICE_INFO_FETCH_FAILED,
            CY_ERROR_DRIVER_OPEN_FAILED,
            CY_ERROR_INVALID_PARAMETER,
            CY_ERROR_REQUEST_FAILED,
            CY_ERROR_DOWNLOAD_FAILED,
            CY_ERROR_FIRMWARE_INVALID_SIGNATURE,
            CY_ERROR_INVALID_FIRMWARE,
            CY_ERROR_DEVICE_NOT_FOUND,
            CY_ERROR_IO_TIMEOUT,
            CY_ERROR_PIPE_HALTED,
            CY_ERROR_BUFFER_OVERFLOW,
            CY_ERROR_INVALID_HANDLE,
            CY_ERROR_ALLOCATION_FAILED,
            CY_ERROR_I2C_DEVICE_BUSY,
            CY_ERROR_I2C_NAK_ERROR,
            CY_ERROR_I2C_ARBITRATION_ERROR,
            CY_ERROR_I2C_BUS_ERROR,
            CY_ERROR_I2C_BUS_BUSY,
            CY_ERROR_I2C_STOP_BIT_SET,
            CY_ERROR_STATUS_MONITOR_EXIST
        }

        public enum CY_DEVICE_CLASS : int
        {
            CY_CLASS_DISABLED = 0,
            CY_CLASS_CDC = 0x02,
            CY_CLASS_PHDC = 0x0F,
            CY_CLASS_VENDOR = 0xFF
        }

        public enum CY_DEVICE_TYPE : int
        {
            CY_TYPE_DISABLED = 0,
            CY_TYPE_UART,
            CY_TYPE_SPI,
            CY_TYPE_I2C,
            CY_TYPE_JTAG,
            CY_TYPE_MFG
        }

        public enum CY_DEVICE_SERIAL_BLOCK : int
        {
            SerialBlock_SCB0 = 0,
            SerialBlock_SCB1,
            SerialBlock_MFG
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CY_VID_PID
        {
            public ushort vid;
            public ushort pid;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CY_DEVICE_INFO
        {
            public CY_VID_PID vidPid;
            public byte numInterfaces;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CY_STRING_DESCRIPTOR_SIZE)]
            public byte[] manufacturerName;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CY_STRING_DESCRIPTOR_SIZE)]
            public byte[] productName;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CY_STRING_DESCRIPTOR_SIZE)]
            public byte[] serialNum;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CY_STRING_DESCRIPTOR_SIZE)]
            public byte[] deviceFriendlyName;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CY_MAX_DEVICE_INTERFACE)]
            public CY_DEVICE_TYPE[] deviceType;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CY_MAX_DEVICE_INTERFACE)]
            public CY_DEVICE_CLASS[] deviceClass;

            public CY_DEVICE_SERIAL_BLOCK deviceBlock;
        }

        [DllImport(DllName, CallingConvention = CC)]
        public static extern CY_RETURN_STATUS CyGetListofDevices(out byte numDevices);

        [DllImport(DllName, CallingConvention = CC)]
        public static extern CY_RETURN_STATUS CyGetDeviceInfoVidPid(
            CY_VID_PID vidPid,
            [Out] byte[] deviceIdList,
            [Out] CY_DEVICE_INFO[] deviceInfoList,
            out byte deviceCount,
            byte infoListLength);

        [DllImport(DllName, CallingConvention = CC)]
        public static extern CY_RETURN_STATUS CyOpen(byte deviceNumber, byte interfaceNum, out IntPtr handle);

        [DllImport(DllName, CallingConvention = CC)]
        public static extern CY_RETURN_STATUS CyClose(IntPtr handle);
    }
}