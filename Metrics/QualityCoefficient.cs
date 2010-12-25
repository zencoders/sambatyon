using System;
using System.Collections.Generic;

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
		private double fqComponent;
		#endregion
		#region Properties
		public double coefficient {
			get {
				return ((0.4*this.pqComponent)+(0.4*this.fqComponent)+(0.2*this.affComponent));
			}
		}
		public double peerQuality {
			get {
				return this.pqComponent;
			}
		}
		public double fileQuality {
			get {
				return this.fqComponent;
			}
		}
		public double searchAffinity {
			get {
				return this.affComponent;
			}
		}
		#endregion
		#region Constructors
		public QualityCoefficient(double aff=0.0, double pq=0.0, double fq=0.0) {
			this.affComponent=aff;
			this.pqComponent=pq;
			this.fqComponent=fq;
		}
		#endregion
	}
}.