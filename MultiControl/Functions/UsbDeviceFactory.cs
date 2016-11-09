﻿/* ======================================================================== 
 * 描述信息 
 *  
 * 作者：Ivan JL Zhang       
 * 时间：2016/11/8 14:15:17 
 * 文件名：UsbDeviceFactory 
 * 版本：V1.0.0 
 * 文件说明：
 * 
 * 
 * 修改者：           
 * 时间：               
 * 修改说明： 
* ======================================================================== 
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using LibUsbDotNet.WinUsb;
using MultiControl.Common;
using MultiControl.Lib;

namespace MultiControl.Functions
{
    class UsbDeviceFactory
    {
        CMDHelper cmd = new CMDHelper();
        public async Task<string> FindArrivaledDevice(string serialNumber)
        {
            int count = 0;
            while (count <= config_inc.CMD_REPEAT_MAX_TIME)
            {
                UsbRegDeviceList allDevices = UsbDevice.AllDevices;
                Debug.WriteLine($"find devices {allDevices.Count}");
                await CMDHelper.Adb_KillServer();
                for (int index = 0; index < allDevices.Count; index++)
                {
                    var device = (WinUsbRegistry)allDevices[index];
                    UsbDevice usbDevice;
                    bool result = device.Open(out usbDevice);
                    Debug.WriteLine($"open device registry info page:{result}");
                    if (result)
                    {
                        if (serialNumber == usbDevice.Info.SerialString)
                        {
                            string[] locationPaths = (string[])device.DeviceProperties["LocationPaths"];
                            usbDevice.Close();
                            CMDHelper.Adb_StartServer();
                            return filterUsbPort(locationPaths[0]);
                        }
                    }
                    if (usbDevice != null)
                        usbDevice.Close();
                }
                count++;
                await Task.Delay(500);
            }
            CMDHelper.Adb_StartServer();
            return String.Empty;
        }
        /// <summary>
        /// 已知Bug: 三星手机在识别端口的时候会多一阶
        /// </summary>
        /// <param name="locationPath"></param>
        /// <returns></returns>
        private string filterUsbPort(string locationPath)
        {
            string usb_port = String.Empty;
            string[] arr = locationPath.Split('#');
            int count = 0;
            foreach (var node in arr)
            {
                if (node.Contains("USB("))
                {
                    int port = -1;
                    Int32.TryParse(node.Substring(4, 1), out port);
                    if (port > -1)
                    {
                        count++;
                        usb_port += "#" + port.ToString("D4");
                    }
                    if (count >= 2)
                        break;
                }
            }
            return usb_port;
        }
    }
}
