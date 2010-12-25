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
