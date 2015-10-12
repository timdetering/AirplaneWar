using System;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace AirplaneWar
{
	/// <summary>
	/// Summary description for meshobjects.
	/// </summary>
	public class meshobject : MyMesh
	{
		public ArrayList Positions=new ArrayList();
		bool drawing=false;
		public meshobject(Device d3device, string path, string file):base(d3device)
		{
			ExtendedMaterial[] materials=null;

			GraphicsStream o=null;
			mesh = Mesh.FromFile(path+file,MeshFlags.SystemMemory, d3dDevice, out o, out materials);

			//	Before getting this information from the material buffer, the material and texture arrays must be resized to fit all the materials and textures for this mesh.
			MeshMaterials=new Material[materials.Length]; // Mesh Material data
			MeshTextures=new Texture[materials.Length];  // ' Mesh Textures

			/*	GraphicsStream g=mesh.LockVertexBuffer(LockFlags.ReadOnly);
				Geometry.ComputeBoundingBox(g,mesh.NumberVertices, mesh.VertexFormat, out min,out max);
				mesh.UnlockVertexBuffer();
				g.Close();
				g=null; */

			// We need to extract the material properties and texture names
			// from the MtrlBuffer
			int i;
			for(i = 0;i<materials.Length;i++) 
			{
    
				// Copy the material using the d3dx helper function
				MeshMaterials[i]=materials[i].Material3D;

				// Set the ambient color for the material (D3DX does not do this)
				MeshMaterials[i].Ambient = MeshMaterials[i].Diffuse;
     
				// Create the texture
				string strTexName = materials[i].TextureFilename;
				if ((strTexName!=null) && (strTexName != "")) 
				{
					MeshTextures[i]=TextureLoader.FromFile(d3dDevice, path+strTexName);
				} 
			} 
			
		}

		public void addlocation(float x, float y, float z, float rx, float ry, float rz) 
		{
			rx=(float)(rx*Math.PI/180);
			ry=(float)(ry*Math.PI/180);
			rz=(float)(rz*Math.PI/180);
			Matrix m=Matrix.RotationYawPitchRoll(rx,ry,rz);
			Matrix m2=Matrix.Translation(x,y,z);
			m.Multiply(m2);
			Positions.Add(m);
		}


		public void Render() 
		{
			if (drawing)
				return;

			drawing=true;

			for (int z=0; z<Positions.Count;z++)
			{
				d3dDevice.SetTransform(TransformType.World, (Matrix)Positions[z]);

				// Rendering of scene objects occur here.
				// real render starts

				// Meshes are divided into subsets, one for each material.
				// Render them in a loop

				for (int i = 0;i<MeshMaterials.Length;i++) 
				{
    
					// Set the material and texture for this subset
					d3dDevice.Material= MeshMaterials[i];
					d3dDevice.SetTexture(0,MeshTextures[i]);
        
					// Draw the mesh subset
					mesh.DrawSubset(i);
				}
			}
			drawing=false;
		}
	}
}
