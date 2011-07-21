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
    /// This class represent the Quality Coefficient. For more information read the following 
    /// Wiki page (http://code.google.com/p/p2p-player/wiki/ImplicitQoS#Coefficiente_di_Qualità)
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
        /// <summary>
        /// Read-Only Property of the Quality Coefficient calculated as a weighted sum of its components.
        /// IMPORTANT: Peer Quality coefficient is deprecated so it is no longer considered as a real compoenent of the
        /// coefficient
        /// </summary>
		public double Coefficient {
			get {
//				return ((0.4*this.pqComponent)+(0.4*this.fqComponent.coefficient)+(0.2*this.affComponent));
                return ((0.6 * this.fqComponent.Coefficient) + (0.4 * this.affComponent));
			}
		}
        /// <summary>
        /// Read-Only Property for the Peer Quality Component of the coefficient
        /// </summary>
		public double peerQuality {
			get {
				return this.pqComponent;
			}
		}
        /// <summary>
        /// Read-Only Property for the File Quality Component of the coefficient
        /// </summary>
		public double fileQuality {
			get {
				return this.fqComponent.Coefficient;
			}
		}
        /// <summary>
        /// Read-Only Property for the Search Affinity Component of the coefficient
        /// </summary>
		public double searchAffinity {
			get {
				return this.affComponent;
			}
		}
		#endregion
		#region Constructors
        /// <summary>
        /// Main Constructor of the Coefficient. This just initializes all the components with the given values.
        /// </summary>
        /// <param name="fq">File Quality Component</param>
        /// <param name="aff">Search Affinity Component</param>
        /// <param name="pq">Peer Quality Component.</param>
		public QualityCoefficient(FileQualityCoefficient fq,double aff=0.0, double pq=0.0) {
			this.affComponent=aff;
			this.pqComponent=pq;
			this.fqComponent=fq;
		}
        /// <summary>
        /// Default Constructor of the Coefficient. This initializes all the components at <c>0.0</c> value.
        /// </summary>
		public QualityCoefficient():this(new FileQualityCoefficient()) {
		}
		#endregion
        /// <summary>
        /// Returns a string representation of the Quality Coefficient and of its components.
        /// </summary>
        /// <returns>The string containing the coefficient representation</returns>
		public override String ToString() {
			StringBuilder sB=new StringBuilder();
			sB.Append("Affinity: ");
			sB.Append(this.affComponent);
			sB.Append("\nPeer Quality: ");
			sB.Append(this.pqComponent);
			sB.Append("\nFile Quality: ");
			sB.Append(this.fqComponent);
			sB.Append("\nQuality Coefficient: ");
			sB.Append(this.Coefficient);
			return sB.ToString();
		}
        /// <summary>
        /// Less-Than comparison operator.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <(QualityCoefficient a, QualityCoefficient b)
        {
            return a < b;
        }
        /// <summary>
        /// Greater-Than comparison operator
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >(QualityCoefficient a, QualityCoefficient b)
        {
            return a > b;
        }
        /// <summary>
        /// Equals comparison operator
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(QualityCoefficient a, QualityCoefficient b)
        {
            return a == b;
        }

        /// <summary>
        /// Not-Equals comparison Operator
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(QualityCoefficient a, QualityCoefficient b)
        {
            return a != b;
        }

        /// <summary>
        /// Method that compare this object with another. If the other object is a QualityCoefficient too, then
        /// the comparison is done by using the Coeffiecient property (which is a double type)
        /// </summary>
        /// <param name="obj">Object to compare with</param>
        /// <returns>
        /// A value less than, greater than or equals to zero if the current object is,  respectevely, 
        /// less than, greater than or equals to the one passed as argument
        /// </returns>
        public int CompareTo(object obj)
        {
            if (obj is QualityCoefficient)
            {
                QualityCoefficient coeff = (QualityCoefficient)obj;
                return this.Coefficient.CompareTo(coeff.Coefficient);
            }
            throw new ArgumentException("object is not a QualityCoefficient");
        }
    }
}