using System;
using System.Collections.Generic;
using System.Text;

namespace Metrics
{
	/// <summary>
	/// Classe rappresentante il coefficiente di qualità.
	/// </summary>
	public class QualityCoefficient {
		#region Attribute
		/// <summary>
		/// Componente che tiene conto di quanto il brano sia pertinente alla ricerca 
		/// fatta dall'utente
		/// </summary>
		private double affComponent;
		/// <summary>
		/// Componente che tiene conto del carico corrente e della distanza sulla DHT 
		/// del peer che deve fornire il brano.
		/// </summary>
		private double pqComponent;
		/// <summary>
		/// Componente che tiene conto della qualità del File contenente il brano 
		/// ( bit-rate del brano, codifica, ecc)
		/// </summary>
		private FileQualityCoefficient fqComponent;
		#endregion
		#region Properties
		public double coefficient {
			get {
				return ((0.4*this.pqComponent)+(0.4*this.fqComponent.coefficient)+(0.2*this.affComponent));
			}
		}
		public double peerQuality {
			get {
				return this.pqComponent;
			}
		}
		public double fileQuality {
			get {
				return this.fqComponent.coefficient;
			}
		}
		public double searchAffinity {
			get {
				return this.affComponent;
			}
		}
		#endregion
		#region Constructors
		public QualityCoefficient(FileQualityCoefficient fq,double aff=0.0, double pq=0.0) {
			this.affComponent=aff;
			this.pqComponent=pq;
			this.fqComponent=fq;
		}
		public QualityCoefficient():this(new FileQualityCoefficient()) {
		}
		#endregion
		public override String ToString() {
			StringBuilder sB=new StringBuilder();
			sB.Append("Affinity: ");
			sB.Append(this.affComponent);
			sB.Append("\nPeer Quality: ");
			sB.Append(this.pqComponent);
			sB.Append("\nFile Quality: ");
			sB.Append(this.fqComponent);
			sB.Append("\nQuality Coefficient: ");
			sB.Append(this.coefficient);
			return sB.ToString();
		}
	}
}