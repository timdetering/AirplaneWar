using System;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using DSound=Microsoft.DirectX.DirectSound;

namespace AirplaneWar
{
	/// <summary>
	/// Summary description for ShootingClass.
	/// </summary>
	public class ShootingClass
	{
		DSound.Device dsoundDevice;
		private DSound.SecondaryBuffer dsoundBuffer=null;
		shot gunshot=null;
		Hashtable Directions;

		public ShootingClass(Device d3dDevice, DSound.Device dsoundD)
		{
			Hashtable aux=new Hashtable();
			Directions=Hashtable.Synchronized(aux);
			dsoundDevice=dsoundD;
			dsoundBuffer=new DSound.SecondaryBuffer(@"media\sound\machgun.wav",dsoundDevice);
			gunshot=new shot(d3dDevice);
		}
		public void Render() 
		{
			lock( Directions.SyncRoot ) 
			{
				foreach (DictionaryEntry en in Directions) 
				{
					Vector3 pos=(Vector3)gunshot.Positions[en.Key];
					Vector3 speedV=(Vector3)en.Value;
					pos.Add(speedV);
					gunshot.Positions[en.Key]=pos;
				} 
			}

			gunshot.Render();
		}
		public void NewShot(Vector3 pos, Vector3 direction) 
		{
			dsoundBuffer.Play(0,DSound.BufferPlayFlags.Default);
			int id=Directions.Count;
		    Directions[id]=direction;
			gunshot.Directions[id]=direction;
			gunshot.Positions[id]=pos;
		}
	}
}
