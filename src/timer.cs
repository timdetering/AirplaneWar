using System;
using System.Runtime.InteropServices;

namespace AirplaneWar
{
	/// <summary>
	/// Calculate the time between a start and stop call.
	/// </summary>
	public class timer
	{
		private long start, stop, freq; // time between renders

		public timer()
		{
			if (QueryPerformanceFrequency(out freq) == false)
			{
				throw new System.ComponentModel.Win32Exception();
			}
		}

		public void Start() 
		{
			QueryPerformanceCounter(out start);
		}

		public void Stop() 
		{
			QueryPerformanceCounter(out stop);
		}

		public double Time() 
		{
			return (double)(stop - start) / (double) freq;
		}

		[DllImport("Kernel32.dll")]
		private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

		[DllImport("Kernel32.dll")]
		private static extern bool QueryPerformanceFrequency(out long lpFrequency);

	}
}
