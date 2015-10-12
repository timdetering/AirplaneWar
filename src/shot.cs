using System;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace AirplaneWar
{
	/// <summary>
	/// Summary description for shot.
	/// </summary>
	public class shot : MyMesh
	{
		public Hashtable Positions=new Hashtable();
		public Hashtable Directions=new Hashtable();
		bool drawing=false;

		public shot(Device d3device):base(d3device)
		{
			Hashtable aux=new Hashtable();
			Positions=Hashtable.Synchronized(aux);
			mesh=Mesh.Box(d3dDevice,0.1f,0.1f,0.7f);
		}

		public void Render() 
		{
			if (drawing)
				return;
			drawing=true;
			lock( Positions.SyncRoot ) 
			{
				foreach (DictionaryEntry en in Positions) 
				{
					Matrix lookat=Matrix.LookAtLH((Vector3)en.Value,Vector3.Add((Vector3)en.Value,(Vector3)Directions[en.Key]),new Vector3(0f,1f,0f));
					lookat.Invert();
					d3dDevice.SetTransform(TransformType.World, lookat);
					// Rendering of scene objects occur here.
					// real render starts
					Material mtrl = new Material();
					mtrl.Ambient = mtrl.Diffuse = System.Drawing.Color.White;
					d3dDevice.Material = mtrl;
					d3dDevice.SetTexture(0,null);

					// Draw the mesh subset
					mesh.DrawSubset(0);
				}
			}
			drawing=false;
		}
	}
}
