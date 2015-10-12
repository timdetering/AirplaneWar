using System;
using System.IO;
using System.Threading;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

/* This landscape system is very simple (any serious gamedev would
 * kill me for this). In the future we might implement ROAM (or if anyone
 * wants to do it) but for now this is the way this works:
 * 
 * We create a grid of 9x9 divided in 81 .x files.
 * The file terrenoinfo.dat has information about the size of each
 * square and its center position.
 * 
 * When rendering we decide which square is going to be shown and ignore
 * all others.
 */

namespace AirplaneWar
{
	public struct MapMesh 
	{
		public Material [] MeshMaterials;
		public Texture [] MeshTextures;
		public Mesh mesh;
		public bool loading;
		public Vector3 min;
		public Vector3 max;
		public Vector3 center;
		public Mesh box;
		public Matrix mat;
		public Vector3 [] m_vecBoundsLocal;   // bounding box coordinates (in local coord space)
		public Vector3 [] m_vecBoundsWorld;   // bounding box coordinates (in world coord space)
		public Plane []   m_planeBoundsWorld; // bounding box planes (in world coord space)

	}

	/// <summary>
	/// Summary description for Box.
	/// </summary>
	public class Landscape
	{
		protected Device d3dDevice;
		Matrix matWorld=new Matrix();
		public Vector3 min;
		public Vector3 max;
		Texture maptexture;
		Material mapmaterial;
		bool drawing=false;
		bool simple=false;

		MapMesh [] map;

		public struct Vertex
		{
			public Vector3 p;
			public Vector3 n;
			public float tu, tv;
			public static readonly VertexFormats Format = VertexFormats.Position | VertexFormats.Normal | VertexFormats.Texture1;
		};

		public Landscape(Device d3device)
		{
			matWorld.Translate(0f,0f,0f);
			d3dDevice = d3device;
			this.simple=Form1.terrainFlat;
			map=new MapMesh[81];

			// Read data information from terrenoinfo.dat
			// so we can actually know which terrain will be shown
			// without loading the whole mesh into memory.

		/*	for (int p1=0;p1<81;p1++) 
			{
				LoadTerrainGrid lg2=new LoadTerrainGrid(d3dDevice,map,p1);
				lg2.LoadGrid();
			} */

			if (!simple) 
			{
				StreamReader sr=new StreamReader(@"media\terrain\terrenoinfo.dat");
				Matrix world=new Matrix();

				for (int p=0;p<81;p++) 
				{
					sr.ReadLine(); // read the grid number
					map[p].min.X=float.Parse(sr.ReadLine());
					map[p].min.Y=float.Parse(sr.ReadLine());
					map[p].min.Z=float.Parse(sr.ReadLine());
					map[p].max.X=float.Parse(sr.ReadLine());
					map[p].max.Y=float.Parse(sr.ReadLine());
					map[p].max.Z=float.Parse(sr.ReadLine());
					map[p].mesh=null;
					map[p].loading=false;
					map[p].mat=new Matrix();
					map[p].mat.Translate((map[p].min.X+map[p].max.X)/2f,(map[p].min.Y+map[p].max.Y)/2f,(map[p].min.Z+map[p].max.Z)/2f);

					map[p].center=new Vector3((map[p].min.X+map[p].max.X)/2f,(map[p].min.Y+map[p].max.Y)/2f,(map[p].min.Z+map[p].max.Z)/2f);

					map[p].m_vecBoundsLocal=new Vector3[8];

					map[p].m_vecBoundsLocal[0] = new Vector3( map[p].min.X, map[p].min.Y, map[p].min.Z ); // xyz
					map[p].m_vecBoundsLocal[1] = new Vector3( map[p].max.X, map[p].min.Y, map[p].min.Z ); // Xyz
					map[p].m_vecBoundsLocal[2] = new Vector3( map[p].min.X, map[p].max.Y, map[p].min.Z ); // xYz
					map[p].m_vecBoundsLocal[3] = new Vector3( map[p].max.X, map[p].max.Y, map[p].min.Z ); // XYz
					map[p].m_vecBoundsLocal[4] = new Vector3( map[p].min.X, map[p].min.Y, map[p].max.Z ); // xyZ
					map[p].m_vecBoundsLocal[5] = new Vector3( map[p].max.X, map[p].min.Y, map[p].max.Z ); // XyZ
					map[p].m_vecBoundsLocal[6] = new Vector3( map[p].min.X, map[p].max.Y, map[p].max.Z ); // xYZ
					map[p].m_vecBoundsLocal[7] = new Vector3( map[p].max.X, map[p].max.Y, map[p].max.Z ); // XYZ
 
					// transform local position to world coordinates
					map[p].m_vecBoundsWorld=new Vector3[8];
					for( int i = 0; i < 8; i++ )
						map[p].m_vecBoundsWorld[i]=map[p].m_vecBoundsLocal[i]; // Vector3.TransformCoordinate( map[p].m_vecBoundsLocal[i], map[p].mat );

					// Determine planes of the bounding box
					map[p].m_planeBoundsWorld=new Plane[6];
					map[p].m_planeBoundsWorld[0]=Plane.FromPoints( map[p].m_vecBoundsWorld[0], 
						map[p].m_vecBoundsWorld[1], map[p].m_vecBoundsWorld[2] ); // Near
					map[p].m_planeBoundsWorld[1]=Plane.FromPoints(map[p].m_vecBoundsWorld[6], 
						map[p].m_vecBoundsWorld[7], map[p].m_vecBoundsWorld[5] ); // Far
					map[p].m_planeBoundsWorld[2]=Plane.FromPoints(map[p].m_vecBoundsWorld[2], 
						map[p].m_vecBoundsWorld[6], map[p].m_vecBoundsWorld[4] ); // Left
					map[p].m_planeBoundsWorld[3]=Plane.FromPoints(map[p].m_vecBoundsWorld[7], 
						map[p].m_vecBoundsWorld[3], map[p].m_vecBoundsWorld[5] ); // Right
					map[p].m_planeBoundsWorld[4]=Plane.FromPoints(map[p].m_vecBoundsWorld[2], 
						map[p].m_vecBoundsWorld[3], map[p].m_vecBoundsWorld[6] ); // Top
					map[p].m_planeBoundsWorld[5]=Plane.FromPoints(map[p].m_vecBoundsWorld[1], 
						map[p].m_vecBoundsWorld[0], map[p].m_vecBoundsWorld[4] ); // Bottom

					map[p].box=Mesh.Box(d3dDevice,map[p].max.X-map[p].min.X,map[p].max.Y-map[p].min.Y,map[p].max.Z-map[p].min.Z);
				}
				sr.Close();

				LoadTerrainGrid lg=new LoadTerrainGrid(d3dDevice,map,32);
				lg.LoadGrid(32);
				lg.LoadGrid(41);
				lg.LoadGrid(21);
				lg.LoadGrid(31);
				lg.LoadGrid(33);
				lg.LoadGrid(34);
				lg.LoadGrid(40);
				lg.LoadGrid(42);
				lg.LoadGrid(43);
				lg.LoadGrid(39);
				lg.LoadGrid(20);
				lg.LoadGrid(22);
				lg.LoadGrid(23);
			} 
			else 
			{
				map[0].mesh=Mesh.FromFile(@"media\terrain\single.x",MeshFlags.SystemMemory, d3dDevice); // , out o, out materials);
			}

			if (Form1.smallTexture)
				maptexture=TextureLoader.FromFile(d3dDevice, @"media\terrain\ps_texture_simple.jpg");
			else
				maptexture=TextureLoader.FromFile(d3dDevice, @"media\terrain\ps_texture_4k.jpg");


			mapmaterial=new Material();
			mapmaterial.Diffuse=mapmaterial.Ambient=System.Drawing.Color.FromArgb(149,149,149);

		}

		private int getSquare(Vector3 pos) 
		{
			for (int i=0;i<81;i++) 
			{
				if ((pos.X >= map[i].min.X) && (pos.X <= map[i].max.X) && (pos.Z >= map[i].min.Z) && (pos.Z <= map[i].max.Z)) 
					return i;
			}
			return -1;
		}

		private void loadgrid(int x) 
		{
			// if already in memory, return
			if ((map[x].mesh!=null) || (map[x].loading))
				return;

			map[x].loading=true;

			LoadTerrainGrid lg=new LoadTerrainGrid(d3dDevice,map,x);

			Thread t=new Thread (new ThreadStart(lg.LoadGrid) );
			t.Start();
		}

		private void disposegrid(int x) 
		{
			// if loading or not loaded at all, return
			if ((map[x].mesh==null) || (map[x].loading))
				return;

			map[x].mesh.Dispose();
			map[x].mesh=null;
		}

		/* distance to hit the ground */
		public float Intersect(Vector3 pos, Vector3 dir) 
		{
			int square=0;

			if (!simple) 
			{
				square=getSquare(pos);

				if (square==-1)
					return pos.Y;
			
				if ((map[square].mesh==null) || (map[square].loading))
					return 0f;
			}

			IntersectInformation hit=new IntersectInformation();
			if (map[square].mesh.Intersect(pos,dir,ref hit)) 
				return hit.Dist;
			return 0f;
		}

		/* We try to get the height of the landscape so the airplane
		 * can land with pos.Y = landscape.Y
		 * For that we get the current height on that point compared
		 * to the land, and subtract that value from our pos.Y that way
		 * our pos.Y should be equal to landscape.Y
		 * */
		public float RealHeight(Vector3 pos) 
		{
			int square=0;
			
			if (!simple) 
			{
				square=getSquare(pos);
			
				if (square==-1)
					return pos.Y;

				if ((map[square].mesh==null) || (map[square].loading))
					return 0f;

			}
			IntersectInformation hit=new IntersectInformation();
			Vector3 dir=new Vector3(0f,-1f,0f);
			if (map[square].mesh.Intersect(pos,dir,ref hit)) 
				return pos.Y-hit.Dist;
			dir.Y=1f;
			if (map[square].mesh.Intersect(pos,dir,ref hit)) 
			{
				if (pos.Y>0f)
					return pos.Y-hit.Dist;
				else
					return pos.Y+hit.Dist;
			}

			// no intersection at all? return the object height
			return pos.Y;
		}

		public void SetTransform(Matrix mat) 
		{
			matWorld=mat;
		}  

		public void ApplyTransform() 
		{
			// The transform Matrix is used to position and orient the objects
			// you are drawing

			d3dDevice.SetTransform(TransformType.World, matWorld);
		}

		private float DistanceToLand(Vector3 myposition, int i) 
		{
//			map[p].center=new Vector3((map[p].min.X+map[p].max.X)/2f,(map[p].min.Y+map[p].max.Y)/2f,(map[p].min.Z+map[p].max.Z)/2f);

			Vector3 vector=Vector3.Subtract(myposition,map[i].center);
			double mindistance=Math.Sqrt(Vector3.Dot(vector,vector));
			// if too close or too far, don't even bother to calculate
			// more
			if ((mindistance < 25000) || (mindistance > 200000))
				return (float)mindistance;
			float height=(map[i].min.Y+map[i].max.Y)/2f;
			Vector3 actual=new Vector3(map[i].min.X,height,map[i].min.Z);

			vector=Vector3.Subtract(myposition,actual);
			double distance=Math.Sqrt(Vector3.Dot(vector,vector));
			if (distance < 25000)
				return (float)distance;
			mindistance=Math.Min(distance,mindistance);

			actual.Z=map[i].max.Z;
			vector=Vector3.Subtract(myposition,actual);
			distance=Math.Sqrt(Vector3.Dot(vector,vector));
			if (distance < 25000)
				return (float)distance;
			mindistance=Math.Min(distance,mindistance);


			actual.X=map[i].max.X;
			vector=Vector3.Subtract(myposition,actual);
			distance=Math.Sqrt(Vector3.Dot(vector,vector));
			if (distance < 25000)
				return (float)distance;
			mindistance=Math.Min(distance,mindistance);

			actual.Z=map[i].min.Z;
			vector=Vector3.Subtract(myposition,actual);
			distance=Math.Sqrt(Vector3.Dot(vector,vector));
			if (distance < 25000)
				return (float)distance;
			mindistance=Math.Min(distance,mindistance);
			return (float)mindistance;
		}

		private void RenderSquare(int x) 
		{
			if (!simple) 
			{
				// if the mesh is not loaded, do it
				if ((map[x].mesh==null) && (map[x].loading==false)) 
					loadgrid(x);

				if (map[x].loading)
					return;
			}

			ApplyTransform();

			// Set the material and texture for this subset
			d3dDevice.Material= mapmaterial;
			d3dDevice.SetTexture(0, maptexture);
			d3dDevice.SamplerState[0].MinFilter=TextureFilter.Linear;
			d3dDevice.SamplerState[0].MagFilter=TextureFilter.Linear;
    
			// Draw the mesh subset
			map[x].mesh.DrawSubset(0);
		}

		public void Render(CULLINFO m_cullinfo, Vector3 myposition) 
		{
			if (drawing)
				return;

			drawing=true;
			if (!simple) 
			{
				for (int i=0;i<81;i++) 
				{  
					float distance=DistanceToLand(myposition,i);

					if (distance < 70000f) 
					{
						// if close enough, load it into memory
						loadgrid(i);

						// if visible on camara, render it
						if (CullObject( m_cullinfo, 
							map[i].m_vecBoundsWorld, map[i].m_planeBoundsWorld ))
							RenderSquare(i);
					} 
					else if (distance > 125000f) 
					{
						// if it is very far away, remove it from memory
						// so we free some ram
						disposegrid(i);
					}
				}
			} 
			else 
			{
				RenderSquare(0);
			}
			drawing=false;
		}

		//-----------------------------------------------------------------------------
		// Name: CullObject()
		// Desc: Determine the cullstate for an object bounding box (OBB).  
		//       The algorithm is:
		//       1) If any OBB corner pt is inside the frustum, return CS_INSIDE
		//       2) Else if all OBB corner pts are outside a single frustum plane, 
		//          return CS_OUTSIDE
		//       3) Else if any frustum edge penetrates a face of the OBB, return 
		//          CS_INSIDE_SLOW
		//       4) Else if any OBB edge penetrates a face of the frustum, return
		//          CS_INSIDE_SLOW
		//       5) Else if any point in the frustum is outside any plane of the 
		//          OBB, return CS_OUTSIDE_SLOW
		//       6) Else return CS_INSIDE_SLOW
		//-----------------------------------------------------------------------------
		private bool CullObject( CULLINFO pCullInfo, Vector3[] pVecBounds, Plane[] pPlaneBounds )
		{
			byte[] bOutside={0,0,0,0,0,0,0,0};

			// Check boundary vertices in the box (8) against all 6 frustum planes, 
			// and store result (1 if outside) in a bitfield
			for( int iPoint = 0; iPoint < 8; iPoint++ )
			{
				int k=0;
				for( int iPlane = 0; iPlane < 6; iPlane++ )
				{
					if( (pCullInfo.planeFrustum[iPlane].A * pVecBounds[iPoint].X +
						pCullInfo.planeFrustum[iPlane].B * pVecBounds[iPoint].Y +
						pCullInfo.planeFrustum[iPlane].C * pVecBounds[iPoint].Z +
						pCullInfo.planeFrustum[iPlane].D) < 0.0f)
					{   // this vertex is outside this plane, shift 1
						bOutside[iPoint] |= (byte)(1 << iPlane);
						k++;
					}
				}
				// If any point is inside all 6 frustum planes, it is inside
				// the frustum, so the object must be rendered.
				if( bOutside[iPoint] == 0 )
					return true;
			}

			// If all points are outside any single frustum plane, the object is
			// outside the frustum
			if( (bOutside[0] &  bOutside[1] & bOutside[2] &  bOutside[3] &  
				bOutside[4] &  bOutside[5] &  bOutside[6] &  bOutside[7]) != 0 )
			{
				return false;
			}

			// Now see if any of the frustum edges penetrate any of the faces of
			// the bounding box
			Vector3 [][] edge = {
									new Vector3[2] {pCullInfo.vecFrustum[0], pCullInfo.vecFrustum[1]}, // front bottom
									new Vector3[2] {pCullInfo.vecFrustum[2], pCullInfo.vecFrustum[3]}, // front top
									new Vector3[2] {pCullInfo.vecFrustum[0], pCullInfo.vecFrustum[2]}, // front left
									new Vector3[2] {pCullInfo.vecFrustum[1], pCullInfo.vecFrustum[3]}, // front right
									new Vector3[2] {pCullInfo.vecFrustum[4], pCullInfo.vecFrustum[5]}, // back bottom
									new Vector3[2] {pCullInfo.vecFrustum[6], pCullInfo.vecFrustum[7]}, // back top
									new Vector3[2] {pCullInfo.vecFrustum[4], pCullInfo.vecFrustum[6]}, // back left
									new Vector3[2] {pCullInfo.vecFrustum[5], pCullInfo.vecFrustum[7]}, // back right
									new Vector3[2] {pCullInfo.vecFrustum[0], pCullInfo.vecFrustum[4]}, // left bottom
									new Vector3[2] {pCullInfo.vecFrustum[2], pCullInfo.vecFrustum[6]}, // left top
									new Vector3[2] {pCullInfo.vecFrustum[1], pCullInfo.vecFrustum[5]}, // right bottom
									new Vector3[2] {pCullInfo.vecFrustum[3], pCullInfo.vecFrustum[7]}, // right top
			};
			Vector3 [][] face = {
									new Vector3[4] {pVecBounds[0], pVecBounds[2], pVecBounds[3], pVecBounds[1]}, // front
									new Vector3[4] {pVecBounds[4], pVecBounds[5], pVecBounds[7], pVecBounds[6]}, // back
									new Vector3[4] {pVecBounds[0], pVecBounds[4], pVecBounds[6], pVecBounds[2]}, // left
									new Vector3[4] {pVecBounds[1], pVecBounds[3], pVecBounds[7], pVecBounds[5]}, // right
									new Vector3[4] {pVecBounds[2], pVecBounds[6], pVecBounds[7], pVecBounds[3]}, // top
									new Vector3[4] {pVecBounds[0], pVecBounds[4], pVecBounds[5], pVecBounds[1]}, // bottom
			};
			int iEdge;
			for(iEdge = 0; iEdge < 12; iEdge++ )
			{
				for(int iFace = 0; iFace < 6; iFace++ )
				{
					if( EdgeIntersectsFace( edge[iEdge], face[iFace], pPlaneBounds[iFace] ) )
					{
						return true;
					}
				}
			}

			// Now see if any of the bounding box edges penetrate any of the faces of
			// the frustum
			Vector3 [][] edge2 = {
									 new Vector3[2] {pVecBounds[0], pVecBounds[1]}, // front bottom
									 new Vector3[2] {pVecBounds[2], pVecBounds[3]}, // front top
									 new Vector3[2] {pVecBounds[0], pVecBounds[2]}, // front left
									 new Vector3[2] {pVecBounds[1], pVecBounds[3]}, // front right
									 new Vector3[2] {pVecBounds[4], pVecBounds[5]}, // back bottom
									 new Vector3[2] {pVecBounds[6], pVecBounds[7]}, // back top
									 new Vector3[2] {pVecBounds[4], pVecBounds[6]}, // back left
									 new Vector3[2] {pVecBounds[5], pVecBounds[7]}, // back right
									 new Vector3[2] {pVecBounds[0], pVecBounds[4]}, // left bottom
									 new Vector3[2] {pVecBounds[2], pVecBounds[6]}, // left top
									 new Vector3[2] {pVecBounds[1], pVecBounds[5]}, // right bottom
									 new Vector3[2] {pVecBounds[3], pVecBounds[7]}, // right top
			};
			Vector3 [][] face2 = {
									 new Vector3[4]{pCullInfo.vecFrustum[0], pCullInfo.vecFrustum[2], pCullInfo.vecFrustum[3], pCullInfo.vecFrustum[1]}, // front
									 new Vector3[4]{pCullInfo.vecFrustum[4], pCullInfo.vecFrustum[5], pCullInfo.vecFrustum[7], pCullInfo.vecFrustum[6]}, // back
									 new Vector3[4]{pCullInfo.vecFrustum[0], pCullInfo.vecFrustum[4], pCullInfo.vecFrustum[6], pCullInfo.vecFrustum[2]}, // left
									 new Vector3[4]{pCullInfo.vecFrustum[1], pCullInfo.vecFrustum[3], pCullInfo.vecFrustum[7], pCullInfo.vecFrustum[5]}, // right
									 new Vector3[4]{pCullInfo.vecFrustum[2], pCullInfo.vecFrustum[6], pCullInfo.vecFrustum[7], pCullInfo.vecFrustum[3]}, // top
									 new Vector3[4]{pCullInfo.vecFrustum[0], pCullInfo.vecFrustum[4], pCullInfo.vecFrustum[5], pCullInfo.vecFrustum[1]}, // bottom
			};
			for( iEdge = 0; iEdge < 12; iEdge++ )
			{
				for(int iFace = 0; iFace < 6; iFace++ )
				{
					if( EdgeIntersectsFace( edge2[iEdge], face2[iFace], pCullInfo.planeFrustum[iFace] ) )
					{
						return true;
					}
				}
			}

			// Now see if frustum is contained in bounding box
			// If any frustum corner point is outside any plane of the bounding box,
			// the frustum is not contained in the bounding box, so the object
			// is outside the frustum
			for(int iPlane = 0; iPlane < 6; iPlane++ )
			{
				if( pPlaneBounds[iPlane].A * pCullInfo.vecFrustum[0].X +
					pPlaneBounds[iPlane].B * pCullInfo.vecFrustum[0].Y +
					pPlaneBounds[iPlane].C * pCullInfo.vecFrustum[0].Z +
					pPlaneBounds[iPlane].D  < 0.0f )
				{
					return false;
				}
			}

			// Bounding box must contain the frustum, so render the object
			return true;
		}

		
		//-----------------------------------------------------------------------------
		// Name: EdgeIntersectsFace()
		// Desc: Determine if the edge bounded by the two vectors in pEdges intersects
		//       the quadrilateral described by the four vectors in pFacePoints.  
		//       Note: pPlane could be derived from pFacePoints using 
		//       D3DXPlaneFromPoints, but it is precomputed in advance for greater
		//       speed.
		//-----------------------------------------------------------------------------
		private bool EdgeIntersectsFace( Vector3 [] pEdges, Vector3 [] pFacePoints, 
			Plane pPlane )
		{
			// If both edge points are on the same side of the plane, the edge does
			// not intersect the face
			float fDist1;
			float fDist2;
			fDist1 = pPlane.A * pEdges[0].X + pPlane.B * pEdges[0].Y +
				pPlane.C * pEdges[0].Z + pPlane.D;
			fDist2 = pPlane.A * pEdges[1].X + pPlane.B * pEdges[1].Y +
				pPlane.C * pEdges[1].Z + pPlane.D;
			if( fDist1 > 0 && fDist2 > 0 ||
				fDist1 < 0 && fDist2 < 0 )
			{
				return false;
			}

			// Find point of intersection between edge and face plane (if they're
			// parallel, edge does not intersect face and D3DXPlaneintersectLine 
			// returns NULL)
			Vector3 ptintersection=Plane.IntersectLine(pPlane, pEdges[0], pEdges[1] );
			if(ptintersection == Vector3.Empty )
				return false;

			// Project onto a 2D plane to make the pt-in-poly test easier
			float fAbsA = (pPlane.A > 0 ? pPlane.A : -pPlane.A);
			float fAbsB = (pPlane.B > 0 ? pPlane.B : -pPlane.B);
			float fAbsC = (pPlane.C > 0 ? pPlane.C : -pPlane.C);
			Vector2 [] facePoints=new Vector2[4];
			Vector2 point;
			if( fAbsA > fAbsB && fAbsA > fAbsC )
			{
				// Plane is mainly pointing along X axis, so use Y and Z
				for( int i = 0; i < 4; i++)
				{
					facePoints[i].X = pFacePoints[i].Y;
					facePoints[i].Y = pFacePoints[i].Z;
				}
				point.X = ptintersection.Y;
				point.Y = ptintersection.Z;
			}
			else if( fAbsB > fAbsA && fAbsB > fAbsC )
			{
				// Plane is mainly pointing along Y axis, so use X and Z
				for( int i = 0; i < 4; i++)
				{
					facePoints[i].X = pFacePoints[i].X;
					facePoints[i].Y = pFacePoints[i].Z;
				}
				point.X = ptintersection.X;
				point.Y = ptintersection.Z;
			}
			else
			{
				// Plane is mainly pointing along Z axis, so use X and Y
				for( int i = 0; i < 4; i++)
				{
					facePoints[i].X = pFacePoints[i].X;
					facePoints[i].Y = pFacePoints[i].Y;
				}
				point.X = ptintersection.X;
				point.Y = ptintersection.Y;
			}

			// If point is on the outside of any of the face edges, it is
			// outside the face.  
			// We can do this by taking the determinant of the following matrix:
			// | x0 y0 1 |
			// | x1 y1 1 |
			// | x2 y2 1 |
			// where (x0,y0) and (x1,y1) are points on the face edge and (x2,y2) 
			// is our test point.  If this value is positive, the test point is
			// "to the left" of the line.  To determine whether a point needs to
			// be "to the right" or "to the left" of the four lines to qualify as
			// inside the face, we need to see if the faces are specified in 
			// clockwise or counter-clockwise order (it could be either, since the
			// edge could be penetrating from either side).  To determine this, we
			// do the same test to see if the third point is "to the right" or 
			// "to the left" of the line formed by the first two points.
			// See http://forum.swarthmore.edu/dr.math/problems/scott5.31.96.html
			float x0, x1, x2, y0, y1, y2;
			x0 = facePoints[0].X;
			y0 = facePoints[0].Y;
			x1 = facePoints[1].X;
			y1 = facePoints[1].Y;
			x2 = facePoints[2].X;
			y2 = facePoints[2].Y;
			bool bClockwise = false;
			if( x1*y2 - y1*x2 - x0*y2 + y0*x2 + x0*y1 - y0*x1 < 0 )
				bClockwise = true;
			x2 = point.X;
			y2 = point.Y;
			for( int i = 0; i < 4; i++ )
			{
				x0 = facePoints[i].X;
				y0 = facePoints[i].Y;
				if( i < 3 )
				{
					x1 = facePoints[i+1].X;
					y1 = facePoints[i+1].Y;
				}
				else
				{
					x1 = facePoints[0].X;
					y1 = facePoints[0].Y;
				}
				if( ( x1*y2 - y1*x2 - x0*y2 + y0*x2 + x0*y1 - y0*x1 > 0 ) == bClockwise )
					return false;
			}

			// If we get here, the point is inside all four face edges, 
			// so it's inside the face.
			return true;
		}

	}
}
