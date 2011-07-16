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

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Metrics
{
	/// <summary>
	/// Classe rappresentante il coefficiente di qualità.
	/// </summary>
	public class QualityCoefficient : IComparable {
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
//				return ((0.4*this.pqComponent)+(0.4*this.fqComponent.coefficient)+(0.2*this.affComponent));
                return ((0.6 * this.fqComponent.coefficient) + (0.4 * this.affComponent));
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

        public static bool operator <(QualityCoefficient a, QualityCoefficient b)
        {
            return a < b;
        }

        public static bool operator >(QualityCoefficient a, QualityCoefficient b)
        {
            return a > b;
        }

        public static bool operator ==(QualityCoefficient a, QualityCoefficient b)
        {
            return a == b;
        }

        public static bool operator !=(QualityCoefficient a, QualityCoefficient b)
        {
            return a != b;
        }

        public int CompareTo(object obj)
        {
            if (obj is QualityCoefficient)
            {
                QualityCoefficient coeff = (QualityCoefficient)obj;
                return this.coefficient.CompareTo(coeff.coefficient);
            }
            throw new ArgumentException("object is not a QualityCoefficient");
        }
    }
}