#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V2;

using VVVV.Core.Logging;

using DisplayInfoWMIProvider;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "ScreenInfo", Category = "Device", Version = "Advanced", Help = "Basic template with one string in/out", Tags = "")]
	#endregion PluginInfo
	public class AdvancedDeviceScreenInfoNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Update", IsSingle = true, IsBang = true)]
		IDiffSpread<bool> Update;
		
		[Output("Model")]
		ISpread<string> Model;
		
		[Output("Monitor ID")]
		ISpread<string> MonitorID;
		
		[Output("PnP ID")]
		ISpread<string> PnPID;
		
		[Output("Serial Number")]
		ISpread<string> SerialNumber;
		
		[Import()]
		ILogger FLogger;
		#endregion fields & pins
		

    [DllImport("user32.dll")]
    static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

    [Flags()]
    public enum DisplayDeviceStateFlags : int
    {
        /// <summary>The device is part of the desktop.</summary>
        AttachedToDesktop = 0x1,
        MultiDriver = 0x2,
        /// <summary>The device is part of the desktop.</summary>
        PrimaryDevice = 0x4,
        /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
        MirroringDriver = 0x8,
        /// <summary>The device is VGA compatible.</summary>
        VGACompatible = 0x10,
        /// <summary>The device is removable; it cannot be the primary display.</summary>
        Removable = 0x20,
        /// <summary>The device has more display modes than its output devices support.</summary>
        ModesPruned = 0x8000000,
        Remote = 0x4000000,
        Disconnect = 0x2000000
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DISPLAY_DEVICE
    {
        [MarshalAs(UnmanagedType.U4)]
        public int cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        [MarshalAs(UnmanagedType.U4)]
        public DisplayDeviceStateFlags StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    static public IEnumerable<DISPLAY_DEVICE> GetDisplays()
    {
        DISPLAY_DEVICE d = new DISPLAY_DEVICE();
        d.cb = Marshal.SizeOf(d);
    		for (uint id = 0; EnumDisplayDevices(null, id, ref d, 0); id++)
            {
                if (d.StateFlags.HasFlag(DisplayDeviceStateFlags.AttachedToDesktop))
                {
    	        	yield return d;
                    d.cb = Marshal.SizeOf(d);
                    EnumDisplayDevices(d.DeviceName, 0, ref d, 0);
	            	yield return d;
                }
                d.cb = Marshal.SizeOf(d);
            }
    }
		
		public void Evaluate(int SpreadMax)
		{
			if(Update.IsChanged && Update[0]) {
				Model.SliceCount = MonitorID.SliceCount = PnPID.SliceCount = SerialNumber.SliceCount = 0;
//				foreach(DisplayDetails dd in DisplayDetails.GetMonitorDetails()) {
//					Model.Add(dd.Model);
//					MonitorID.Add(dd.MonitorID);
//					PnPID.Add(dd.PnPID);
//					SerialNumber.Add(dd.SerialNumber);
//				}
				foreach(DISPLAY_DEVICE dd in GetDisplays()) {
					Model.Add(dd.DeviceName);
					MonitorID.Add(dd.DeviceID);
					PnPID.Add(dd.DeviceString);
					
				}
			}
		}
	}
}
