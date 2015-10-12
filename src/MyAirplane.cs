using System;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using DInput=Microsoft.DirectX.DirectInput;
using System.Drawing;

namespace AirplaneWar
{
	public struct MyFlatSurface 
	{
		public float height;
		public float width;
		public Vector3 normal;

		public MyFlatSurface(float h, float w, Vector3 n) 
		{
			height=h;
			width=w;
			normal=n;
		}
	}

	/// <summary>
	/// Summary description for MyAirplane.
	/// </summary>
	public class MyAirplane
	{
		Microsoft.DirectX.Direct3D.Font d3dxfont;
		MyFlatSurface wingsSpan;
		MyFlatSurface frontSpan;
		MyFlatSurface tailSpan;
		MyFlatSurface lateralSpan;
		MyFlatSurface bottomSpan;
		MyFlatSurface tailLateralSpan;

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

		Vector3 force=new Vector3(0f,0f,0f);
		public Vector3 prepos;

		private int view=1;

		private bool gearbrake=false;
		Landscape landscape;
		airplane myAirplane;
		InputDevice idevice=null;
		ShootingClass gunshots=null;
		float weight=500f; // 500 kgs
		float rotor=0f;
		float mass=0f;
		double tailangle=0d;
		float wingangleattack=0f;
		float maxspeed=0f;

		float tailPitchRotation=0f;
		float tailRudderRotation=0f;
		float m_fPitchVelocity=0f;
		float m_fRollVelocity=0f;
		float ya=0f;

		public float height=0f;
		public float realheight=0f;

		float liftSpeed=0f;

	//	DInput.Device didev;
		Device d3dDevice;

		Matrix oldView;
		Matrix m_matView;
		public Quaternion rotation=new Quaternion();

		float speed=0;
		float acceleration=0;
		timer measure=new timer();
		float time=0.05f;
		float gravity=9.81f;
		float supertime=0f;

		public Vector3 m_vVelocity, m_vPosition;

		Vector3 gravityVector;
		Vector3 liftVector;
		Vector3 trustVector;
		Vector3 dragVector;
		Vector3 speedVector;

		Matrix m_matOrientation;

		public MyAirplane(Device d3d, System.Drawing.Font font, Microsoft.DirectX.DirectSound.Device dsound, Landscape l)
		{
			measure.Start();
			d3dxfont=new Microsoft.DirectX.Direct3D.Font(d3d,font);
			myAirplane=new airplane(d3d);
			landscape=l;
			idevice=new InputDevice();
			d3dDevice=d3d;
			gunshots=new ShootingClass(d3d, dsound);
		//	didev=di;
			rotation = new Quaternion();
			rotation=Quaternion.Identity;
			rotation.RotateYawPitchRoll((float)Math.PI,0f,0f);

			// the airplane starts with 20º angle (landed)
		//	rotation.RotateYawPitchRoll(0, -0.34906585f, 0);

			m_matOrientation=new Matrix();
			m_matOrientation.Translate(0, 0, 0);
    
			m_vPosition = new Vector3(1, 1, -10);
			prepos=m_vPosition;
			m_vVelocity=new Vector3(0, 0, 0);
			gravityVector=new Vector3(0, 0, 0);
			liftVector=new Vector3(0, 0, 0);
			trustVector=new Vector3(0, 0, 0);
			dragVector=new Vector3(0, 0, 0);
			speedVector=new Vector3(0,0,0);
			mass=weight/gravity;

			m_matView=new Matrix();
			oldView=new Matrix();

		}

		protected void SetupMatrices() 
		{
		//	Matrix matWorld=new Matrix();
			// The transform Matrix is used to position and orient the objects
			// you are drawing
			// For our world matrix, we will just rotate the object about the y axis.
		//	Vector3 w=new Vector3(1, 1, 1);

			//	matWorld.RotateAxis(w, Timer / 2);
			//	aplane.SetTransform(matWorld);
			//	b.SetTransform(matWorld);
			
			copymatrix(ref m_matOrientation,ref oldView);

			CamaraTransform();

   
		}

		protected void copymatrix(ref Matrix mat, ref Matrix old) 
		{
			old.M11=mat.M11;
			old.M12=mat.M12;
			old.M13=mat.M13;
			old.M14=mat.M14;
			old.M21=mat.M21;
			old.M22=mat.M22;
			old.M23=mat.M23;
			old.M24=mat.M24;
			old.M31=mat.M31;
			old.M32=mat.M32;
			old.M33=mat.M33;
			old.M34=mat.M34;
			old.M41=mat.M41;
			old.M42=mat.M42;
			old.M43=mat.M43;
			old.M44=mat.M44;
		}
 
		private void checkLanded() 
		{

			// check if any tire is touching the ground

			/* translate the tail, left and right tire position
			 * to the current world position */
			Vector3 checkTail=Vector3.TransformNormal(tailTire,m_matOrientation);
			checkTail.Add(m_vPosition);

			Vector3 checkLeftTire=Vector3.TransformNormal(leftTire,m_matOrientation);
			checkLeftTire.Add(m_vPosition);

			Vector3 checkRightTire=Vector3.TransformNormal(rightTire,m_matOrientation);
			checkRightTire.Add(m_vPosition);

			// check if any tire is underground
			Vector3 upVector=new Vector3(0f,1f,0f);
			float tail=landscape.Intersect(checkTail,upVector);
			float left=landscape.Intersect(checkLeftTire,upVector);
			float right=landscape.Intersect(checkRightTire,upVector);



			/*	Vector3 tailTire=new Vector3(0f,-0.5f,-3.1f);
				Vector3 leftTire=new Vector3(-0.6f,-1.1f,2.1f);
				Vector3 rightTire=new Vector3(0.6f,-1.1f,2.1f); */



			// Thanks to #gamedev and gamedev.net for helping me with this
			// through direct help or through articles and resources.
			// One or more of the tires are underground
			if ((tail>0f) || (left>0f) || (right>0f)) 
			{
				Vector3 normal1=Vector3.Cross(Vector3.Subtract(checkTail,checkLeftTire),Vector3.Subtract(checkTail,checkRightTire));
				
				float y1=(checkTail.Y + checkLeftTire.Y + checkRightTire.Y) / 3f;
				
				checkTail.Y=checkTail.Y+tail;
				checkLeftTire.Y=checkLeftTire.Y+left;
				checkRightTire.Y=checkRightTire.Y+right;
				
				float y2=(checkTail.Y + checkLeftTire.Y + checkRightTire.Y) / 3f;

				Vector3 normal2=Vector3.Cross(Vector3.Subtract(checkTail,checkLeftTire),Vector3.Subtract(checkTail,checkRightTire));

				normal1.Normalize();
				normal2.Normalize();

				Vector3 axisRotation=Vector3.Cross(normal1,normal2);
				axisRotation.Normalize();
				float angle=(float)Math.Acos(Math.Min(Math.Max(-1.0f, Vector3.Dot(normal1,normal2)), 1.0f));
				
				//	float moveup=Math.Max(tail,Math.Max(left,right));


				rotation.Multiply(Quaternion.RotationAxis(axisRotation,angle));
				//	matTemp=Matrix.RotationQuaternion(rotation);
				//	matTemp.Multiply(Matrix.RotationAxis(axisRotation,angle));
				// matTemp=Matrix.RotationAxis(axisRotation,angle);
				m_vPosition.Y=m_vPosition.Y+(y2-y1);
			
		
				// friction should go here i guess

				// velocity and acceleraton on Y reset to 0
				m_vVelocity.Y=0f;
				force.Y=0f;


			} 
			// we have all the forces added up, now we find the acceleration
			// by dividing by the mass, remember: Force = Acceleration * Mass
			Vector3 accelerationVector=Vector3.Scale(force,1.0f/mass);

			// find the velocity (speed and direction)
			// formula: v = v0 + at
			m_vVelocity.X=m_vVelocity.X+accelerationVector.X*(time);
			m_vVelocity.Y=m_vVelocity.Y+accelerationVector.Y*(time);
			m_vVelocity.Z=m_vVelocity.Z+accelerationVector.Z*(time);
		}

		protected void CamaraTransform() 
		{
			// Update the position vector
			Vector3 vT=new Vector3(0,0,0), vTemp=new Vector3();
			vT.Add(m_vVelocity);
			prepos=m_vPosition;
			m_vPosition.Add(vT);
    

			Quaternion qR=new Quaternion();
			float det=0;

			Matrix matTrans=new Matrix(), matTemp=new Matrix();
			qR.RotateYawPitchRoll(ya, m_fPitchVelocity, m_fRollVelocity);
			rotation=Quaternion.Multiply(qR,rotation);

			checkLanded();

			//	m_vPosition.y=m_vPosition.y+gravityY;
			
			matTemp=Matrix.RotationQuaternion(rotation);
			matTrans.Translate(m_vPosition.X, m_vPosition.Y, m_vPosition.Z);

			m_matOrientation=Matrix.Multiply(matTemp, matTrans);

			/* if (newPos.Length() > 0f)
				m_vPosition=newPos; */

			speed=m_vVelocity.Length();

			/* apply the new orientation to all the surfaces 
			 * this should be improved so we don't have to create
			 * new objects all the time */

			Vector3 up=new Vector3(0,1,0);
			Vector3 left=new Vector3(1,0,0);
			wingsSpan=new MyFlatSurface(6f,6f,up);
			Matrix matTemp2=new Matrix();
			// wings have a 4º on airplane's body
			matTemp2=Matrix.RotationAxis(left, -0.069f);
			wingsSpan.normal.TransformNormal(matTemp2);

			tailSpan=new MyFlatSurface(1f,1f,up);
			// rotate the tail to achieve pitch
			matTemp2=Matrix.RotationAxis(left, this.tailPitchRotation);
			tailSpan.normal.TransformNormal(matTemp2);

			tailLateralSpan=new MyFlatSurface(1f,1f,new Vector3(1,0,0));
			matTemp2=Matrix.RotationAxis(new Vector3(0,1,0),this.tailRudderRotation);
			tailLateralSpan.normal.TransformNormal(matTemp2);

			bottomSpan=new MyFlatSurface(4f,4f,up);
			frontSpan=new MyFlatSurface(4f,4f,new Vector3(0,0,1));
			lateralSpan=new MyFlatSurface(4f,4f,new Vector3(1,0,0));


			wingsSpan.normal.TransformNormal(m_matOrientation);
			wingsSpan.normal.Normalize();
			frontSpan.normal.TransformNormal(m_matOrientation);
			frontSpan.normal.Normalize();
			bottomSpan.normal.TransformNormal(m_matOrientation);
			bottomSpan.normal.Normalize();
			tailSpan.normal.TransformNormal(m_matOrientation);
			tailSpan.normal.Normalize();
			lateralSpan.normal.TransformNormal(m_matOrientation);
			lateralSpan.normal.Normalize();
			tailLateralSpan.normal.TransformNormal(m_matOrientation);
			tailLateralSpan.normal.Normalize();

			if (view==2) 
			{
				Vector3 pos=Vector3.TransformNormal(new Vector3(0f,1f,-10f),m_matOrientation);
				pos.Add(m_vPosition);
				// Vector3 pos=new Vector3(m_vPosition.X, m_vPosition.Y, m_vPosition.Z-50);
						
				m_matView=Matrix.LookAtLH(pos,m_vPosition,new Vector3(0,1,0));
				this.myAirplane.Render(m_matOrientation);
			} 
			else 
			{			
				m_matView=Matrix.Invert(ref det,m_matOrientation);
			}

    

			d3dDevice.SetTransform(TransformType.View, m_matView);
		}

		private float GetContactArea(MyFlatSurface plane) 
		{
			Vector3 airdirection=Vector3.Scale(m_vVelocity,-1.0f);
			airdirection.Normalize();
			plane.normal.Normalize();

			return Physics.GetContactArea(plane,airdirection);
		}

		private float GetAirResistance(float dragcoefficient, float airdensity, float speed, MyFlatSurface area) 
		{
			Vector3 airdirection=Vector3.Scale(m_vVelocity,-1.0f);
			airdirection.Normalize();
			area.normal.Normalize();
			return dragcoefficient*airdensity*speed*speed*Physics.GetContactArea(area,airdirection)/2;
		}

		private float calculateLift(float airdensity) 
		{
			if (m_vVelocity.Length()==0f)
				return 0f;

			if (m_vVelocity.Length()>3f)
				airdensity=(airdensity/1)+1-(1);

			Vector3 airdirection=Vector3.Scale(m_vVelocity,-1.0f);
			airdirection.Normalize();

			float cosangle=Math.Min(Math.Max(-1.0f, Vector3.Dot(airdirection, wingsSpan.normal)), 1.0f);

			float angleattack=(float)Math.Acos(cosangle);
			angleattack=(float)(angleattack-(Math.PI/2));

			float wingsurface=GetContactArea(wingsSpan);


			float at14=1.2f; // lift coefficient at 14º
			float liftcoefficient=0;

			float angle=(float)Math.Abs(angleattack);

			if (angle < 0.2443460953f) // 14º 
			{
				liftcoefficient=(angle*at14)/0.2443460953f;  // for wings
			} 
			else if (angle<=0.2792526803f)  // 16º
			{
				liftcoefficient=at14;
			}
			else  // turn around the lift coefficient table (it loses efficiency)
			{
				liftcoefficient=((angle-((angle-0.2443460953f)*2))*at14)/0.2443460953f;
			}

			if (liftcoefficient<0f)
				liftcoefficient=0f;

			wingangleattack=angleattack;

			float lift=((liftcoefficient*airdensity*speed*speed*wingsurface/2));

			return lift;
		}

		private float calculateTailRudder(float airdensity) 
		{
			if ((m_vVelocity.Length()==0f)) // || (speed>16.7f))
				return 0f;

			Vector3 airdirection=Vector3.Scale(m_vVelocity,-1.0f);
			airdirection.Normalize();
			float cosangle=Math.Min(Math.Max(-1.0f, Vector3.Dot(airdirection, this.tailLateralSpan.normal)), 1.0f);
			
			float angleattack=(float)Math.Acos(cosangle);

			// lift is the force pushing up
			float lift=((0.2f*airdensity*speed*speed*GetContactArea(tailLateralSpan)/2));

			// From that lift, calculate the tail rotation

			/* max height of the triangle formed from the center
			 * of the airplane to the tail (we approximate the center
			 * to 5 meters)
			 * traduction: maximum distance that can be pushed up
			 */
			float maxLift=(float)Math.Tan(angleattack)*5;

			float weight=50; // 50 kgs the tail weights (force)
			float mass=weight/gravity;

			//		Vector3 liftV=new Vector3(0,lift,0);
			//		liftV.TransformNormal(m_matOrientation);

			/* From force to acceleration a = f/m
			 * and then to distance = a * time
			 * */
			lift=(lift/mass)*time;
			if (Math.Abs(lift)>Math.Abs(maxLift)) 
			{
				if (lift>0)
					return -angleattack;
				return angleattack;
			}

			lift=(float)Math.Atan(lift/5);

			// return rotation
			return -lift;

		}

		private float calculateTailLift(float airdensity) 
		{
			if ((m_vVelocity.Length()==0f)) // || (speed>16.7f))
				return 0f;

			Vector3 airdirection=Vector3.Scale(m_vVelocity,-1.0f);
			airdirection.Normalize();
			float cosangle=Math.Min(Math.Max(-1.0f, Vector3.Dot(airdirection, tailSpan.normal)), 1.0f);
			
			float angleattack=(float)Math.Acos(cosangle);

			/* float wingsurface=GetContactArea(tailSpan);

		/*	float at14=1.2f; // lift coefficient at 14º
			float liftcoefficient=0;
			angleattack=(float)(angleattack-(Math.PI/2));
			float angle=(float)Math.Abs(angleattack);

			// tailangle is the degree version of the (radians) angleattack
			tailangle=angleattack*180/Math.PI; // attack*(180/Math.PI);
			
			if (Math.Abs(tailangle)<0.2f)
				return 0f;

			if (angle < 0.2443460953f) // 14º 
			{
				liftcoefficient=(angle*at14)/0.2443460953f;  // for wings
			} 
			else if (angle<=0.2792526803f)  // 16º
			{
				liftcoefficient=at14;
			}
			else  // turn around the lift coefficient table (it loses efficiency)
			{
				liftcoefficient=((angle-((angle-0.2443460953f)*2))*at14)/0.2443460953f;
			}

			if (liftcoefficient<0f)
				liftcoefficient=0f;
				
		*/

			// lift is the force pushing up
			float lift=((0.2f*airdensity*speed*speed*GetContactArea(tailSpan)/2));

			// From that lift, calculate the tail rotation

			/* max height of the triangle formed from the center
			 * of the airplane to the tail (we approximate the center
			 * to 5 meters)
			 * traduction: maximum distance that can be pushed up
			 */
			float maxLift=(float)Math.Tan(angleattack)*5;

			float weight=50; // 50 kgs the tail weights (force)
			float mass=weight/gravity;

	//		Vector3 liftV=new Vector3(0,lift,0);
	//		liftV.TransformNormal(m_matOrientation);

			/* From force to acceleration a = f/m
			 * and then to distance = a * time
			 * */
			lift=(lift/mass)*time;
			if (Math.Abs(lift)>Math.Abs(maxLift)) 
			{
				if (lift>0)
					lift=angleattack;
				else
					lift=-angleattack;
				return lift;
			}

			lift=(float)Math.Atan(lift/5);

			// return rotation
			return lift;
		}

		private void doPhysics() 
		{
			if (Form1.focus) 
			{
				idevice.CheckInput();

				bool pitch=false, rudder=false;
				foreach( DeviceState state in idevice.DeviceStates )
				{ 
					for( int i=0; i < state.InputState.Length; i++ )
					{
						if (state.IsMapped[i]) 
						{
							switch (i) 
							{
								case (int)GameActions.Rudder:
									if (state.InputState[i] != 0) 
									{
										tailRudderRotation = -state.InputState[i]/191f;
										// ya=state.InputState[i]/2000f;
										rudder=true;
									}
									break; 
								case (int)GameActions.Bank:
									if (state.InputState[i] != 0) 
									{
										if (height<=1f)
											tailRudderRotation = -state.InputState[i]/191f;
										//	ya=state.InputState[i]/2000f;
										else
											m_fRollVelocity=-state.InputState[i]/2000f;
										rudder=true;
									}
									break; 
								case (int)GameActions.Pitch:
									if (state.InputState[i] != 0) 
									{
										tailPitchRotation = state.InputState[i]/191f;
										// m_fPitchVelocity=-state.InputState[i]/2000f;
										pitch=true;
									}
									break;
								case (int)GameActions.Shoot:
									if (state.InputState[i] != 0) 
									{
										Vector3 shot=new Vector3(0f,0f,30f);
										shot.TransformNormal(m_matOrientation);
										shot.Add(m_vVelocity);
										gunshots.NewShot(m_vPosition,shot);
									}
									break;
								case (int)GameActions.Quit:
									if (state.InputState[i] != 0)
										System.Windows.Forms.Application.Exit();
									break;
								case (int)GameActions.FirstPersonView:
									if (state.InputState[i] != 0)
										this.view=1;
									break;
								case (int)GameActions.OutsideView:
									if (state.InputState[i] != 0)
										this.view=2;
									break;
								case (int)GameActions.T100:
									if (state.InputState[i] != 0)
										maxspeed=90f;
									break;
								case (int)GameActions.T88:
									if (state.InputState[i] != 0)
										maxspeed=77f;
									break;
								case (int)GameActions.T77:
									if (state.InputState[i] != 0)
										maxspeed=65f;
									break;
								case (int)GameActions.T66:
									if (state.InputState[i] != 0)
										maxspeed=54f;
									break;
								case (int)GameActions.T55:
									if (state.InputState[i] != 0)
										maxspeed=42f;
									break;
								case (int)GameActions.T44:
									if (state.InputState[i] != 0)
										maxspeed=30f;
									break;
								case (int)GameActions.T33:
									if (state.InputState[i] != 0)
										maxspeed=17f;
									break;
								case (int)GameActions.T22:
									if (state.InputState[i] != 0)
										maxspeed=11f;
									break;
								case (int)GameActions.T10:
									if (state.InputState[i] != 0)
										maxspeed=7f;
									break;
								case (int)GameActions.Throttle:
									if (state.InputState[i] != 0)
										maxspeed=(float)state.InputState[i];
									break;
								case (int)GameActions.Stop:
									if (state.InputState[i] != 0)
										maxspeed=0f;
									break;
								case (int)GameActions.PitchUp:
									if (state.InputState[i] != 0) 
									{
										tailPitchRotation=tailPitchRotation-0.05f;
										// m_fPitchVelocity=m_fPitchVelocity+0.02f;
										pitch=true;
									}
									break;
								case (int)GameActions.PitchDown:
									if (state.InputState[i] != 0) 
									{
										tailPitchRotation=tailPitchRotation+0.05f;
										// m_fPitchVelocity=m_fPitchVelocity-0.02f;
										pitch=true;
									}
									break;
								case (int)GameActions.RudderLeft:
									if (state.InputState[i] != 0) 
									{
										if (height<=1f)
											tailRudderRotation=tailRudderRotation-0.01f;
											// ya=ya-0.01f;
										else
											m_fRollVelocity=m_fRollVelocity+0.01f;
										rudder=true;
									}
									break;
								case (int)GameActions.RudderRight:
									if (state.InputState[i] != 0) 
									{
										if (height<=1f)
											tailRudderRotation=tailRudderRotation+0.01f;
											// ya=ya+0.01f;
										else
											m_fRollVelocity=m_fRollVelocity-0.01f;
										rudder=true;
									}
									break;
								case (int)GameActions.Brake:
									if (state.InputState[i] != 0) 
									{
										if (height<=1f)
											gearbrake= !(gearbrake);
									}
									break;
								default:
									break;
							}
						}
					}
					if (!pitch) 
					{
						tailPitchRotation=0f;
						// m_fPitchVelocity=0f;
					}

					if (!rudder)
					{
						tailRudderRotation=0f;
						// ya=0;
						m_fRollVelocity=0;
					}	
				}
			}
			else 
			{
				tailRudderRotation=0f;
				// ya=0;
				m_fRollVelocity=0;
			} 

			// calculate acceleration

			if (rotor>maxspeed) 
			{ 	
				acceleration=-9.88888f;
			}
			else if (rotor<maxspeed) 
			{
				acceleration=9.88888f;
			} 
			else 
			{
				acceleration=0;
			}

			// engine limits
			if (rotor<0f) 
			{
				acceleration=0;
				rotor=0;
			} 

			float airdensity=1;
			airdensity -= height/10000f;
			if (airdensity<0)
				airdensity=0;

			// get the vertical speed (lift)

			liftVector.Y=calculateLift(airdensity);
			float tailLift=this.calculateTailLift(airdensity);
		//	tailRudderRotation=0f;
			float tailRudder=this.calculateTailRudder(airdensity);

			m_fPitchVelocity=tailLift;
			// if on the ground, make turning easy
			if ((height<1f) && (speed < 5f) && (speed > 0f))
				tailRudder=tailRudder*(200/speed);
			ya=tailRudder;

			float groundfriction=1f;
			if (gearbrake)
				groundfriction=10f;
			float curvefriction=1f;

			if (height<=5f) 
			{ 
				// next to ground there is a bigger sustentation force
				// simulated by a bigger air density
				airdensity=2;
				// at ground it takes a while to gain speed and stop faster
				if (height<=1f) 
				{
					groundfriction+=0.5f;
					curvefriction=10f;
				}
			}

			// apply drag (frontal drag produced by air)
			float dragcoefficient=0.05f;  // for frontal surface
			
			float dragZ=((groundfriction*GetAirResistance(dragcoefficient,airdensity,speed,frontSpan)));
			dragVector.Z=dragZ;



			// apply drag (lateral drag produced by air)
			dragcoefficient=0.2f;  // for frontal surface

			dragZ=(((groundfriction*groundfriction*curvefriction)*GetAirResistance(dragcoefficient,airdensity,speed,lateralSpan)));

			dragVector.X=dragZ;

			// apply drag produced by the bottom surface against air
			dragcoefficient=0.2f;  // for frontal surface

			dragZ=GetAirResistance(dragcoefficient,airdensity,speed,bottomSpan);
			dragVector.Y=dragZ;

			// apply engine trust (not sure about this formula)
			// F = .5 * r * A * [Ve ^2 - V0 ^2] 
			// where Ve is speed of exit and V0 speed of entrance
			// the problem here is that we know V0 which is the speed
			// of the airplane but I don't know Ve (speed of wind
			// after going through the blades) I couldn't find the formula
			// so I try to approximate it the best I can (rotor speed)
			float fansurface=3f;
			float trustcoefficient=0.05f;  // for airplanes
			float trustZ=((trustcoefficient*airdensity*fansurface*((rotor*rotor)-(speed*speed))/2));
			trustVector.Z=trustZ;

			rotor = rotor + acceleration*time;


			Vector3 accelerationVector=new Vector3(0,0,0);

			// we do all calculus on the local coordenates
			accelerationVector=Vector3.Add(trustVector, liftVector);
			accelerationVector.Add(dragVector);

			accelerationVector.TransformNormal(m_matOrientation);

			// gravity
			gravityVector.Y=-(weight);
			accelerationVector.Add(gravityVector);


			// we have all the forces added up, now
			force=accelerationVector;
			
		}

		public void Render() 
		{
			measure.Stop();
			time=(float)measure.Time();
			// time=(float)(((int)(measure.Time()*10000))/10000f);
			measure.Start();
			// time=0.05f;
			doPhysics();
			supertime+=time;

			// Setup the world, view, and projection matrices
			SetupMatrices();
			
			gunshots.Render();
		//	myAirplane.Render(m_matOrientation);

			Rectangle rect=new Rectangle(10,20,0,0);
			d3dxfont.DrawText("Tiempo: "+supertime+" Time: "+time, rect, DrawTextFormat.None, Color.AntiqueWhite);
			rect.Y=30;
			d3dxfont.DrawText("Speed: "+(speed*60*60)/1000, rect, DrawTextFormat.None, Color.AntiqueWhite);
			rect.Y=40;
			d3dxfont.DrawText("Height: "+height+" Y: "+m_vPosition.Y+" X: "+m_vPosition.X+" Z: "+m_vPosition.Z, rect, DrawTextFormat.None, Color.AntiqueWhite);
			rect.Y=50;
			d3dxfont.DrawText("Wing: "+wingangleattack*(180/Math.PI)+"  Tail: "+tailangle, rect, DrawTextFormat.None, Color.AntiqueWhite);

			//		d3dx8.DrawText(d3dxfont, unchecked((int)0xFF00FFFF), "GraSpeed: "+gravitySpeed, ref rect, 0);
			rect.Y=60;
			d3dxfont.DrawText("LiftSpeed: "+liftSpeed, rect, DrawTextFormat.None, Color.AntiqueWhite);
			rect.Y=70;
			d3dxfont.DrawText("Brake: "+gearbrake, rect, DrawTextFormat.None, Color.AntiqueWhite);
			//		d3dx8.DrawText(d3dxfont, unchecked((int)0xFF00FFFF), "Distancia: "+m_vVelocity.z/0.03, ref rect, 0);
			rect.Y=80;
			d3dxfont.DrawText("Rotor: "+rotor, rect, DrawTextFormat.None, Color.AntiqueWhite);
		
		}
	}
}
