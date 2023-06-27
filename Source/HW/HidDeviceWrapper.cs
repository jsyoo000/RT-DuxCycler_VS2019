using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;

namespace Duxcycler.Source.HW
{
    public class HidDeviceWrapper : IDisposable
    {
        private HidDevice device;
        private byte[] buffer;

        /// <summary>
        /// Data Receive Handler
        /// </summary>
        public event Action<byte[]> DataReceived;
        public bool IsConnect = false;
        public string SeringNum;
        public int VID;
        public int PID;

        public HidDeviceWrapper(int vendorId, int productId)
        {
            device = HidDevices.Enumerate().FirstOrDefault(d => d.Attributes.VendorId == vendorId && d.Attributes.ProductId == productId);

            if (device == null)
            {
                IsConnect = false;
            }
            else
            {
                VID = vendorId;
                PID = productId;

                IsConnect = true;
                buffer = new byte[device.Capabilities.InputReportByteLength];
            }
        }

        public void Open()
        {
            device.OpenDevice();

            byte[] serialDataBuffer;
            if (device != null)
            {
                if (device.ReadSerialNumber(out serialDataBuffer))
                {
                    SeringNum = Encoding.Unicode.GetString(serialDataBuffer);
                }
            }

            device.Inserted += DeviceInserted;
            device.Removed += DeviceRemoved;
            device.MonitorDeviceEvents = true;
            device.ReadReport(OnReport);

        }

        public void Close()
        {
            device.CloseDevice();
        }

        public void Write(byte[] data)
        {
            if (device.IsOpen)
            {
                const int TX_BUFSIZE = 65;
                byte[] Tx_Buffer = new byte[TX_BUFSIZE];
                Array.Copy(data, 0, Tx_Buffer, 0, data.Length);
                device.Write(Tx_Buffer);
            }
        }

        /// <summary>
        /// device Inserted event handler
        /// </summary>
        private void DeviceInserted()
        {
            DeviceConnectEvent?.Invoke(this, null);
        }

        /// <summary>
        /// device Remove event handler
        /// </summary>
        private void DeviceRemoved()
        {
            DeviceDisconnectEvent?.Invoke(this, null);
        }

        private void OnReport(HidReport report)
        {
            if (report.Data.Length > 0)
            {
                byte[] data = report.Data;
                DataReceived?.Invoke(data);
            }

            device.ReadReport(OnReport);
        }

        public void Dispose()
        {
            if (device != null)
            {
                device.Dispose();
                device = null;
            }
        }

        public event EventHandler DeviceDisconnectEvent;
        public event EventHandler DeviceConnectEvent;
    }
}
