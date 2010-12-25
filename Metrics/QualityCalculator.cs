using System;
using System.Collections;
using System.Collections.Generic;
using TagLib;
using TagLib.Mpeg;

namespace Metrics{
	/// <summary>
	/// Description of QualityCalculator.
	/// </summary>
	public static class QualityCalculator {
		#region Private Methods
		private static double calculateAFF(String searchString,String[] tags) {
			int[] concatenazioni=new int[tags.Length];
			for (int k=0;k<concatenazioni.Length;k++) {
				concatenazioni[k]=0;				
			}
			LinkedList<double> valori=new LinkedList<double>();
			String[] ricerca=searchString.ToLower().Split(' ');			
			for (int i=0;i<ricerca.Length;i++) {
				String parola=ricerca[i];
				double max=-1.0;
				for(int k=0;k<tags.Length;k++) {
					String etichetta_no_spazi=tags[k].ToLower().Replace(" ","");
					if (etichetta_no_spazi.IndexOf(parola)!=-1) {
						concatenazioni[k]+=parola.Length;
						double valore_corrente=parola.Length/((double)etichetta_no_spazi.Length);
						if (valore_corrente>max) {
							max=valore_corrente;
						}
					} else {
						if ((i!=0)&&(concatenazioni[k]!=ricerca[i-1].Length)&&(concatenazioni[k]!=0)) {
							double concatenazioneComponent=concatenazioni[k]/((double)etichetta_no_spazi.Length);
							valori.AddLast(concatenazioneComponent);
						}
						concatenazioni[k]=0;
					}
				}
				valori.AddLast(max);				
			}
			for (int k=0;k<concatenazioni.Length;k++) {
				if ((concatenazioni[k]!=ricerca[ricerca.Length-1].Length)&&(concatenazioni[k]!=0)) {
					double concatenazioneComponent=concatenazioni[k]/((double)tags[k].ToLower().Replace(" ","").Length);
					valori.AddLast(concatenazioneComponent);
				}
			}
			double peso=1/((double)ricerca.Length);			
			double aff=0.0;
			foreach (double val in valori) {				
				aff+=(peso*val);
			}
			return aff;
		}
		private static FileQualityCoefficient calculateFQ(String filepath) {
			AudioFile mpegFile= new AudioFile(filepath);					
			AudioHeader header;
			if (AudioHeader.Find(out header,mpegFile,0)) {		
				double brComp=Math.Truncate((Math.Log10(header.AudioBitrate/192.0)+0.78)*100.0)/100.0;
				double srComp=0.0;
				switch (header.AudioSampleRate) {
					case 32000: 
					srComp=0.2;
					break;
				case 44100:
					srComp=0.8;
					break;
				case 48000:
					srComp=1.0;
					break;
				default:
					srComp=0.0;
					break;										
				}
				double cmComp=0.0;
				switch (header.ChannelMode) {
					case ChannelMode.SingleChannel:
						cmComp=0.2;
						break;
					case ChannelMode.DualChannel:
						cmComp=0.5;
						break;
					case ChannelMode.JointStereo:
						cmComp=0.8;
						break;
					case ChannelMode.Stereo:
						cmComp=1.0;
						break;
					default:
						cmComp=0.0;
						break;
				}
				return new FileQualityCoefficient(brComp,cmComp,srComp);
			} else {
				return new FileQualityCoefficient();			
			}
		}
		private static double calculatePQ(int buffered,int queueSize) {
			return (buffered/((double)queueSize));
		}		
		#endregion
		#region Public Methods
		public static QualityCoefficient calculateSearchAffinity(String searchString,String[] tags) {
			return new QualityCoefficient(new FileQualityCoefficient(),calculateAFF(searchString,tags));
		}
		public static QualityCoefficient calculatePeerQuality(int buffered, int queueSize) {						
			return new QualityCoefficient(new FileQualityCoefficient(),0.0,calculatePQ(buffered,queueSize));
		}
		public static QualityCoefficient calculateFileQuality(String filepath) {			
			return new QualityCoefficient(calculateFQ(filepath));
		}		
		public static QualityCoefficient calculateQualityCoefficient(String searchString,String[] tags,int buffered,int queueSize,String filepath) {
			return new QualityCoefficient(calculateFQ(filepath),calculateAFF(searchString,tags),calculatePQ(buffered,queueSize));
		}
		#endregion
	}
}
