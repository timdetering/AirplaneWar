using System;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace AirplaneWar
{
	/// <summary>
	/// Summary description for LoadTerrainGrid.
	/// </summary>
	public class LoadTerrainGrid
	{
		protected Device d3dDevice;
		int grid;
		MapMesh [] map;

		// Custom vertex type
/*		public struct MeshVertex
		{
			public Vector3 p;
			public Vector3 n;
		//	public float tu, tv;
			public static readonly VertexFormats Format = VertexFormats.Position | VertexFormats.Normal; // | VertexFormats.Texture1;
		};
*/

		public LoadTerrainGrid(Device d3, MapMesh [] m, int x)
		{
			d3dDevice=d3;
			grid=x;
			map=m;
		}
		public void LoadGrid(int x) 
		{
			grid=x;
			LoadGrid();
		}
		public void LoadGrid() 
		{
			Mesh mesh=null;

			try 
			{
				GraphicsStream o;
				mesh = Mesh.FromFile(@"media\terrain\terreno"+(grid+1)+"_01.x",MeshFlags.SystemMemory, d3dDevice, out o); // , out o, out materials);
				mesh.OptimizeInPlace(MeshFlags.OptimizeCompact | MeshFlags.OptimizeAttrSort | MeshFlags.OptimizeVertexCache, o);
				o.Close();
				o=null;


					/*					VertexBuffer vertices=new VertexBuffer( typeof (CustomVertex.PositionOnly), 6, d3dDevice,0, CustomVertex.PositionOnly.Format, Pool.SystemMemory); 



										// Create rectangle 
										CustomVertex.PositionOnly [] verts = (CustomVertex.PositionOnly[]) vertices.Lock(0, 0); 

										verts[0] = new CustomVertex.PositionOnly(map[grid].max.X,0,map[grid].min.Z); 
										verts[1] = new CustomVertex.PositionOnly(map[grid].min.X,0,map[grid].min.Z);
										verts[2] = new CustomVertex.PositionOnly(map[grid].min.X,0,map[grid].max.Z);

										verts[2] = new CustomVertex.PositionOnly(map[grid].min.X,0,map[grid].max.Z);
										verts[2] = new CustomVertex.PositionOnly(map[grid].max.X,0,map[grid].max.Z);
										verts[2] = new CustomVertex.PositionOnly(map[grid].max.X,0,map[grid].min.Z);
										Mesh.


										vertices.Unlock(); 

					*/
//					mesh = Mesh.FromFile(@"media\terrain\single.x",MeshFlags.SystemMemory, d3dDevice); // , out o, out materials);

//					mesh=Mesh.Box(d3dDevice,294910.3f,1000f,294910.3f);
					// Lock the vertex buffer to generate a simple bounding box
				/*	VertexBuffer vertBuffer = mesh.VertexBuffer;
					int numVertices=mesh.NumberVertices;
					MeshVertex [] vertices;

					vertices = (MeshVertex [])vertBuffer.Lock(0, typeof(MeshVertex),0, numVertices);
					for (int i = 0;i < numVertices;i++) 
					{
						vertices[i].p.X+=map[grid].min.X;
						vertices[i].p.Y+=1;
						vertices[i].p.Z+=map[grid].min.Z;
					}
						
					vertBuffer.Unlock(); 
					vertBuffer.Dispose(); */

				map[grid].mesh=mesh;
			} 
			catch 
			{
				map[grid].mesh=null;
				map[grid].loading=false;
				return;
			}
	
			// verify validity of mesh for simplification
		//	mesh.Validate(o);

	//		mesh.OptimizeInPlace(MeshFlags.OptimizeCompact | MeshFlags.OptimizeAttrSort | MeshFlags.OptimizeVertexCache, o);


			//	Before getting this information from the material buffer, the material and texture arrays must be resized to fit all the materials and textures for this mesh.
/*
			map[grid].MeshMaterials=new Material[materials.Length]; // Mesh Material data
			map[grid].MeshTextures=new Texture[materials.Length];  // ' Mesh Textures
*/
			// Lock the vertex buffer to generate a simple bounding box
		/*	VertexBuffer vb = mesh.VertexBuffer;
			GraphicsStream g=vb.Lock(0, 0, LockFlags.NoSystemLock);
			Geometry.ComputeBoundingBox(g,mesh.NumberVertices, mesh.VertexFormat, out map[grid].min,out map[grid].max);
		//	mesh.UnlockVertexBuffer();
			vb.Unlock();
			vb.Dispose(); */
		//	g.Close();
		//	g=null;


	/*		StreamWriter sw=new StreamWriter(@"media\terrenoinfo.dat",true);
			sw.WriteLine("#"+grid);
			sw.WriteLine(map[grid].min.X);
			sw.WriteLine(map[grid].min.Y);
			sw.WriteLine(map[grid].min.Z);
			sw.WriteLine(map[grid].max.X);
			sw.WriteLine(map[grid].max.Y);
			sw.WriteLine(map[grid].max.Z);
			sw.Close();
*/
/*			int i;
			for(i = 0;i<materials.Length;i++) 
			{
    
				// Copy the material using the d3dx helper function
				map[grid].MeshMaterials[i]=materials[i].Material3D;

				// Set the ambient color for the material (D3DX does not do this)
				map[grid].MeshMaterials[i].Ambient = map[grid].MeshMaterials[i].Diffuse;
     
			}
*/
		//	o.Close();
		//	o=null;
			map[grid].loading=false;
		}
	}
}
