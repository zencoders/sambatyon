/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/22/2010
 * Time: 11:15 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace Daylight.Messages
{
	/// <summary>
	/// Send data in reply to a FindVAlue
	/// </summary>
	[Serializable]
	public class FindValueDataResponse : Response
	{
		private IList<string> vals;
		
		/// <summary>
		/// Make a new response.
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="request"></param>
		/// <param name="data"></param>
		public FindValueDataResponse(ID nodeID, FindValue request, IList<string> data) : base(nodeID, request)
		{
			vals = data;
		}
		
		/// <summary>
		/// Get the values returned for the key
		/// </summary>
		/// <returns></returns>
		public IList<string> GetValues()
		{
			return vals;
		}
		
		public override string GetName()
		{
			return "FIND_VALUE_RESPONSE_DATA";
		}
	}
}
