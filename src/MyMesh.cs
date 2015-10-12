using System;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace AirplaneWar
{
	/// <summary>
	/// Summary description for Mesh.
	/// </summary>
	public class MyMesh
	{
		protected Material [] MeshMaterials;
		protected Texture [] MeshTextures;
		protected Mesh mesh;
		protected Device d3dDevice;
		protected Matrix matWorld=new Matrix();

		public MyMesh(Device d3device)
		{
			d3dDevice=d3device;
			matWorld.Translate(0f,0f,0f);
		}
	}
}
