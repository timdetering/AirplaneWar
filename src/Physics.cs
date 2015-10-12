using System;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace AirplaneWar
{
	/// <summary>
	/// Summary description for Physics.
	/// </summary>
	public class Physics
	{
		public Physics()
		{
		}

		/* Given a surface calculate the area (surface) that is facing
		 * speed/air direction */
		public static float GetContactArea(MyFlatSurface plane, Vector3 airDirection) 
		{
			airDirection.Normalize();
			plane.normal.Normalize();
			float cosangle=Vector3.Dot(airDirection, plane.normal);
			if (Math.Abs(cosangle)<0.017f)
				return 0f;
			return plane.height*plane.width*cosangle;
		}

		public static float GetAirResistance(float dragcoefficient, float airdensity, float speed, float area) 
		{
			return dragcoefficient*airdensity*speed*speed*area/2;
		}
		public static float GetAirResistance(float dragcoefficient, float airdensity, float speed, MyFlatSurface area, Vector3 airdirection) 
		{
			return dragcoefficient*airdensity*speed*speed*GetContactArea(area,airdirection)/2;
		}
	}
}
