using System;
using System.Collections;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace AirplaneWar
{
    /// <summary>
    /// Summary description for airplane.
    /// </summary>
    public class airplane : MyMesh
    {
        public static Hashtable Positions=new Hashtable();
        bool drawing=false;
        /* public Vector3 min;
        public Vector3 max; */
        /* tailTire used to know when the tail hits the ground */
        Vector3 tailTire=new Vector3(0f,-0.5f,-3.1f);
        Vector3 leftTire=new Vector3(-0.6f,-1.1f,2.1f);
        Vector3 rightTire=new Vector3(0.6f,-1.1f,2.1f);
        Vector3 front=new Vector3(0f,0.4f,3.7f);
        /* double wings, check if they hit the ground */
        Vector3 leftBWing=new Vector3(-4.5f,-0.3f,2.1f);
        Vector3 rightBWing=new Vector3(4.5f,-0.3f,2.1f);
        Vector3 leftTWing=new Vector3(-4.7f,1.2f,2.1f);
        Vector3 rightTWing=new Vector3(4.7f,1.2f,2.1f);

        public airplane(Device d3device):base(d3device)
        {
            ExtendedMaterial[] materials=null;

            GraphicsStream o=null;
            mesh = Mesh.FromFile(@"media\airplane.x",MeshFlags.SystemMemory, d3dDevice, out o, out materials);

            //	Before getting this information from the material buffer, the material and texture arrays must be resized to fit all the materials and textures for this mesh.
            MeshMaterials=new Material[materials.Length]; // Mesh Material data
            MeshTextures=new Texture[materials.Length];  // ' Mesh Textures

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
                    MeshTextures[i]=TextureLoader.FromFile(d3dDevice, @"media\"+strTexName);
                } 
            } 

        }

        public void Translate(int id, float x, float y, float z, float x2, float y2, float z2, float w) 
        {
            if (drawing)
                return;
            Matrix mat=new Matrix();
            Quaternion q=new Quaternion(x2,y2,z2,w);

            mat.Translate(x,y,z);
            mat=Matrix.Multiply(Matrix.RotationQuaternion(q),mat);
            Positions[id]=mat;
        }

        

        public void Render(Matrix world) 
        {
            if (drawing)
                return;

            drawing=true;

                d3dDevice.SetTransform(TransformType.World, world);

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
            drawing=false;
        }


        public void Render() 
        {
            drawing=true;
            lock( Positions.SyncRoot ) 
            {
                foreach (DictionaryEntry en in Positions) 
                {
                    d3dDevice.SetTransform(TransformType.World, (Matrix)en.Value);
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
            }
            drawing=false;
        }
    }
}
