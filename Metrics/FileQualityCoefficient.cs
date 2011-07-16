/*****************************************************************************************
 *  p2p-player
 *  An audio player developed in C# based on a shared base to obtain the music from.
 * 
 *  Copyright (C) 2010-2011 Dario Mazza, Sebastiano Merlino
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *  
 *  Dario Mazza (dariomzz@gmail.com)
 *  Sebastiano Merlino (etr@pensieroartificiale.com)
 *  Full Source and Documentation available on Google Code Project "p2p-player", 
 *  see <http://code.google.com/p/p2p-player/>
 *
 ******************************************************************************************/

using System;
using System.CodeDom;

namespace Metrics
{
	/// <summary>
	/// Description of FileQualityCoefficient.
	/// </summary>
	public class FileQualityCoefficient {
		#region Attributes
		private double brComponent;
		private double cmComponent;
		private double srComponent;		
		#endregion
		#region Properties
		public double bitRateComponent {
			get {
				return this.brComponent;
			}
		}
		public double channelModelComponent {
			get {
				return this.cmComponent;
			}
		}
		public double sampleRateComponent {
			get {
				return this.srComponent;
			}
		}
		public double coefficient {
			get {
				return (0.5*this.brComponent)+(0.3*cmComponent)+(0.2*srComponent);
			}
		}
		#endregion
		#region Constructors
		public FileQualityCoefficient(double br=0.0,double cm=0.0,double sr=0.0) {
			this.brComponent=br;
			this.cmComponent=cm;
			this.srComponent=sr;
		}		
		#endregion
		public override string ToString(){
			return string.Format("[FileQualityCoefficient BitRate={0}, ChannelMode={1}, SampleRate={2}, coefficient={3}]",
			                     this.bitRateComponent,this.channelModelComponent, this.sampleRateComponent,this.coefficient);
		}
	}
}
