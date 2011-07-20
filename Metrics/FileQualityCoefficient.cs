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
	/// Description of File Quality Coefficient.
	/// </summary>
	public class FileQualityCoefficient {
		#region Attributes        
		private double brComponent;
		private double cmComponent;
		private double srComponent;		
		#endregion
		#region Properties
        /// <summary>
        /// Read-Only Property for the Bit Rate component of the File Quality Coefficient
        /// </summary>
		public double BitRateComponent {
			get {
				return this.brComponent;
			}
		}
        /// <summary>
        /// Read-Only Property for the Channel Mode component of the File Quality Coefficient
        /// </summary>
		public double ChannelModelComponent {
			get {
				return this.cmComponent;
			}
		}
        /// <summary>
        /// Read-Only Property for the Sample Rate component of the File Quality Coefficient
        /// </summary>
		public double SampleRateComponent {
			get {
				return this.srComponent;
			}
		}
        /// <summary>
        /// Read-Only Property for the Coefficient calculated with a weighted sum of each component
        /// </summary>
		public double Coefficient {
			get {
				return (0.5*this.brComponent)+(0.3*cmComponent)+(0.2*srComponent);
			}
		}
		#endregion
		#region Constructors
        /// <summary>
        /// Main Constructor of the coefficient class. This constructor just initializes all the component with the given values.
        /// </summary>
        /// <param name="br">Bit Rate Component</param>
        /// <param name="cm">Channel Mode Component</param>
        /// <param name="sr">Sample Rate Component</param>
		public FileQualityCoefficient(double br=0.0,double cm=0.0,double sr=0.0) {
			this.brComponent=br;
			this.cmComponent=cm;
			this.srComponent=sr;
		}		
		#endregion
        /// <summary>
        /// Returns a string representation of the File Quality Coefficient and of its components.
        /// </summary>
        /// <returns>The string containing the coefficient representation</returns>
		public override string ToString(){
			return string.Format("[FileQualityCoefficient BitRate={0}, ChannelMode={1}, SampleRate={2}, coefficient={3}]",
			                     this.BitRateComponent,this.ChannelModelComponent, this.SampleRateComponent,this.Coefficient);
		}
	}
}
