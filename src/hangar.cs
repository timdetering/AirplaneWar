using System;
using System.Collections;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace AirplaneWar
{
	/// <summary>
	/// Summary description for shot.
	/// </summary>
	public class hangar
	{
		public Hashtable meshes=new Hashtable();
		bool drawing=false;
		public hangar(Device d3device)
		{
			StreamReader sr=new StreamReader(@"media\objects.dat");
			string line;
			while ((line = sr.ReadLine()) != null) 
			{
				// if not a comment
				if (!line.StartsWith("#")) 
				{
					// the format is:
					// #path file collision positionx y z rotationdegreex y z
					string [] data=line.Split(new char[] {' '});

					// if the mesh is already in memory, don't reload it
					// just add a new location to it.
					meshobject mymesh;
					if (meshes.Contains(data[1]))
						mymesh=(meshobject)meshes[data[1]];
					else
						mymesh=new meshobject(d3device,data[0],data[1]);
					// remember data[2] is to know if you can collide with it or not
					float y = float.Parse(data[4]);
					if (Form1.terrainFlat)
						y-=112f;
					mymesh.addlocation(float.Parse(data[3]),y,float.Parse(data[5]),float.Parse(data[6]),float.Parse(data[7]),float.Parse(data[8]));
					meshes[data[1]]=mymesh;
				}
			}
			sr.Close();
		}

		public void Render() 
		{
			if (drawing)
				return;

			drawing=true;

			foreach (DictionaryEntry en in meshes) 
			{
				((meshobject)en.Value).Render();
			}

			drawing=false;
		}
	}
}
